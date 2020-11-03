using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Omnia.CLI.Commands.Model.Behaviours.Data
{

    public class ApplicationBehaviour
    {
        public ApplicationBehaviour(string @namespace, string expression, IList<string> usings)
        {
            Namespace = @namespace;
            Expression = expression;
            Usings = usings;
        }
        public string Expression { get; set; }
        public string Namespace { get; set; }
        public IList<string> Usings { get; }

    }
}