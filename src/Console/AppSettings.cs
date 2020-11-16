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
            if (SubscriptionNotProvidedAndOnlyOneConfigured())
                return Subscriptions.Single();

            if(!IsSubscriptionNameProvided())
                throw new InvalidOperationException("A registered subscription name must be provided.");

            var sourceSettings = Subscriptions.SingleOrDefault(s => s.Name.Equals(name));
            if (sourceSettings == null)
                throw new InvalidOperationException($"Can't find subscription {name}");
            return sourceSettings;

            bool SubscriptionNotProvidedAndOnlyOneConfigured()
                => !IsSubscriptionNameProvided() && Subscriptions.Count == 1;

            bool IsSubscriptionNameProvided()
                => !string.IsNullOrEmpty(name);
        }
        internal bool Exists(string name)
            => Subscriptions.Any(s => s.Name.Equals(name));

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
