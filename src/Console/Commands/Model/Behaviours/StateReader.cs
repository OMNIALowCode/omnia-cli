using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.Behaviours.Data;
using Omnia.CLI.Extensions;

namespace Omnia.CLI.Commands.Model.Behaviours
{
	public class StateReader
	{

		public IList<State> ExtractMethods(string text)
		{
			var tree = CSharpSyntaxTree.ParseText(text);
			var root = tree.GetCompilationUnitRoot();

			return ExtractStates(root);
		}

		private IList<State> ExtractStates(CompilationUnitSyntax root)
		{

			var methods = root.DescendantNodes(null, false)
				.OfType<MethodDeclarationSyntax>()
				.Where(m => !m.Identifier.ToFullString().Equals("EvaluateStateTransitions"))
				.Select(MapMethod);

			var stateNames = methods.Where(m => !m.Equals(null) && !m.Type.Equals("Transition")).Select(m => m.State).Distinct();

			return stateNames.Select(n => MapState(n, methods.Where(m => m.State.Equals(n) || (m.Type.Equals("Transition") && m.State.StartsWith(n))))).ToList();
		}

		private StateMethod MapMethod(MethodDeclarationSyntax method)
		{
			return method.Identifier.ToFullString() switch
			{
				var assign when assign.StartsWith("AssignTo_") => new StateMethod { Type = "Assign", Expression = ExtractExpression(method), State = assign.Substring("AssignTo_".Length) },
				var transition when transition.StartsWith("EvaluateStateTransition_") => new StateMethod { Type = "Transition", Expression = ExtractExpression(method), State = transition.Substring("EvaluateStateTransition_".Length) },
				var stateIn when stateIn.StartsWith("On") && stateIn.EndsWith("In") => new StateMethod { Type = stateIn, Expression = ExtractExpression(method), State = stateIn.Substring("On".Length, stateIn.Length - "In".Length - 2) },
				var stateOut when stateOut.StartsWith("On") && stateOut.EndsWith("Out") => new StateMethod { Type = stateOut, Expression = ExtractExpression(method), State = stateOut.Substring("On".Length, stateOut.Length - "Out".Length - 2 ) },
				_ => throw new NotSupportedException()
			};
		}
		private static string ExtractExpression(MethodDeclarationSyntax method)
		{

			var blockText = method.Body?.ToFullString();
			return WithoutLeadingAndTrailingBraces(blockText).Trim();

			static string WithoutLeadingAndTrailingBraces(string blockText)
				=> blockText
					.Substring(0, blockText.LastIndexOf('}'))
					  .Substring(blockText.IndexOf('{') + 1);
		}

		private State MapState(string stateName, IEnumerable<StateMethod> methods)
		{
			return new State
			{
				Name = stateName,
				AssignToExpression = ExtractAssignTo(methods),
				Behaviours = ExtractBehaviours(stateName, methods),
				Transitions = ExtractTransitions(stateName, methods, ExtractTransitionNames(stateName, methods))
			};
		}

		private string ExtractAssignTo(IEnumerable<StateMethod> methods) => methods.Where(m => m.Type.Equals("Assign")).Select(m => m.Expression).Single();

		private List<StateBehaviour> ExtractBehaviours(string stateName, IEnumerable<StateMethod> methods) => methods.Where(m => m.Type.Equals($"On{stateName}In") || m.Type.Equals($"On{stateName}Out")).Select(m => new StateBehaviour { Expression = m.Expression, Name = m.Type, Type = m.Type.Substring($"On{stateName}".Length) }).ToList();

		private List<string> ExtractTransitionNames(string stateName, IEnumerable<StateMethod> methods) => methods.Where(m => m.Type.Equals("Transition")).Select(m => m.State.Substring($"{stateName}_".Length)).ToList();

		private List<Transition> ExtractTransitions(string stateName, IEnumerable<StateMethod> methods, List<string> decisions) => decisions.Select(d =>
		{
			var method = methods.Where(m => m.State.Equals($"{stateName}_{d}")).Single();
			return new Transition
			{
				Name = d,
				Expression = method.Expression,
			};
		}).ToList();
	}
}