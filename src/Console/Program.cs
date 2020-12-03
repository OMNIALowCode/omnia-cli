using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using Omnia.CLI.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Spectre.Cli;
using Omnia.CLI.Commands.Subscriptions;


namespace Omnia.CLI
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var configuration = CreateConfigurationRoot();

            var services = new ServiceCollection()
                .AddSingleton(PhysicalConsole.Singleton)
                .AddScoped<IAuthenticationProvider, AuthenticationProvider>()
                .Configure<AppSettings>(configuration);

            services
                .AddHttpClient<IApiClient, ApiClient>();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            };

            var serviceProvider = services.BuildServiceProvider();

            var registrar = new TypeRegistrar(services);

            var app = new CommandApp(registrar);
            app.Configure(config =>
            {
                config.AddBranch("subscriptions", subscription =>
                {
                    subscription.SetDescription("Commands to configure subscriptions.");
                    subscription.AddCommand<AddCommand>("add");
                    subscription.AddCommand<ListCommand>("list");
                    subscription.AddCommand<RemoveCommand>("remove");
                });

                config.AddBranch("security", security =>
                {
                    security.SetDescription("Commands related to Tenant Security.");
                    security.AddBranch("users", users =>
                    {
                        users.SetDescription("Commands related to Tenant Users security.");
                        users.AddCommand<Commands.Security.Users.AddCommand>("add");
                        users.AddCommand<Commands.Security.Users.ImportCommand>("import");
                    });
                });

                config.AddBranch("management", management =>
                {
                    management.SetDescription("Commands related to Management.");
                    management.AddBranch("tenants", tenants =>
                    {
                        tenants.SetDescription("Commands related to Tenants Management.");
                        tenants.AddCommand<Commands.Management.Tenants.AddCommand>("add");
                    });
                });

                config.AddBranch("application", application =>
                {
                    application.SetDescription("Commands related to Tenant.");
                    application.AddCommand<Commands.Application.ImportCommand>("import");
                });

            });


            var subscriptions = GetConfiguredSubscriptions(serviceProvider);

            if (subscriptions?.Count == 0)
                ShowWelcomeScreen();

            try
            {
                return app.Run(args);
            }
            catch (UnrecognizedCommandParsingException exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{exception.Message}. Use --help to list the available options.");
                Console.ResetColor();
                return (int)StatusCodes.InvalidOperation;
            }
        }

        private static IConfigurationRoot CreateConfigurationRoot()
            => new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(Path.Combine(SettingsPathFactory.Path(), "appsettings.json"), true)
                .Build();

        private static IList<AppSettings.Subscription> GetConfiguredSubscriptions(IServiceProvider services)
            => services.GetService<IOptions<AppSettings>>().Value?.Subscriptions;

        private static void ShowWelcomeScreen()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@"

 ::::::::  ::::    ::::  ::::    ::: :::::::::::     :::           ::::::::  :::        :::::::::::
:+:    :+: +:+:+: :+:+:+ :+:+:   :+:     :+:       :+: :+:        :+:    :+: :+:            :+:
+:+    +:+ +:+ +:+:+ +:+ :+:+:+  +:+     +:+      +:+   +:+       +:+        +:+            +:+
+#+    +:+ +#+  +:+  +#+ +#+ +:+ +#+     +#+     +#++:++#++:      +#+        +#+            +#+
+#+    +#+ +#+       +#+ +#+  +#+#+#     +#+     +#+     +#+      +#+        +#+            +#+
#+#    #+# #+#       #+# #+#   #+#+#     #+#     #+#     #+#      #+#    #+# #+#            #+#
 ########  ###       ### ###    #### ########### ###     ###       ########  ########## ###########

");
            Console.ResetColor();
        }
    }
}
