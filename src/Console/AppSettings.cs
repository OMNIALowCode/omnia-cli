using System;
using System.Collections.Generic;

 namespace Omnia.CLI
{
    public class AppSettings
    {
        public IList<Subscription> Subscriptions { get; set; } = new List<Subscription>();

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
