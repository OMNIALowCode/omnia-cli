using System.Collections.Generic;

namespace Omnia.CLI.Commands.Application.Infrastructure
{
    public class ImportCollection
    {
        public string Id { get; set; }
        public string ParentId { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }
}
