using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.Behaviours.Data
{
    public class Entity
    {
        public Entity(IList<Behaviour> behaviours, IList<string> usings)
        {
            Behaviours = behaviours;
            Usings = usings;
        }

        public IList<Behaviour> Behaviours { get; }
        public IList<string> Usings { get; }
    }
}
