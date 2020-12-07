using Spectre.Cli;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Omnia.CLI.Infrastructure;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Description("Add the configuration to a given subscription.")]
    public sealed class AddCommand : AsyncCommand<AddCommandSettings>
    {
        private readonly AppSettings _settings;
        public AddCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public override ValidationResult Validate(CommandContext context, AddCommandSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Name))
            {
                return ValidationResult.Error($"{nameof(settings.Name)} is required");
            }

            if (settings.Endpoint == null)
            {
                return ValidationResult.Error($"{nameof(settings.Endpoint)} is required");
            }

            if (string.IsNullOrWhiteSpace(settings.ClientId))
            {
                return ValidationResult.Error($"{nameof(settings.ClientId)} is required");
            }

            if (string.IsNullOrWhiteSpace(settings.ClientSecret))
            {
                return ValidationResult.Error($"{nameof(settings.ClientSecret)} is required");
            }

            if (_settings.Exists(settings.Name))
            {
                return ValidationResult.Error($"Subscription \"{settings.Name}\" already exists.");
            }

            return base.Validate(context, settings);
        }

        public override Task<int> ExecuteAsync(CommandContext context, AddCommandSettings settings)
        {
            _settings.Subscriptions.Add(new AppSettings.Subscription()
            {
                Name = settings.Name,
                Endpoint = settings.Endpoint,
                Client = new AppSettings.Client()
                {
                    Id = settings.ClientId,
                    Secret = settings.ClientSecret
                }
            });

            var directory = SettingsPathFactory.Path();

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var file = File.CreateText(Path.Combine(directory, "appsettings.json")))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, _settings);
            }

            Console.WriteLine($"Configuration successfully added to subscription \"{settings.Name}\".");
            return Task.FromResult((int)StatusCodes.Success);
        }
    }
}
