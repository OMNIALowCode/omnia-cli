using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Omnia.CLI.Extensions;
using Omnia.CLI.Infrastructure;

namespace Omnia.CLI.CommandHandlers
{
    public class UsersCommandHandler
    {

        public static async Task Import(AppSettings settings, string subscription, string tenantCode, string environmentCode, string file)
        {
            var sourceSettings = settings.Subscriptions.FirstOrDefault(s => s.Name.Equals(subscription));
            if (sourceSettings == null)
                throw new InvalidOperationException($"Can't find subscription {subscription}");

            throw new NotImplementedException();
        }

    }
}

