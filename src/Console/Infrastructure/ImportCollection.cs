using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.CLI.Infrastructure
{
    public class ImportCollection
    {
        public string Id { get; set; }
        public string ParentId { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }
}
