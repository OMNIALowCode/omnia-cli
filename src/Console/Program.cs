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

            app.Command("tenants", (command) =>
            {
                command.Command("model", (modelCommand) =>
                {
                    modelCommand.Command("export", (exportCommand) =>
                    {
                        var subscriptionOption = exportCommand.Option("--subscription <subscription>", "Subscription.",
                            CommandOptionType.SingleValue);
                        var tenantOption = exportCommand.Option("--tenant <tenant>", "", CommandOptionType.SingleValue);
                        var environmentOption = exportCommand.Option("--environment <environment>", "",
                            CommandOptionType.SingleValue);

                        exportCommand.OnExecute(() =>
                        {
                            CommandHandlers.ExportCommandHandler.Run(settings, subscriptionOption.Value(),
                                    tenantOption.Value(),
                                    environmentOption.HasValue() ? environmentOption.Value() : "PRD")
                                .GetAwaiter().GetResult();
                            return 0;
                        });
                    });
                });

                command.Command("security", (securityCommand) =>
                    {
                        securityCommand.Command("users", (usersCommand) =>
                            {
                                usersCommand.Command("import", (usersImportCommand) =>
                                {
                                    var subscriptionOption = usersImportCommand.Option("--subscription <subscription>", "Subscription.",
                                        CommandOptionType.SingleValue);
                                    var tenantOption = usersImportCommand.Option("--tenant <tenant>", "", CommandOptionType.SingleValue);
                                    var environmentOption = usersImportCommand.Option("--environment <environment>", "",
                                        CommandOptionType.SingleValue);
                                    var fileOption = usersImportCommand.Option("--file <file>", "csv file to import", CommandOptionType.SingleValue);

                                    usersImportCommand.OnExecute(() =>
                                    {
                                        CommandHandlers.UsersCommandHandler.Import(settings, subscriptionOption.Value(),
                                            tenantOption.Value(),
                                            environmentOption.HasValue() ? environmentOption.Value() : "PRD",
                                            fileOption.Value())
                                            .GetAwaiter().GetResult();
                                        return 0;
                                    });
                                });
                            });
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
                        CommandHandlers.SourcesCommandHandler.Add(settings, nameOption.Value(),
                            endpointOption.Value(),
                            clientIdOption.Value(),
                            clientSecretOption.Value());
                        return 0;
                    });
                });

                command.Command("remove", (subCommand) =>
                {
                    var nameOption = subCommand.Option("--name <name>", "Subscription.", CommandOptionType.SingleValue);

                    subCommand.OnExecute(() =>
                    {
                        CommandHandlers.SourcesCommandHandler.Remove(settings, nameOption.Value());
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
