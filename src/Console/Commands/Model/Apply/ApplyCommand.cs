using Microsoft.Extensions.Options;
using Omnia.CLI.Commands.Model.Apply.Data.Database;
using Omnia.CLI.Commands.Model.Apply.Data.Server;
using Omnia.CLI.Commands.Model.Apply.Data.UI;
using Omnia.CLI.Commands.Model.Apply.Readers.Database;
using Omnia.CLI.Commands.Model.Apply.Readers.Server;
using Omnia.CLI.Commands.Model.Apply.Readers.UI;
using Omnia.CLI.Commands.Model.Extensions;
using Omnia.CLI.Infrastructure;
using Spectre.Cli;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.IO.Path;

namespace Omnia.CLI.Commands.Model.Apply
{
    [Description("Apply source code to model.")]
    public sealed class ApplyCommand : AsyncCommand<ApplyCommandSettings>
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
        private readonly QueryApplyService _queryApplyService;
        private readonly QueryReader _queryReader = new QueryReader();


        public ApplyCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _settings = options.Value;
            _apiClient = apiClient;
            _definitionService = new DefinitionApplyService(_apiClient);
            _webComponentApplyService = new WebComponentApplyService(_apiClient);
            _uiBehavioursApplyService = new UIBehavioursApplyService(_apiClient);
            _themeApplyService = new ThemeApplyService(_apiClient);
            _applicationBehaviourApplyService = new ApplicationBehaviourApplyService(_apiClient);
            _queryApplyService = new QueryApplyService(_apiClient);
        }

        public override Spectre.Cli.ValidationResult Validate(CommandContext context, ApplyCommandSettings settings)
        {
            if (string.IsNullOrEmpty(settings.Path))
            {
                return Spectre.Cli.ValidationResult.Error($"{nameof(settings.Path)} is required");
            }

            if (!Directory.Exists(settings.Path))
            {
                return Spectre.Cli.ValidationResult.Error($"The value of --path parameters \"{settings.Path}\" is not a valid directory.");
            }
            return base.Validate(context, settings);
        }

        public override async Task<int> ExecuteAsync(CommandContext context, ApplyCommandSettings settings)
        {
            var sourceSettings = _settings.GetSubscription(settings.Subscription);

            await _apiClient.Authenticate(sourceSettings).ConfigureAwait(false);

            IEnumerable<Task<(string name, Entity entity)>> processFileTasks =
                ProcessEntityBehaviours(settings.Path).Union(
                    ProcessDataBehaviours(settings.Path)
                );

            var entities = await Task.WhenAll(processFileTasks).ConfigureAwait(false);

            var applicationBehaviours = await Task.WhenAll(ProcessApplicationBehaviours(settings.Path)).ConfigureAwait(false);

            var stateMachines = await Task.WhenAll(ProcessStates(settings.Path)).ConfigureAwait(false);

            var webComponents = await Task.WhenAll(ProcessWebComponents(settings.Path)).ConfigureAwait(false);

            var uiBehaviours = await Task.WhenAll(ProcessUIBehaviours(settings.Path)).ConfigureAwait(false);
            var themes = await Task.WhenAll(ProcessThemes(settings.Path)).ConfigureAwait(false);

            var queries = await Task.WhenAll(ProcessQueries(settings.Path)).ConfigureAwait(false);

            var tasks = entities.GroupBy(g => g.name)
                .Select(g =>
                    ApplyEntityChanges(settings.Tenant, settings.Environment, g.Key,
                        new Entity(g.First().entity.Namespace,
                        g.SelectMany(e => e.entity?.EntityBehaviours).ToList(),
                        g.SelectMany(e => e.entity?.DataBehaviours).ToList(),
                        g.SelectMany(e => e.entity?.Usings).ToList())
                    )
                ).ToList();

            tasks.AddRange(applicationBehaviours
                .Select(g =>
                    ApplyApplicationBehaviourChanges(settings.Tenant, settings.Environment, g.name, g.entity)
                ));

            tasks.AddRange(stateMachines
                .Select(st =>
                    ApplyStateMachineChanges(settings.Tenant, settings.Environment, st.name, st.entity)
                ));

            tasks.AddRange(webComponents
              .Select(g =>
                  ApplyWebComponentChanges(settings.Tenant, settings.Environment, g.name, g.entity)
              ));

            tasks.AddRange(uiBehaviours
          .Select(g =>
              ApplyUIBehavioursChanges(settings.Tenant, settings.Environment, g.name, g.entity)
          ));
            tasks.AddRange(themes
              .Select(g =>
                  ApplyThemeChanges(settings.Tenant, settings.Environment, g.name, g.entity)
              ));

            tasks.AddRange(queries
              .Select(g =>
                  ApplyQueryChanges(settings.Tenant, settings.Environment, g.name, g.entity)
              ));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var codeDependencies = await ProcessCodeDependencies(settings.Path).ConfigureAwait(false);
            var fileDependencies = await ProcessFileDependencies(settings.Path).ConfigureAwait(false);
            await ApplyDependenciesChanges(settings.Tenant, settings.Environment, codeDependencies, fileDependencies).ConfigureAwait(false);

            if (settings.Build)
                await _apiClient.BuildModel(settings.Tenant, settings.Environment).ConfigureAwait(false);

            AnsiConsole.MarkupLine($"[green]Successfully applied to tenant \"{settings.Tenant}\" model.[/]");
            return (int)StatusCodes.Success;
        }

        private IEnumerable<Task<(string name, List<State> entity)>> ProcessStates(string path)
        {
            var files = Directory.GetFiles(path, "*.StateMachine.cs", SearchOption.AllDirectories);

            return files.Select(ProcessStateMachineFile);
        }

        private IEnumerable<Task<(string name, ApplicationBehaviour entity)>> ProcessApplicationBehaviours(string path)
        {
            var slashCharacter = SettingsPathFactory.OperationSystemPathSlash();
            Regex reg = new Regex(@$"\{slashCharacter}Application\{slashCharacter}[^\{slashCharacter}]+\.cs$");
            var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories)
                .Where(path => reg.IsMatch(path))
                .ToList();

            return files.Select(ProcessApplicationBehavioursFile);
        }

        private IEnumerable<Task<(string name, Entity entity)>> ProcessEntityBehaviours(string path)
        {
            var files = Directory.GetFiles(path, "*.Operations.cs", SearchOption.AllDirectories);

            return files.Select(ProcessEntityBehavioursFile);
        }

        private IEnumerable<Task<(string name, UIEntity entity)>> ProcessUIBehaviours(string path)
        {
            var slashCharacter = SettingsPathFactory.OperationSystemPathSlash();
            Regex uiBehavioursPathRegex = new Regex(@$"\{slashCharacter}Behaviours\{slashCharacter}[^\{slashCharacter}]+\.js$");
            var files = Directory.GetFiles(path, "*.js", SearchOption.AllDirectories)
                .Where(path => uiBehavioursPathRegex.IsMatch(path))
                .ToList();

            return files.Select(ProcessUIBehavioursFile);
        }

        private IEnumerable<Task<(string name, Entity entity)>> ProcessDataBehaviours(string path)
        {
            var files = Directory.GetFiles(path, "*Dao.cs", SearchOption.AllDirectories);

            return files.Select(ProcessDaoFile);
        }

        private async Task<Dictionary<string, IDictionary<string, CodeDependency>>> ProcessCodeDependencies(string path)
        {
            var dependencies = new Dictionary<string, IDictionary<string, CodeDependency>>();

            foreach (string directory in Directory.GetDirectories(path, "CodeDependencies", SearchOption.AllDirectories))
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

        private async Task<IDictionary<string, IList<FileDependency>>> ProcessFileDependencies(string path)
        {
            var dependencies = new Dictionary<string, IList<FileDependency>>();

            var files = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);

            var dependencyData = await Task.WhenAll(files.Select(ProcessFileDependencyFile)).ConfigureAwait(false);

            foreach (var (name, fileDependencies) in dependencyData)
                dependencies.Add(name, fileDependencies);

            return dependencies;
        }

        private IEnumerable<Task<(string name, WebComponent entity)>> ProcessWebComponents(string path)
        {
            var slashCharacter = SettingsPathFactory.OperationSystemPathSlash();
            var webComponentPathRegex = new Regex(@$"\{slashCharacter}WebComponents\{slashCharacter}[^\{slashCharacter}]+\{slashCharacter}index\.js$");
            var files = Directory.GetFiles(path, "index.js", SearchOption.AllDirectories)
                .Where(path => webComponentPathRegex.IsMatch(path))
                .ToList();

            return files.Select(ProcessWebComponentFile);
        }

        private IEnumerable<Task<(string name, Theme entity)>> ProcessThemes(string path)
        {
            var slashCharacter = SettingsPathFactory.OperationSystemPathSlash();
            var themePathRegex = new Regex(@$"\{slashCharacter}Themes\{slashCharacter}[^\{slashCharacter}]+\{slashCharacter}variables\.scss$");
            var files = Directory.GetFiles(path, "variables.scss", SearchOption.AllDirectories)
                .Where(path => themePathRegex.IsMatch(path))
                .ToList();

            return files.Select(ProcessThemeFile);
        }

        private IEnumerable<Task<(string name, Query entity)>> ProcessQueries(string path)
        {
            var slashCharacter = SettingsPathFactory.OperationSystemPathSlash();
            var files = Directory.GetFiles(path, "*.sql", SearchOption.AllDirectories)
                .ToList();

            return files.Select(ProcessQueryFile);
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

        private async Task<(string name, Query entity)> ProcessQueryFile(string filepath)
        {
            Console.WriteLine($"Processing file {filepath}...");
            var content = await ReadFile(filepath).ConfigureAwait(false);

            return (Name(), _queryReader.ExtractData(content));

            string Name()
                => GetFileName(GetFileNameWithoutExtension(filepath));
        }


        private async Task ApplyDependenciesChanges(
            string tenant,
            string environment,
            IDictionary<string, IDictionary<string, CodeDependency>> dataSourceCodeDependencies,
            IDictionary<string, IList<FileDependency>> fileDependencies)
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
                var applySuccessfully = await _definitionService.ReplaceDependencies(tenant, environment,
                    dataSource.Key,
                            dataSource.Value.codeDependencies,
                            dataSource.Value.fileDependencies
                            ).ConfigureAwait(false);

                if (!applySuccessfully)
                    AnsiConsole.MarkupLine($"[red]Failed to apply dependencies to Data Source {dataSource.Key}.[/]");
            }
        }

        private async Task ApplyEntityChanges(
            string tenant,
            string environment,
            string name, Entity entity)
        {
            if (entity.EntityBehaviours?.Count == 0 && entity.DataBehaviours?.Count == 0 && entity.Usings?.Count == 0)
                return;

            var applySuccessfully = await ReplaceData(tenant, environment, name, entity).ConfigureAwait(false);
            if (!applySuccessfully)
                AnsiConsole.MarkupLine($"[red]Failed to apply behaviours to entity {name}.[/]");
        }

        private async Task ApplyApplicationBehaviourChanges(
            string tenant,
            string environment,
            string name, ApplicationBehaviour entity)
        {
            var applySuccessfully = await ReplaceApplicationBehaviourData(tenant, environment, name, entity).ConfigureAwait(false);
            if (!applySuccessfully)
                AnsiConsole.MarkupLine($"[red]Failed to apply application behaviour {name}.[/]");
        }

        private async Task ApplyStateMachineChanges(
            string tenant,
            string environment,
            string name, List<State> entity)
        {
            var applySuccessfully = await ReplaceStateMachineData(tenant, environment, name, entity).ConfigureAwait(false);
            if (!applySuccessfully)
                AnsiConsole.MarkupLine($"[red]Failed to apply states to entity {name}.[/]");
        }

        private async Task ApplyWebComponentChanges(
                        string tenant,
            string environment,
            string name, WebComponent entity)
        {
            var applySuccessfully = await _webComponentApplyService.ReplaceData(tenant, environment,
                            name, entity).ConfigureAwait(false);

            if (!applySuccessfully)
                AnsiConsole.MarkupLine($"[red]Failed to apply WebComponent {name}.[/]");
        }

        private async Task ApplyUIBehavioursChanges(
            string tenant,
            string environment,
            string name, UIEntity entity)
        {
            var entityName = ExtractEntityNameFromFileName(name, string.Empty);
            var applySuccessfully = await _uiBehavioursApplyService.ReplaceData(tenant, environment,
                            entityName, entity).ConfigureAwait(false);

            if (!applySuccessfully)
                AnsiConsole.MarkupLine($"[red]Failed to apply Behaviours to entity {entityName}.[/]");
        }

        private async Task ApplyThemeChanges(
                        string tenant,
            string environment,
            string name, Theme entity)
        {
            var applySuccessfully = await _themeApplyService.ReplaceData(tenant, environment,
                            name, entity).ConfigureAwait(false);

            if (!applySuccessfully)
                AnsiConsole.MarkupLine($"[red]Failed to apply Theme {name}.[/]");
        }

        private async Task ApplyQueryChanges(
                        string tenant,
            string environment,
            string name, Query entity)
        {
            var applySuccessfully = await _queryApplyService.ReplaceData(tenant, environment,
                            name, entity).ConfigureAwait(false);

            if (!applySuccessfully)
                AnsiConsole.MarkupLine($"[red]Failed to apply query {name}.[/]");
        }


        private async Task<bool> ReplaceApplicationBehaviourData(
            string tenant,
            string environment,
            string filepath, ApplicationBehaviour entity)
            => await _applicationBehaviourApplyService.ReplaceData(tenant, environment,
                            ExtractEntityNameFromFileName(filepath, string.Empty), entity).ConfigureAwait(false);

        private async Task<bool> ReplaceData(
            string tenant,
            string environment,
            string name, Entity entity)
            => await _definitionService.ReplaceData(tenant, environment,
                            name, entity).ConfigureAwait(false);

        private async Task<bool> ReplaceStateMachineData(
            string tenant,
            string environment,
            string name, List<State> entity)
            => await _definitionService.ReplaceStateData(tenant, environment, name, entity).ConfigureAwait(false);

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
