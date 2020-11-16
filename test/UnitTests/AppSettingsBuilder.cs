using System;
using Microsoft.Extensions.Options;
using Omnia.CLI;

namespace UnitTests
{
    public class AppSettingsBuilder
    {
        public const string DefaultEndpoint = "http://localhost:8080";

        private readonly AppSettings _settings = new AppSettings();

        public AppSettingsBuilder WithDefaults()
        {
            _settings.Subscriptions.Add(new AppSettings.Subscription
            {
                Name = "Testing",
                Endpoint = new Uri(DefaultEndpoint),
                Client =  new AppSettings.Client
                {
                    Id = "FakeApiClient",
                    Secret = "FakeApiClientSecret"
                }
            });
            return this;
        }

        public AppSettingsBuilder WithSubscription(AppSettings.Subscription subscription)
        {
            _settings.Subscriptions.Add(subscription);
            return this;
        }

        public AppSettings Build()
            => _settings;

        public IOptions<AppSettings> BuildAsOptions()
            => Options.Create(Build());
    }
}
