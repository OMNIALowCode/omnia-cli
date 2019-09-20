using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace Omnia.CLI.CommandHandlers
{
    public static class SourcesCommandHandler
    {
        public static void Add(AppSettings settings, string name, string endpoint, string clientId, string clientSecret)
        {
            settings.Subscriptions.Add(new AppSettings.Subscription()
            {
                Name = name,
                Endpoint = new Uri(endpoint),
                Client = new AppSettings.Client()
                {
                    Id = clientId,
                    Secret = clientSecret
                }
            });

            using (StreamWriter file = File.CreateText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, settings);
            }
        }

        public static void Remove(AppSettings settings, string name)
        {
            var subscription = settings.Subscriptions.FirstOrDefault(s => s.Name.Equals(name));
            settings.Subscriptions.Remove(subscription);

            using (StreamWriter file = File.CreateText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, settings);
            }
        }
    }
}
