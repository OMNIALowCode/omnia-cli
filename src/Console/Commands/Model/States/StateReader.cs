using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Omnia.CLI.Commands.Model.States.Data;

namespace Omnia.CLI.Commands.Model.States
{
	public class StateReader
	{
		public IList<State> ExtractMethods(string text)
		{

			var tree = CSharpSyntaxTree.ParseText(text);
			var root = tree.GetCompilationUnitRoot();

			var methods = root.DescendantNodes(null, false)
				.OfType<MethodDeclarationSyntax>()
				.ToList();

			var stateMachineName = ExtractStateMachineName(root);

			return ExtractStates(methods, stateMachineName);
		}

		public static string ExtractStateMachineName(CompilationUnitSyntax root)
		{
			var classDeclaration = root.DescendantNodes(null, false)
				.OfType<ClassDeclarationSyntax>()
				.ToList()
				.FirstOrDefault();

			return classDeclaration.Identifier.Text;
		}

		private IList<State> ExtractStates(List<MethodDeclarationSyntax> methods, string stateMachineName)
		{
			List<string> stateNames = (from method in methods
									   where method.Identifier.ValueText.StartsWith("AssignTo_")
									   select method.Identifier.ValueText.Split('_')[1]).ToList();

			return (from stateName in stateNames
					let evaluateStateTransitionsCases = (from method in methods
														 where method.Identifier.ValueText.Equals("EvaluateStateTransitions")
														 let nodes = method.DescendantNodes()
														 let switchCase = SyntaxFactory.Block(nodes.OfType<SwitchStatementSyntax>()).Statements[0]
														 select ((SwitchStatementSyntax)switchCase).Sections
														 ).FirstOrDefault()
					select new State
					{
						Name = stateName,
						BehaviourIn = ExtractBehaviour(methods, stateName, "In"),
						BehaviourOut = ExtractBehaviour(methods, stateName, "Out"),
						Decisions = ExtractDecisions(evaluateStateTransitionsCases, stateName, stateMachineName),
						IsInitial = ExtractIsInitial(evaluateStateTransitionsCases, stateName, stateMachineName),
						Transitions = ExtractTransitions(methods, evaluateStateTransitionsCases, stateName, stateMachineName),
						ExpressionAssignTo = ExtractAssignTo(methods, stateName)
					}).ToList();
		}

		private static string ExtractBehaviour(List<MethodDeclarationSyntax> methods, string stateName, string behaviourType)
		{
			return (from method in methods
							where method.Identifier.ValueText.Equals($"On{stateName}{behaviourType}")
							let nodes = method.DescendantNodes()
							let block = SyntaxFactory.Block(nodes.OfType<ExpressionStatementSyntax>())
							select block.GetText().ToString().Trim('{', '}')).FirstOrDefault();
		}

		private static List<string> ExtractDecisions(SyntaxList<SwitchSectionSyntax> evaluateStateTransitionsCases, string stateName, string stateMachineName)
		{
			return (from evaluation in evaluateStateTransitionsCases
					where evaluation.Labels.ToString().StartsWith($"case {stateMachineName}StateMachineStates.{stateName} when \"")
					select evaluation.Labels.ToString().Split('"')[1]).Distinct().ToList();
		}

		private static bool ExtractIsInitial(SyntaxList<SwitchSectionSyntax> evaluateStateTransitionsCases, string stateName, string stateMachineName)
		{
			return (from evaluation in evaluateStateTransitionsCases
					where evaluation.Labels.ToString().StartsWith($"case {stateMachineName}StateMachineStates.{stateName} when string.IsNullOrEmpty(_Context.Operation.Decision)")
					select evaluation != null).FirstOrDefault();
		}

		private static List<Transition> ExtractTransitions(List<MethodDeclarationSyntax> methods, SyntaxList<SwitchSectionSyntax> evaluateStateTransitionsCases, string stateName, string stateMachineName)
		{
			var transitionNames = ExtractTransitionNames(methods, stateName);

			return (from transitionName in transitionNames
					let transitionExpression = ExtractTransitionExpression(methods, stateName, transitionName)
					from evaluation in evaluateStateTransitionsCases
					where evaluation.Labels.ToString().Contains($"EvaluateStateTransition_{stateName}_{transitionName}()")
					let transitionType = evaluation.Labels.ToString().Contains($"case {stateMachineName}StateMachineStates.{stateName} when \"") ? EvaluationType.Decision : EvaluationType.Automatic
					let statements = SyntaxFactory.Block(evaluation.Statements[0])
					let goToStateName = (statements.GetText().ToString().Split('.')[1]).Split(')')[0]

					select new Transition()
					{
						Evaluation = new Evaluation()
						{
							Decision = transitionType.Equals(EvaluationType.Decision) ? evaluation.Labels.ToString().Split('"')[1] : null,
							Expression = transitionExpression
						},
						GoToStateName = goToStateName,
						Name = transitionName,
						Type = transitionType
					}).ToList();
		}

		private static List<string> ExtractTransitionNames(List<MethodDeclarationSyntax> methods, string stateName)
		{
			return (from method in methods
					where method.Identifier.ValueText.StartsWith($"EvaluateStateTransition_{stateName}_")
					// Can't split ValueText by '_' because "Confirmation_Transition" is a valid transition name.
					select method.Identifier.ValueText.Substring(method.Identifier.ValueText.IndexOf($"EvaluateStateTransition_{stateName}_") + $"EvaluateStateTransition_{stateName}_".Length)).ToList();
		}

		private static string ExtractTransitionExpression(List<MethodDeclarationSyntax> methods, string stateName, string transitionName)
		{
			return (from method in methods
					where method.Identifier.ValueText.Equals($"EvaluateStateTransition_{stateName}_{transitionName}")
					let nodes = method.DescendantNodes()
					let block = SyntaxFactory.Block(nodes.Where(n => n is ExpressionStatementSyntax || n is ReturnStatementSyntax || n is LocalDeclarationStatementSyntax).OfType<StatementSyntax>())
					select block.GetText().ToString().Trim('{', '}')
					).FirstOrDefault();
		}

		private static string ExtractAssignTo(List<MethodDeclarationSyntax> methods, string stateName)
		{
			return (from method in methods
					where method.Identifier.ValueText.StartsWith($"AssignTo_{stateName}")
					let nodes = method.DescendantNodes()
					let block = SyntaxFactory.Block(nodes.Where(n => n is ExpressionStatementSyntax || n is ReturnStatementSyntax || n is LocalDeclarationStatementSyntax).OfType<StatementSyntax>())
					select block.GetText().ToString().Trim('{', '}')
					).FirstOrDefault();
		}
	}
}