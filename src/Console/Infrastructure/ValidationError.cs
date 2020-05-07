using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.CLI.Infrastructure
{
    public class ValidationError
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }
}
