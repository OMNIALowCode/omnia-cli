using System;
using Microsoft.Extensions.Options;
using Omnia.CLI;

namespace UnitTests
{
    public class AppSettingsBuilder
    {
        private readonly AppSettings _settings = new AppSettings();

        public AppSettingsBuilder WithDefaults()
        {
            _settings.Subscriptions.Add(new AppSettings.Subscription()
            {
                Name = "Testing",
                Endpoint = new Uri("http://localhost:8080/"),
                Client =  new AppSettings.Client()
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
