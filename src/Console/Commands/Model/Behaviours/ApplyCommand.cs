﻿using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Omnia.CLI.Commands.Model.Behaviours.Data;
using Omnia.CLI.Commands.Model.Behaviours.Readers;
using Omnia.CLI.Commands.Model.Extensions;
using Omnia.CLI.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model.Behaviours
{
    [Command(Name = "apply", Description = "Apply behaviours to model from source code.")]
    [HelpOption("-h|--help")]
    public class ApplyCommand
    {
        private readonly AppSettings _settings;
        private readonly IApiClient _apiClient;
        private readonly DefinitionService _definitionService;
        private readonly EntityBehaviourReader _entityBehaviourReader = new EntityBehaviourReader();
        private readonly ApplicationBehaviourReader _applicationReader = new ApplicationBehaviourReader();
        private readonly DaoReader _daoReader = new DaoReader();
        private readonly DependencyReader _dependencyReader = new DependencyReader();
        private readonly StateReader _stateReader = new StateReader();
        public ApplyCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _settings = options.Value;
            _apiClient = apiClient;
            _definitionService = new DefinitionService(_apiClient);
        }

        [Option("--subscription", CommandOptionType.SingleValue, Description = "Name of the configured subscription.")]
        public string Subscription { get; set; }
        [Option("--tenant", CommandOptionType.SingleValue, Description = "Tenant to export.")]
        public string Tenant { get; set; }
        [Option("--environment", CommandOptionType.SingleValue, Description = "Environment to import.")]
        public string Environment { get; set; } = Constants.DefaultEnvironment;
        [Option("--path", CommandOptionType.SingleValue, Description = "Complete path to the ZIP file.")]
        public string Path { get; set; }
        [Option("--build", CommandOptionType.NoValue, Description = "Perform a model build after the importation.")]
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

            IEnumerable<Task<(string name, ApplicationBehaviour entity)>> processApplicationBehaviourFileTasks =
                ProcessApplicationBehaviours();

            var applicationBehaviours = await Task.WhenAll(processApplicationBehaviourFileTasks).ConfigureAwait(false);

            IEnumerable<Task<(string name, List<State> entity)>> processStateMachineFileTasks =
                ProcessStates();

            var stateMachines = await Task.WhenAll(processStateMachineFileTasks).ConfigureAwait(false);

            var applyTasks = entities.GroupBy(g => g.name)
                .Select(g =>
                    ApplyEntityChanges(g.Key,
                        new Entity(g.First().entity.Namespace,
                        g.SelectMany(e => e.entity.EntityBehaviours).ToList(),
                        g.SelectMany(e => e.entity.DataBehaviours).ToList(),
                        g.SelectMany(e => e.entity.Usings).ToList())
                    )
                );

            var applyApplicationBehaviourTasks = applicationBehaviours
                .Select(g =>
                    ApplyApplicationBehaviourChanges(g.name, g.entity)
                );

            var applyStateMachineTasks = stateMachines
                .Select(st =>
                    ApplyStateMachineChanges(st.name, st.entity)
                );


            await Task.WhenAll(applyTasks).ConfigureAwait(false);
            await Task.WhenAll(applyApplicationBehaviourTasks).ConfigureAwait(false);
            await Task.WhenAll(applyStateMachineTasks).ConfigureAwait(false);


            var codeDependencies = await ProcessCodeDependencies().ConfigureAwait(false);
            var fileDependencies = await ProcessFileDependencies().ConfigureAwait(false);
            await ApplyDependenciesChanges(codeDependencies, fileDependencies).ConfigureAwait(false);

            if (Build)
                await _apiClient.BuildModel(Tenant, Environment).ConfigureAwait(false);


            Console.WriteLine($"Successfully applied behaviours to tenant \"{Tenant}\" model.");
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

        private IEnumerable<Task<(string name, Entity entity)>> ProcessDataBehaviours()
        {
            var files = Directory.GetFiles(Path, "*Dao.cs", SearchOption.AllDirectories);

            return files.Select(ProcessDaoFile);
        }

        private async Task<IDictionary<string, CodeDependency>> ProcessCodeDependencies()
        {
            var dependencies = new Dictionary<string, CodeDependency>();

            foreach (string directory in Directory.GetDirectories(Path, "CodeDependencies", SearchOption.AllDirectories))
            {
                var files = Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly);

                var dependencyData = await Task.WhenAll(files.Select(ProcessCodeDependencyFile)).ConfigureAwait(false);

                foreach (var (name, codeDependency) in dependencyData)
                    dependencies.Add(name, codeDependency);
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

        private async Task ApplyDependenciesChanges(IDictionary<string, CodeDependency> codeDependencies, IDictionary<string, IList<FileDependency>> fileDependencies)
        {
            var dataPerDataSource = new Dictionary<string, (IDictionary<string, CodeDependency> codeDependencies, IList<(string, FileDependency)> fileDependencies)>();

            foreach (var codeDependency in codeDependencies)
            {
                var dataSource = codeDependency.Value.Namespace.Split('.')[4];

                if (!dataPerDataSource.ContainsKey(dataSource))
                    dataPerDataSource.Add(dataSource, (new Dictionary<string, CodeDependency>(), new List<(string, FileDependency)>()));

                dataPerDataSource[dataSource].codeDependencies.Add(codeDependency.Key, codeDependency.Value);
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

        private async Task<bool> ReplaceApplicationBehaviourData(string filepath, ApplicationBehaviour entity)
        => await _definitionService.ReplaceApplicationBehaviourData(Tenant, Environment,
                            ExtractEntityNameFromFileName(filepath, string.Empty), entity).ConfigureAwait(false);

        private async Task<bool> ReplaceData(string name, Data.Entity entity)
            => await _definitionService.ReplaceData(Tenant, Environment,
                            name, entity).ConfigureAwait(false);

        private async Task<bool> ReplaceStateMachineData(string name, List<State> entity) 
            => await _definitionService.ReplaceStateData(Tenant, Environment, name, entity).ConfigureAwait(false);

        private static string ExtractEntityNameFromFileName(string filepath, string suffix)
        {
            var filename = System.IO.Path.GetFileName(filepath);
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
