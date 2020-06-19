using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace Omnia.CLI
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var configuration = CreateConfigurationRoot();
            
            var services = new ServiceCollection()
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .Configure<AppSettings>(configuration)
                .AddHttpClient()
                .BuildServiceProvider();

            var app = new CommandLineApplication<App>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            var subscriptions = GetConfiguredSubscriptions(services);

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

        private static IList<AppSettings.Subscription> GetConfiguredSubscriptions(ServiceProvider services)
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
