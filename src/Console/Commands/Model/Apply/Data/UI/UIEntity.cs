using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.Apply.Data.UI
{
    public class UIEntity
    {
        public UIEntity(IList<UIBehaviour> entityBehaviours)
        {
            EntityBehaviours = entityBehaviours ?? new List<UIBehaviour>();
        }

        public IList<UIBehaviour> EntityBehaviours { get; }
    }
}
