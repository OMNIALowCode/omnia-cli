using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.Behaviours.Data
{

    public class ApplicationBehaviour
    {
        public ApplicationBehaviour(string name, string description, string @namespace, string expression, IList<string> usings)
        {
            Name = name;
            Description = description;
            Namespace = @namespace;
            Expression = expression;
            Usings = usings;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Expression { get; set; }
        public string Namespace { get; set; }
        public IList<string> Usings { get; }

    }
}