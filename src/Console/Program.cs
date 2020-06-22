using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using Omnia.CLI.Infrastructure;

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

            var serviceProvider = services.BuildServiceProvider();

            var app = new CommandLineApplication<App>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(serviceProvider);

            var subscriptions = GetConfiguredSubscriptions(serviceProvider);

            if (subscriptions?.Count == 0)
                ShowWelcomeScreen();

            return app.Execute(args);
        }

        private static IConfigurationRoot CreateConfigurationRoot()
            => new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OMNIA", "CLI", "appsettings.json"), true)
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
