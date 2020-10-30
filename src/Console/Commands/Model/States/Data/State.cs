using System.Collections.Generic;

namespace Omnia.CLI.Commands.Model.States.Data
{
    public enum EvaluationType
	{
        Automatic,
        Decision
	}
    
    public class Evaluation
	{
        public string Expression { get; set; }
#nullable enable
        public string? Decision { get; set; }
#nullable disable
    }

    public class Transition
    {
        public string Name { get; set; }
        public string GoToStateName { get; set; }
        public EvaluationType Type { get; set; }
        public Evaluation Evaluation { get; set; }
    }

    public class State
    {
        public string Name { get; set; }
        public List<string> Decisions { get; set; }
        public string BehaviourIn { get; set; }
        public string BehaviourOut { get; set; }
        public bool IsInitial { get; set; }
        public List<Transition>? Transitions { get; set; }
    }
}