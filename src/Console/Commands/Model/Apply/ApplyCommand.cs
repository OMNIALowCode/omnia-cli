using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Commands.Model.Apply.Data.Server;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Commands.Model.Apply.Readers.Server;
using Omnia.CLI.Commands.Model.Apply.Readers.UI;
using Omnia.CLI.Commands.Model.Extensions;
using Omnia.CLI.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.IO.Path;

namespace Omnia.CLI.Commands.Model.Apply
{
    [Command(Name = "apply", Description = "Apply source code to model.")]
    [HelpOption("-h|--help")]
    public class ApplyCommand
    {
        private readonly AppSettings _settings;
        private readonly IApiClient _apiClient;
        private readonly DefinitionApplyService _definitionService;
        private readonly ApplicationBehaviourApplyService _applicationBehaviourApplyService;
        private readonly WebComponentApplyService _webComponentApplyService;
        private readonly UIBehavioursApplyService _uiBehavioursApplyService;
        private readonly UIEntityBehaviourReader _uiEntityBehaviourReader = new UIEntityBehaviourReader();
        private readonly ThemeApplyService _themeApplyService;
        private readonly EntityBehaviourReader _entityBehaviourReader = new EntityBehaviourReader();
        private readonly ApplicationBehaviourReader _applicationReader = new ApplicationBehaviourReader();
        private readonly DaoReader _daoReader = new DaoReader();
        private readonly DependencyReader _dependencyReader = new DependencyReader();
        private readonly StateReader _stateReader = new StateReader();
        private readonly WebComponentReader _webComponentReader = new WebComponentReader();
        private readonly ThemeReader _themeReader = new ThemeReader();
        public ApplyCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _settings = options.Value;
            _apiClient = apiClient;
            _definitionService = new DefinitionApplyService(_apiClient);
            _webComponentApplyService = new WebComponentApplyService(_apiClient);
            _uiBehavioursApplyService = new UIBehavioursApplyService(_apiClient);
            _themeApplyService = new ThemeApplyService(_apiClient);
            _applicationBehaviourApplyService = new ApplicationBehaviourApplyService(_apiClient);
        }

        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }
        [Option("--tenant", CommandOptionType.SingleValue, Description = "Tenant to import.")]
        public string Tenant { get; set; }
        [Option("--environment", CommandOptionType.SingleValue, Description = "Environment to import.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;
        [Option("--path", CommandOptionType.SingleValue, Description = "Complete path to the source code directory.")]
        public string Path { get; set; } = ".";
        [Option("--build", CommandOptionType.NoValue, Description = "Perform a model build after applying.")]
        public bool Build { get; set; }

        public async Task<int> OnExecute(CommandLineApplication cmd)
        {
            if (string.IsNullOrEmpty(Path))
            {
                Console.WriteLine($"{nameof(Path)} is required");
                return (int)StatusCodes.InvalidArgument;
            }

            if (!Directory.Exists(Path))
            {
                Console.WriteLine($"The value of --path parameters \"{Path}\" is not a valid directory.");
                return (int)StatusCodes.InvalidArgument;
            }

            var sourceSettings = _settings.GetSubscription(Subscription);

            await _apiClient.Authenticate(sourceSettings).ConfigureAwait(false);

            IEnumerable<Task<(string name, Entity entity)>> processFileTasks =
                ProcessEntityBehaviours().Union(
                    ProcessDataBehaviours()
                );

            var entities = await Task.WhenAll(processFileTasks).ConfigureAwait(false);

            var applicationBehaviours = await Task.WhenAll(ProcessApplicationBehaviours()).ConfigureAwait(false);

            var stateMachines = await Task.WhenAll(ProcessStates()).ConfigureAwait(false);

            var webComponents = await Task.WhenAll(ProcessWebComponents()).ConfigureAwait(false);

            var uiBehaviours = await Task.WhenAll(ProcessUIBehaviours()).ConfigureAwait(false);
            var themes = await Task.WhenAll(ProcessThemes()).ConfigureAwait(false);

            var tasks = entities.GroupBy(g => g.name)
                .Select(g =>
                    ApplyEntityChanges(g.Key,
                        new Entity(g.First().entity.Namespace,
                        g.SelectMany(e => e.entity?.EntityBehaviours).ToList(),
                        g.SelectMany(e => e.entity?.DataBehaviours).ToList(),
                        g.SelectMany(e => e.entity?.Usings).ToList())
                    )
                ).ToList();

            tasks.AddRange(applicationBehaviours
                .Select(g =>
                    ApplyApplicationBehaviourChanges(g.name, g.entity)
                ));

            tasks.AddRange(stateMachines
                .Select(st =>
                    ApplyStateMachineChanges(st.name, st.entity)
                ));

            tasks.AddRange(webComponents
              .Select(g =>
                  ApplyWebComponentChanges(g.name, g.entity)
              ));

            tasks.AddRange(uiBehaviours
          .Select(g =>
              ApplyUIBehavioursChanges(g.name, g.entity)
          ));
            tasks.AddRange(themes
              .Select(g =>
                  ApplyThemeChanges(g.name, g.entity)
              ));


            await Task.WhenAll(tasks).ConfigureAwait(false);

            var codeDependencies = await ProcessCodeDependencies().ConfigureAwait(false);
            var fileDependencies = await ProcessFileDependencies().ConfigureAwait(false);
            await ApplyDependenciesChanges(codeDependencies, fileDependencies).ConfigureAwait(false);

            if (Build)
                await _apiClient.BuildModel(Tenant, Environment).ConfigureAwait(false);


            Console.WriteLine($"Successfully applied to tenant \"{Tenant}\" model.");
            return (int)StatusCodes.Success;
        }

        private IEnumerable<Task<(string name, List<State> entity)>> ProcessStates()
        {
            var files = Directory.GetFiles(Path, "*.StateMachine.cs", SearchOption.AllDirectories);

            return files.Select(ProcessStateMachineFile);
        }

        private IEnumerable<Task<(string name, ApplicationBehaviour entity)>> ProcessApplicationBehaviours()
        {
            Regex reg = new Regex(@"\\Application\\[^\\]+\.cs$");
            var files = Directory.GetFiles(Path, "*.cs", SearchOption.AllDirectories)
                .Where(path => reg.IsMatch(path))
                .ToList();

            return files.Select(ProcessApplicationBehavioursFile);
        }

        private IEnumerable<Task<(string name, Entity entity)>> ProcessEntityBehaviours()
        {
            var files = Directory.GetFiles(Path, "*.Operations.cs", SearchOption.AllDirectories);

            return files.Select(ProcessEntityBehavioursFile);
        }

        private IEnumerable<Task<(string name, UIEntity entity)>> ProcessUIBehaviours()
        {
            var uiBehavioursPathRegex = new Regex(@"\\Behaviours\\[^\\]+\.js$");
            var files = Directory.GetFiles(Path, "*.js", SearchOption.AllDirectories)
                .Where(path => uiBehavioursPathRegex.IsMatch(path))
                .ToList();

            return files.Select(ProcessUIBehavioursFile);
        }

        private IEnumerable<Task<(string name, Entity entity)>> ProcessDataBehaviours()
        {
            var files = Directory.GetFiles(Path, "*Dao.cs", SearchOption.AllDirectories);

            return files.Select(ProcessDaoFile);
        }

        private async Task<Dictionary<string, IDictionary<string, CodeDependency>>> ProcessCodeDependencies()
        {
            var dependencies = new Dictionary<string, IDictionary<string, CodeDependency>>();

            foreach (string directory in Directory.GetDirectories(Path, "CodeDependencies", SearchOption.AllDirectories))
            {
                var files = Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly);

                var dependencyData = await Task.WhenAll(files.Select(ProcessCodeDependencyFile)).ConfigureAwait(false);

                foreach (var (name, codeDependency) in dependencyData)
                {
                    var dataSource = codeDependency.Namespace.Split('.')[4];
                    if (!dependencies.ContainsKey(dataSource))
                        dependencies.Add(dataSource, new Dictionary<string, CodeDependency>());
                    dependencies[dataSource].Add(name, codeDependency);
                }
            }
            return dependencies;
        }

        private async Task<IDictionary<string, IList<FileDependency>>> ProcessFileDependencies()
        {
            var dependencies = new Dictionary<string, IList<FileDependency>>();

            var files = Directory.GetFiles(Path, "*.csproj", SearchOption.AllDirectories);

            var dependencyData = await Task.WhenAll(files.Select(ProcessFileDependencyFile)).ConfigureAwait(false);

            foreach (var (name, fileDependencies) in dependencyData)
                dependencies.Add(name, fileDependencies);

            return dependencies;
        }

        private IEnumerable<Task<(string name, WebComponent entity)>> ProcessWebComponents()
        {
            var webComponentPathRegex = new Regex(@"\\WebComponents\\[^\\]+\\index\.js$");
            var files = Directory.GetFiles(Path, "index.js", SearchOption.AllDirectories)
                .Where(path => webComponentPathRegex.IsMatch(path))
                .ToList();

            return files.Select(ProcessWebComponentFile);
        }

        private IEnumerable<Task<(string name, Theme entity)>> ProcessThemes()
        {
            var themePathRegex = new Regex(@"\\Themes\\[^\\]+\\variables\.scss$");
            var files = Directory.GetFiles(Path, "variables.scss", SearchOption.AllDirectories)
                .Where(path => themePathRegex.IsMatch(path))
                .ToList();

            return files.Select(ProcessThemeFile);
        }

        private async Task<(string name, Entity entity)> ProcessEntityBehavioursFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (ExtractEntityNameFromFileName(filepath, ".Operations.cs"), _entityBehaviourReader.ExtractData(content));
        }

        private async Task<(string name, ApplicationBehaviour entity)> ProcessApplicationBehavioursFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (ExtractEntityNameFromFileName(filepath, ".cs"), _applicationReader.ExtractData(content));
        }

        private async Task<(string name, List<State> entity)> ProcessStateMachineFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return ($"{ExtractEntityNameFromFileName(filepath, ".StateMachine.cs")}StateMachine", _stateReader.ExtractData(content).ToList());
        }

        private async Task<(string name, Entity entity)> ProcessDaoFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (ExtractEntityNameFromFileName(filepath, "Dao.cs"), _daoReader.ExtractData(content));
        }

        private async Task<(string name, CodeDependency codeDependency)> ProcessCodeDependencyFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (ExtractEntityNameFromFileName(filepath, ".cs"), _dependencyReader.ExtractCodeDependencies(content));
        }

        private async Task<(string name, IList<FileDependency> fileDependencies)> ProcessFileDependencyFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (ExtractEntityNameFromFileName(filepath, ".csproj"), _dependencyReader.ExtractFileDependencies(content));
        }

        private async Task<(string name, WebComponent entity)> ProcessWebComponentFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (Name(), _webComponentReader.ExtractData(content));

            string Name()
                => GetFileName(GetDirectoryName(filepath));
        }


        private async Task<(string name, UIEntity entity)> ProcessUIBehavioursFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (ExtractEntityNameFromFileName(filepath, ".js"), _uiEntityBehaviourReader.ExtractData(content));
        }

        private async Task<(string name, Theme entity)> ProcessThemeFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (Name(), _themeReader.ExtractData(content));

            string Name()
                => GetFileName(GetDirectoryName(filepath));
        }

        private async Task ApplyDependenciesChanges(IDictionary<string, IDictionary<string, CodeDependency>> dataSourceCodeDependencies, IDictionary<string, IList<FileDependency>> fileDependencies)
        {
            var dataPerDataSource = new Dictionary<string, (IDictionary<string, CodeDependency> codeDependencies, IList<(string, FileDependency)> fileDependencies)>();

            foreach (var dataSourceDependency in dataSourceCodeDependencies)
            {
                var dataSource = dataSourceDependency.Key;
                dataPerDataSource.Add(dataSource, (dataSourceDependency.Value, new List<(string, FileDependency)>()));
            }

            foreach (var fileDependencyData in fileDependencies)
            {
                var nameParts = fileDependencyData.Key.Split('.');
                var dataSource = nameParts[2];
                var location = nameParts[1];

                if (!dataPerDataSource.ContainsKey(dataSource))
                    dataPerDataSource.Add(dataSource, (new Dictionary<string, CodeDependency>(), new List<(string, FileDependency)>()));

                foreach (var fileDependency in fileDependencyData.Value)
                    dataPerDataSource[dataSource].fileDependencies.Add((location, fileDependency));
            }

            foreach (var dataSource in dataPerDataSource)
            {
                var applySuccessfully = await _definitionService.ReplaceDependencies(Tenant, Environment,
                    dataSource.Key,
                            dataSource.Value.codeDependencies,
                            dataSource.Value.fileDependencies
                            ).ConfigureAwait(false);

                if (!applySuccessfully)
                    Console.WriteLine($"Failed to apply dependencies to Data Source {dataSource.Key}.");
            }
        }

        private async Task ApplyEntityChanges(string name, Entity entity)
        {
            if (entity.EntityBehaviours?.Count == 0 && entity.DataBehaviours?.Count == 0 && entity.Usings?.Count == 0)
                return;

            var applySuccessfully = await ReplaceData(name, entity).ConfigureAwait(false);
            if (!applySuccessfully)
                Console.WriteLine($"Failed to apply behaviours to entity {name}.");
        }

        private async Task ApplyApplicationBehaviourChanges(string name, ApplicationBehaviour entity)
        {
            var applySuccessfully = await ReplaceApplicationBehaviourData(name, entity).ConfigureAwait(false);
            if (!applySuccessfully)
                Console.WriteLine($"Failed to apply application behaviour {name}.");
        }

        private async Task ApplyStateMachineChanges(string name, List<State> entity)
        {
            var applySuccessfully = await ReplaceStateMachineData(name, entity).ConfigureAwait(false);
            if (!applySuccessfully)
                Console.WriteLine($"Failed to apply states to entity {name}.");
        }

        private async Task ApplyWebComponentChanges(string name, WebComponent entity)
        {
            var applySuccessfully = await _webComponentApplyService.ReplaceData(Tenant, Environment,
                            name, entity).ConfigureAwait(false);

            if (!applySuccessfully)
                Console.WriteLine($"Failed to apply WebComponent {name}.");
        }

        private async Task ApplyUIBehavioursChanges(string name, UIEntity entity)
        {
            var applySuccessfully = await _uiBehavioursApplyService.ReplaceData(Tenant, Environment,
                            ExtractEntityNameFromFileName(name, string.Empty), entity).ConfigureAwait(false);

            if (!applySuccessfully)
                Console.WriteLine($"Failed to apply WebComponent {name}.");
        }

        private async Task ApplyThemeChanges(string name, Theme entity)
        {
            var applySuccessfully = await _themeApplyService.ReplaceData(Tenant, Environment,
                            name, entity).ConfigureAwait(false);

            if (!applySuccessfully)
                Console.WriteLine($"Failed to apply Theme {name}.");
        }

        private async Task<bool> ReplaceApplicationBehaviourData(string filepath, ApplicationBehaviour entity)
        => await _applicationBehaviourApplyService.ReplaceData(Tenant, Environment,
                            ExtractEntityNameFromFileName(filepath, string.Empty), entity).ConfigureAwait(false);

        private async Task<bool> ReplaceData(string name, Entity entity)
            => await _definitionService.ReplaceData(Tenant, Environment,
                            name, entity).ConfigureAwait(false);

        private async Task<bool> ReplaceStateMachineData(string name, List<State> entity)
            => await _definitionService.ReplaceStateData(Tenant, Environment, name, entity).ConfigureAwait(false);

        private static string ExtractEntityNameFromFileName(string filepath, string suffix)
        {
            var filename = GetFileName(filepath);
            return filename.Substring(0, filename.Length - suffix.Length);
        }

        private static async Task<string> ReadFile(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var sr = new StreamReader(fs);
            return await sr.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
