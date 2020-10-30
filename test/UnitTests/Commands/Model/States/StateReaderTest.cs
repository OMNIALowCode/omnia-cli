using Omnia.CLI.Commands.Model.States;
using Omnia.CLI.Commands.Model.States.Data;
using Shouldly;
using System.Linq;
using Xunit;

namespace UnitTests.Commands.Model.States
{
	public class StateReaderTest
	{

		private const string FileText =
@"
/***************************************************************
****************************************************************
	THIS CODE HAS BEEN AUTOMATICALLY GENERATED
****************************************************************
****************************************************************/

using Omnia.Behaviours.mvTesting3.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Omnia.Libraries.Infrastructure.Connector;
using Omnia.Libraries.Infrastructure.Connector.Client;
using System.Net.Http.Formatting;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Omnia.Libraries.Infrastructure.Behaviours;
using Action = Omnia.Libraries.Infrastructure.Behaviours.Action;


namespace Omnia.Behaviours.mvTesting3.Internal.System.Model
{
    public partial class RequestAssistance
    {

		public void EvaluateStateTransitions()
        {
			
			switch (_state)
            {
				case RequestAssistanceStateMachineStates.Initial when ""Accept"".Equals(_Context.Operation.Decision) &&  EvaluateStateTransition_Initial_Confirm():
                    MoveToState(RequestAssistanceStateMachineStates.Accepted);
                    break;
				case RequestAssistanceStateMachineStates.Initial when ""Reject"".Equals(_Context.Operation.Decision) &&  EvaluateStateTransition_Initial_Decline() :

					MoveToState(RequestAssistanceStateMachineStates.Rejected);
                    break;
				case RequestAssistanceStateMachineStates.Initial when  EvaluateStateTransition_Initial_Draft() :

					MoveToState(RequestAssistanceStateMachineStates.Initial);
                    break;

				case RequestAssistanceStateMachineStates.Initial when string.IsNullOrEmpty(_Context.Operation.Decision) :

					_assigned = AssignTo_Initial();
                    break;
				case RequestAssistanceStateMachineStates.Initial when string.IsNullOrEmpty(_Context.Operation.Decision) :

					_assigned = AssignTo_Initial();
                    break;
				case RequestAssistanceStateMachineStates.Initial when string.IsNullOrEmpty(_Context.Operation.Decision) :

					_assigned = AssignTo_Initial();
                    break;
                default:
                    break;
            }

		void MoveToState(RequestAssistanceStateMachineStates targetState)
		{
			var oldState = _state;

			// STATE OUT
			switch (_state)
			{
				case RequestAssistanceStateMachineStates.Initial:
					OnInitialOut(targetState);
					break;
				case RequestAssistanceStateMachineStates.Rejected:
					OnRejectedOut(targetState);
					break;
				case RequestAssistanceStateMachineStates.Accepted:
					OnAcceptedOut(targetState);
					break;

				default:
					break;
			}

			// CHANGE STATE
			_state = targetState;

			// SET ASSIGNED && EXECUTE STATE IN
			switch (_state)
			{
				case RequestAssistanceStateMachineStates.Initial:
					_assigned = AssignTo_Initial();

					OnInitialIn(oldState);
					break;
				case RequestAssistanceStateMachineStates.Rejected:
					_assigned = AssignTo_Rejected();

					OnRejectedIn(oldState);
					break;
				case RequestAssistanceStateMachineStates.Accepted:
					_assigned = AssignTo_Accepted();

					OnAcceptedIn(oldState);
					break;

				default:
					break;
			}
		}
	}


	/// Assign Expressions
	/// -------------------------
	private string AssignTo_Initial()
	{
			return null;
	}
	private string AssignTo_Rejected()
	{
			return null;
	}
	private string AssignTo_Accepted()
	{
			return null;
	}

	/// Transition Expressions
	/// -------------------------
	private bool EvaluateStateTransition_Initial_Confirm()
	{
			var test = ""tst"";
			this._name = ""Confirmation"";
			return true;
	}
	private bool EvaluateStateTransition_Initial_Decline()
	{
			return true;
	}
	private bool EvaluateStateTransition_Initial_Draft()
	{
			return true;
	}

	/// State Expressions
	/// -------------------------

	private void OnInitialIn(RequestAssistanceStateMachineStates fromState)
	{
			this._name = ""Initial In Name"";
	}

	private void OnInitialOut(RequestAssistanceStateMachineStates toState)
	{
			this._name = ""Initial Out Name"";
	}

	private void OnRejectedIn(RequestAssistanceStateMachineStates fromState)
	{

	}

	private void OnRejectedOut(RequestAssistanceStateMachineStates toState)
	{

	}

	private void OnAcceptedIn(RequestAssistanceStateMachineStates fromState)
	{

	}

	private void OnAcceptedOut(RequestAssistanceStateMachineStates toState)
	{

	}
}
}";

		[Fact]
		public void ExtractMethods_Successfully()
		{
			var reader = new StateReader();

			var states = reader.ExtractMethods(FileText);

			states.ShouldNotBeNull();
			states.Count.ShouldBe(3);
		}

		[Fact]
		public void ExtractMethods_HasName()
		{
			var reader = new StateReader();

			var states = reader.ExtractMethods(FileText);

			states.ShouldNotContain(s => string.IsNullOrEmpty(s.Name));
		}

		[Fact]
		public void ExtractMethods_InitialHasDecisions()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.Decisions.ShouldNotBeEmpty();
			initial.Decisions.Count.ShouldBe(2);
		}

		[Fact]
		public void ExtractMethods_RejectedEmptyDecisions()
		{
			var reader = new StateReader();

			var rejected = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Rejected"));

			rejected.Decisions.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractMethods_AcceptedEmptyDecisions()
		{
			var reader = new StateReader();

			var accepted = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Accepted"));

			accepted.Decisions.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractMethods_ValidInitialDecisions()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.Decisions.ShouldContain("Accept");
			initial.Decisions.ShouldContain("Reject");
		}

		[Fact]
		public void ExtractMethods_ValidInitialBehaviourIn()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.BehaviourIn.ShouldBe("\t\t\tthis._name = \"Initial In Name\";\r\n");
		}

		[Fact]
		public void ExtractMethods_ValidInitialBehaviourOut()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.BehaviourOut.ShouldBe("\t\t\tthis._name = \"Initial Out Name\";\r\n");
		}

		[Fact]
		public void ExtractMethods_ValidAcceptedBehaviourOut()
		{
			var reader = new StateReader();

			var accepted = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Accepted"));

			accepted.BehaviourOut.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractMethods_ValidAcceptedBehaviourIn()
		{
			var reader = new StateReader();

			var accepted = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Accepted"));

			accepted.BehaviourIn.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractMethods_ValidRejectedBehaviourOut()
		{
			var reader = new StateReader();

			var rejected = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Rejected"));

			rejected.BehaviourOut.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractMethods_ValidRejectedBehaviourIn()
		{
			var reader = new StateReader();

			var rejected = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Rejected"));

			rejected.BehaviourIn.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractMethods_ValidInitialIsInitial()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.IsInitial.ShouldNotBeNull();
			initial.IsInitial.ShouldBeTrue();
		}

		[Fact]
		public void ExtractMethods_ValidAcceptedIsInitial()
		{
			var reader = new StateReader();

			var accepted = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Accepted"));

			accepted.IsInitial.ShouldNotBeNull();
			accepted.IsInitial.ShouldBeFalse();
		}

		[Fact]
		public void ExtractMethods_ValidRejectedIsInitial()
		{
			var reader = new StateReader();

			var rejected = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Rejected"));

			rejected.IsInitial.ShouldNotBeNull();
			rejected.IsInitial.ShouldBeFalse();
		}

		[Fact]
		public void ExtractMethods_InitialHasTransitions()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.Transitions.ShouldNotBeEmpty();
			initial.Transitions.Count.ShouldBe(3);
		}

		[Fact]
		public void ExtractMethods_AcceptedEmptyTransitions()
		{
			var reader = new StateReader();

			var accepted = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Accepted"));

			accepted.Transitions.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractMethods_RejectedEmptyTransitions()
		{
			var reader = new StateReader();

			var rejected = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Rejected"));

			rejected.Transitions.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractMethods_InitialHasConfirmTransition()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.Transitions.ShouldContain(t => t.Name.Equals("Confirm"));
		}

		[Fact]
		public void ExtractMethods_InitialHasDeclineTransition()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.Transitions.ShouldContain(t => t.Name.Equals("Decline"));
		}

		[Fact]
		public void ExtractMethods_InitialHasDraftTransition()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.Transitions.ShouldContain(t => t.Name.Equals("Draft"));
		}

		[Fact]
		public void ExtractMethods_ValidConfirmGoToStateName()
		{
			var reader = new StateReader();

			var confirm = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Confirm"));

			confirm.GoToStateName.ShouldBe("Accepted");
		}

		[Fact]
		public void ExtractMethods_ValidDeclineGoToStateName()
		{
			var reader = new StateReader();

			var reject = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Decline"));

			reject.GoToStateName.ShouldBe("Rejected");
		}

		[Fact]
		public void ExtractMethods_ValidDraftGoToStateName()
		{
			var reader = new StateReader();

			var draft = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Draft"));

			draft.GoToStateName.ShouldBe("Initial");
		}

		[Fact]
		public void ExtractMethods_ValidConfirmEvaluationType()
		{
			var reader = new StateReader();

			var confirm = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Confirm"));

			confirm.Type.ShouldBe(EvaluationType.Decision);
		}

		[Fact]
		public void ExtractMethods_ValidDeclineEvaluationType()
		{
			var reader = new StateReader();

			var reject = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Decline"));

			reject.Type.ShouldBe(EvaluationType.Decision);
		}

		[Fact]
		public void ExtractMethods_ValidDraftEvaluationType()
		{
			var reader = new StateReader();

			var draft = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Draft"));

			draft.Type.ShouldBe(EvaluationType.Automatic);
		}

		[Fact]
		public void ExtractMethods_ValidConfirmEvaluation()
		{
			var reader = new StateReader();

			var confirm = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Confirm"));

			confirm.Evaluation.ShouldNotBeNull();
			confirm.Evaluation.Expression.ShouldBe("\t\t\tvar test = \"tst\";\r\n\t\t\tthis._name = \"Confirmation\";\r\n\t\t\treturn true;\r\n");
			confirm.Evaluation.Decision.ShouldNotBeNull();
		}

		[Fact]
		public void ExtractMethods_ValidDeclineEvaluation()
		{
			var reader = new StateReader();

			var reject = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Decline"));

			reject.Evaluation.ShouldNotBeNull();
			reject.Evaluation.Expression.ShouldBe("\t\t\treturn true;\r\n");
			reject.Evaluation.Decision.ShouldNotBeNull();
		}

		[Fact]
		public void ExtractMethods_ValidDraftEvaluation()
		{
			var reader = new StateReader();

			var draft = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"))
				.Transitions.First(t => t.Name.Equals("Draft"));

			draft.Evaluation.ShouldNotBeNull();
			draft.Evaluation.Expression.ShouldBe("\t\t\treturn true;\r\n");
			draft.Evaluation.Decision.ShouldBeNull();
		}

		[Fact]
		public void ExtractMethods_ValidInitialExpressionAssignTo()
		{
			var reader = new StateReader();

			var initial = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Initial"));

			initial.ExpressionAssignTo.ShouldBe("\t\t\treturn null;\r\n");
		}

		[Fact]
		public void ExtractMethods_ValidRejectedExpressionAssignTo()
		{
			var reader = new StateReader();

			var rejected = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Rejected"));

			rejected.ExpressionAssignTo.ShouldBe("\t\t\treturn null;\r\n");
		}

		[Fact]
		public void ExtractMethods_ValidAcceptedExpressionAssignTo()
		{
			var reader = new StateReader();

			var accepted = reader.ExtractMethods(FileText)
				.First(m => m.Name.Equals("Accepted"));

			accepted.ExpressionAssignTo.ShouldBe("\t\t\treturn null;\r\n");
		}
	}
}
