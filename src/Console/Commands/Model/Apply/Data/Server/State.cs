using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.Apply.Data.Server
{
    public class StateMethod
	{
        public string Type { get; set; }
        public string Expression { get; set; }
        public string State { get; set; }
	}
    
    public class StateBehaviour
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Expression { get; set; }
    }

    public class Transition
    {
        public string Name { get; set; }
        public string Expression { get; set; }
    }

    public class State
    {
        public string Name { get; set; }
        public string AssignToExpression { get; set; }
        public List<StateBehaviour> Behaviours { get; set; }
        public List<Transition> Transitions { get; set; }
    }
}