using System;
using System.Collections.Generic;
using System.Linq;

namespace Omnia.CLI
{
    public class AppSettings
    {
        public IList<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        internal Subscription GetSubscription(string name)
        {
            var sourceSettings = Subscriptions.FirstOrDefault(s => s.Name.Equals(name));
            if (sourceSettings == null)
                throw new InvalidOperationException($"Can't find subscription {name}");
            return sourceSettings;
        }

        public class Subscription
        {
            public string Name { get; set; }
            public Uri Endpoint { get; set; }

            public Uri ApiUrl => new Uri(Endpoint, "/api/v1/");
            public Uri IdentityServerUrl => new Uri(Endpoint, "/identity/");

            public Client Client { get; set; } = new Client();

        }
        public class Client
        {
            public string Id { get; set; }
            public string Secret { get; set; }
        }
    }
}
