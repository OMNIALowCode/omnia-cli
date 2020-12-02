using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Apply.Data.Server;
using Omnia.CLI.Commands.Model.Apply.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Omnia.CLI.Commands.Model.Apply.Readers.Server
{
    public class StateReader
	{
		public IList<State> ExtractData(string text)
		{
			var tree = CSharpSyntaxTree.ParseText(text);
			var root = tree.GetCompilationUnitRoot();

			return ExtractStates(root);
		}

		private IList<State> ExtractStates(CompilationUnitSyntax root)
		{

			var methods = root.DescendantNodes()
				.OfType<MethodDeclarationSyntax>()
				.Where(m => !m.Identifier.ToFullString().Equals("EvaluateStateTransitions"))
				.Select(MapMethod)
                .ToList();

			var stateNames = methods.Where(m => !m.Equals(null) && !m.Type.Equals("Transition")).Select(m => m.State).Distinct();

			return stateNames.Select(n => MapState(n, 
                methods.Where(m => m.State.Equals(n) || (m.Type.Equals("Transition") && m.State.StartsWith(n))).ToList())).ToList();
		}

		private static StateMethod MapMethod(MethodDeclarationSyntax method)
		{
			return method.Identifier.ToFullString() switch
			{
                var assign when assign.StartsWith("AssignTo_")
					=> new StateMethod { Type = "Assign", Expression = method.ExtractExpression(), State = assign.Substring("AssignTo_".Length) },
                var transition when transition.StartsWith("EvaluateStateTransition_")
					=> new StateMethod { Type = "Transition", Expression = method.ExtractExpression(), State = transition.Substring("EvaluateStateTransition_".Length) },
				var stateIn when stateIn.StartsWith("On") && stateIn.EndsWith("In")
					=> new StateMethod { Type = stateIn, Expression = method.ExtractExpression(), State = stateIn.Substring("On".Length, stateIn.Length - "In".Length - 2) },
				var stateOut when stateOut.StartsWith("On") && stateOut.EndsWith("Out")
					=> new StateMethod { Type = stateOut, Expression = method.ExtractExpression(), State = stateOut.Substring("On".Length, stateOut.Length - "Out".Length - 2 ) },
				_ => throw new NotSupportedException()
			};
		}

		private State MapState(string stateName, IList<StateMethod> methods)
		{
			return new State
			{
				Name = stateName,
				AssignToExpression = ExtractAssignTo(methods),
				Behaviours = ExtractBehaviours(stateName, methods),
				Transitions = ExtractTransitions(stateName, methods, ExtractTransitionNames(stateName, methods))
			};
		}

		private static string ExtractAssignTo(IEnumerable<StateMethod> methods) => methods.Where(m => m.Type.Equals("Assign")).Select(m => m.Expression).Single();

		private static List<StateBehaviour> ExtractBehaviours(string stateName, IEnumerable<StateMethod> methods) 
			=> methods.Where(m => m.Type.Equals($"On{stateName}In") || m.Type.Equals($"On{stateName}Out")).Select(m => new StateBehaviour { Expression = m.Expression, Name = m.Type, Type = m.Type.Substring($"On{stateName}".Length) }).Where(BehaviourHasExpression).ToList();

		private static List<string> ExtractTransitionNames(string stateName, IEnumerable<StateMethod> methods) 
			=> methods.Where(m => m.Type.Equals("Transition")).Select(m => m.State.Substring($"{stateName}_".Length)).ToList();

		private static List<Transition> ExtractTransitions(string stateName, IEnumerable<StateMethod> methods, List<string> decisions) 
			=> decisions.Select(d =>
			{
				var method = methods.Single(m => m.State.Equals($"{stateName}_{d}"));
				return new Transition
				{
					Name = d,
					Expression = method.Expression,
				};
			}).Where(TransitionHasExpression).ToList();

		private static bool BehaviourHasExpression(StateBehaviour stateBehaviour)
			=> !string.IsNullOrEmpty(stateBehaviour.Expression);

		private static bool TransitionHasExpression(Transition transition)
			=> !string.IsNullOrEmpty(transition.Expression);
	}
}