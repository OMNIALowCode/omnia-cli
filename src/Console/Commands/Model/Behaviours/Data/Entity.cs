using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.Behaviours.Data
{
    public class Entity
    {
        public Entity(string @namespace, 
            IList<EntityBehaviour> entityBehaviours, IList<DataBehaviour> dataBehaviours, 
            IList<string> usings)
        {
            Namespace = @namespace;
            EntityBehaviours = entityBehaviours ?? new List<EntityBehaviour>();
            DataBehaviours = dataBehaviours ?? new List<DataBehaviour>();
            Usings = usings ?? new List<string>();
        }

        public IList<EntityBehaviour> EntityBehaviours { get; }
        public IList<DataBehaviour> DataBehaviours { get; }
        public IList<string> Usings { get; }
        public string Namespace { get; }
    }
}
