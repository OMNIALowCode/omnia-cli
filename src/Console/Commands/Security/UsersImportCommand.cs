using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Omnia.CLI.Infrastructure;
using System.Linq;
using System.IO.Compression;
using Omnia.CLI.Extensions;
using System.Collections.Generic;

namespace Omnia.CLI.Commands.Security
{
    [Command(Name = "users-import", Description = "")]
    [HelpOption("-h|--help")]
    public class UsersImportCommand
    {
        private readonly AppSettings _settings;
        public UsersImportCommand(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        [Option("--subscription", CommandOptionType.SingleValue, Description = "")]
        public string Subscription { get; set; }
        [Option("--tenant", CommandOptionType.SingleValue, Description = "")]
        public string Tenant { get; set; }
        [Option("--environment", CommandOptionType.SingleValue, Description = "")]
        public string Environment { get; set; }

        [Option("--path", CommandOptionType.SingleValue, Description = "")]
        public string Path { get; set; }

        public Task<int> OnExecute(CommandLineApplication cmd)
        {
            throw new NotImplementedException();
        }
        

    }
}
