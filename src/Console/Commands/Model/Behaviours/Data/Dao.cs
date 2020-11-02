using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.Behaviours.Data
{
    public class Dao
    {
        public Dao(string @namespace, IList<DataBehaviour> behaviours, IList<string> usings)
        {
            Namespace = @namespace;
            Behaviours = behaviours;
            Usings = usings;
        }

        public IList<DataBehaviour> Behaviours { get; }
        public IList<string> Usings { get; }
        public string Namespace { get; }
    }
}
