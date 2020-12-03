using Spectre.Cli;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Omnia.CLI.Infrastructure;
using System.ComponentModel;

namespace Omnia.CLI.Commands.Subscriptions
{
    public sealed class AddCommandSettings : CommandSettings
    {
        [CommandOption("--name <VALUE>")]
        [Description("Name to reference this subscription configuration when using the CLI.")]
        public string Name { get; set; }

        [CommandOption("--endpoint <VALUE>")]
        [Description("Subscription endpoint. Example: https://platform.omnialowcode.com")]
        public Uri Endpoint { get; set; }

        [CommandOption("--client-id <VALUE>")]
        [Description("API Client - ID.")]
        public string ClientId { get; set; }

        [CommandOption("--client-secret <VALUE>")]
        [Description("API Client - Secret.")]
        public string ClientSecret { get; set; }
    }



}
