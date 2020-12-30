using Microsoft.Extensions.Options;
using Omnia.CLI.Commands.Application.Infrastructure;
using Omnia.CLI.Infrastructure;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Omnia.CLI.Extensions;
using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;

namespace Omnia.CLI.Commands.Application
{
    [Description("Import application data.")]
    public sealed class ImportCommand : AsyncCommand<ImportCommandSettings>
    {
        private readonly AppSettings _settings;
        private readonly IApiClient _apiClient;

        public ImportCommand(IOptions<AppSettings> options, IApiClient apiClient)
        {
            _apiClient = apiClient;
            _settings = options.Value;
        }

        public override ValidationResult Validate(CommandContext context, ImportCommandSettings settings)
        {
            if (string.IsNullOrEmpty(settings.Path))
            {
                return ValidationResult.Error($"{nameof(settings.Path)} is required");
            }

            if (!File.Exists(settings.Path))
            {
                return ValidationResult.Error($"The value of --path parameters \"{settings.Path}\" is not a valid file.");
            }
            return base.Validate(context, settings);
        }

        public override async Task<int> ExecuteAsync(CommandContext context, ImportCommandSettings settings)
        {
            var sourceSettings = _settings.GetSubscription(settings.Subscription);

            await _apiClient.Authenticate(sourceSettings).ConfigureAwait(false);

            var reader = new ImportDataReader();
            try
            {
                var data = reader.ReadExcel(settings.Path);

                var success = await ProcessDefinitions(settings.Tenant, settings.Environment, data).ConfigureAwait(false);

                if (!success) return (int)StatusCodes.UnknownError;

                Console.WriteLine($"Successfully imported data to tenant \"{settings.Tenant}\".");
                return (int)StatusCodes.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in importation : {ex.GetBaseException().Message}.");
                return (int)StatusCodes.UnknownError;
            }
        }

        private async Task<bool> ProcessDefinitions(string tenant, string environment, ICollection<ImportData> data)
        {
            var failed = new List<string>();
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Cyan,
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            using (var progressBar = new ProgressBar(data.Count, "Processing file...", options))
            {
                foreach (var dataEntry in data)
                {
                    var (success, messages) =
                        await CreateEntities(progressBar, _apiClient, tenant, environment, dataEntry.Definition, dataEntry.DataSource, dataEntry.Data)
                            .ConfigureAwait(false);
                    progressBar.Tick();

                    if (success)
                        continue;

                    failed.AddRange(messages);
                }
            }

            Console.WriteLine($"----- Failed: {failed.Count} -----");
            foreach (var message in failed)
                Console.WriteLine(message);

            return !failed.Any();
        }

        private static async Task<(bool Success, string[] Messages)> CreateEntities(ProgressBarBase progressBar,
            IApiClient apiClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            ICollection<(int RowNumber, IDictionary<string, object> Values)> data)
        {
            var failedEntities = new List<string>();

            using (var child = progressBar.Spawn(data.Count, $"Processing entity {definition} with {data.Count} records..."))
            {
                foreach (var (rowNumber, values) in data)
                {
                    var (statusCode, errors) = await CreateEntity(apiClient, tenantCode, environmentCode, definition, dataSource, values)
                        .ConfigureAwait(false);

                    child.Tick(statusCode == (int)StatusCodes.Success ? null : $"Error creating entity for {dataSource} {definition}");

                    if (statusCode == (int)StatusCodes.Success) continue;

                    child.ForegroundColor = ConsoleColor.DarkRed;

                    failedEntities.Add($"Error to import {dataSource}.{definition}: In row {rowNumber} with errors: {GetErrors(errors)}");
                }
            }

            return (!failedEntities.Any(), failedEntities.ToArray());

            static string GetErrors(ApiError errors) => errors != null ? ProcessErrors(errors) : "Unknown Error";

            static string ProcessErrors(ApiError errors)
                    => errors.Errors != null ? JoinErrors(errors) : $" \n\r {errors.Code} - {errors.Message}";

            static string JoinErrors(ApiError errors)
                => string.Concat(errors.Errors.Select(c => $"\n\r {c.Name} - {c.Message}"));
        }

        private static async Task<(int statusCode, ApiError errors)> CreateEntity(IApiClient apiClient,
            string tenantCode,
            string environmentCode,
            string definition,
            string dataSource,
            IDictionary<string, object> data)
        {
            var response = await apiClient.Post($"/api/v1/{tenantCode}/{environmentCode}/application/{definition}/{dataSource}", data.ToHttpStringContent())
                .ConfigureAwait(false);

            return response.Success ? ((int)StatusCodes.Success, null) : ((int)StatusCodes.InvalidOperation, response.ErrorDetails);
        }
    }
}
