using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Omnia.CLI.Infrastructure;
using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;

namespace Omnia.CLI.Commands.Subscriptions
{
    [Description("Remove a given subscription configuration.")]
    public sealed class RemoveCommand : AsyncCommand<RemoveCommandSettings>
    {
        private readonly AppSettings _settings;
        public RemoveCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public override ValidationResult Validate(CommandContext context, RemoveCommandSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Name))
            {
                return ValidationResult.Error($"{nameof(settings.Name)} is required");
            }

            if (!_settings.Exists(settings.Name))
            {
                return ValidationResult.Error($"Subscription \"{settings.Name}\" can't be found.");
            }
            return base.Validate(context, settings);
        }
        public override Task<int> ExecuteAsync(CommandContext context, RemoveCommandSettings settings)
        {
            var subscription = _settings.GetSubscription(settings.Name);
            _settings.Subscriptions.Remove(subscription);

            var directory = SettingsPathFactory.Path();

            using (var file = File.CreateText(Path.Combine(directory, "appsettings.json")))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, _settings);
            }

            Console.WriteLine($"Subscription \"{settings.Name}\" configuration removed successfully.");
            return Task.FromResult((int)StatusCodes.Success);
        }
    }
}
