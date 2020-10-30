using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.Behaviours.Data
{
    public class Entity
    {
        public Entity(string @namespace, IList<Behaviour> behaviours, IList<string> usings)
        {
            Namespace = @namespace;
            Behaviours = behaviours;
            Usings = usings;
        }

        public IList<Behaviour> Behaviours { get; }
        public IList<string> Usings { get; }
        public string Namespace { get; }
    }
}
