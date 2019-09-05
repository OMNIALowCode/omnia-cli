using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;

 namespace Omnia.CLI
{
    class Program
    {
        static int Main(string[] args)
        {
            var settings = new AppSettings();
            var configuration = CreateConfigurationRoot();
            configuration.Bind(settings);
            
            var app = new CommandLineApplication()
            {
                Name = "OMNIA CLI"
            };

            app.Command("export", (command) =>
            {
                var subscriptionOption = command.Option("--subscription <subscription>", "Subscription.", CommandOptionType.SingleValue);
                var tenantOption = command.Option("--tenant <tenant>", "", CommandOptionType.SingleValue);
                var environmentOption = command.Option("--environment <environment>", "", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    CommandHandlers.ExportCommandHandler.Run(settings, subscriptionOption.Value(),
                        tenantOption.Value(),
                        environmentOption.HasValue() ? environmentOption.Value() : "PRD")
                        .GetAwaiter().GetResult();
                    return 0;
                });
            });

            app.Command("subscriptions", (command) =>
            {
                command.Command("add", (subCommand) =>
                {
                    var nameOption = subCommand.Option("--name <name>", "Subscription.", CommandOptionType.SingleValue);
                    var endpointOption = subCommand.Option("--endpoint <endpoint>", "", CommandOptionType.SingleValue);
                    var clientIdOption = subCommand.Option("--client-id <client-id>", "", CommandOptionType.SingleValue);
                    var clientSecretOption = subCommand.Option("--client-secret <client-secret>", "", CommandOptionType.SingleValue);

                    subCommand.OnExecute(() =>
                    {
                        CommandHandlers.SourcesCommandHandler.Run(settings, nameOption.Value(),
                            endpointOption.Value(),
                            clientIdOption.Value(),
                            clientSecretOption.Value());
                        return 0;
                    });
                });
            });

            

            return app.Execute(args);
        }

        private static IConfigurationRoot CreateConfigurationRoot()
            => new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
    }
}
