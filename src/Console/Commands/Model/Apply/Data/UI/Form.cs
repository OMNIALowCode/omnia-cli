using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.Apply.Data.UI
{
    public class Form
    {
        public Form(IList<UIEntityBehaviour> entityBehaviours)
        {
            EntityBehaviours = entityBehaviours ?? new List<UIEntityBehaviour>();
        }

        public IList<UIEntityBehaviour> EntityBehaviours { get; }
    }
}
