using Omnia.CLI.Commands.Model.Apply.Readers;
using Shouldly;
using System.Linq;
using Xunit;

namespace UnitTests.Commands.Model.Apply
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
		public void ExtractData_Successfully()
		{
			var reader = new StateReader();

			var states = reader.ExtractData(FileText);

			states.ShouldNotBeNull();
			states.Count.ShouldBe(3);
		}

		[Fact]
		public void ExtractData_HasName()
		{
			var reader = new StateReader();

			var states = reader.ExtractData(FileText);

			states.ShouldNotContain(s => string.IsNullOrEmpty(s.Name));
		}

		[Fact]
		public void ExtractData_InitialHasTransitions()
		{
			var reader = new StateReader();

			var initial = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"));

			initial.Transitions.ShouldNotBeEmpty();
			initial.Transitions.Count.ShouldBe(3);
		}

		[Fact]
		public void ExtractData_AcceptedEmptyTransitions()
		{
			var reader = new StateReader();

			var accepted = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Accepted"));

			accepted.Transitions.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractData_RejectedEmptyTransitions()
		{
			var reader = new StateReader();

			var rejected = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Rejected"));

			rejected.Transitions.ShouldBeEmpty();
		}

		[Fact]
		public void ExtractData_InitialHasConfirmTransition()
		{
			var reader = new StateReader();

			var initial = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"));

			initial.Transitions.ShouldContain(t => t.Name.Equals("Confirm"));
		}

		[Fact]
		public void ExtractData_InitialHasDeclineTransition()
		{
			var reader = new StateReader();

			var initial = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"));

			initial.Transitions.ShouldContain(t => t.Name.Equals("Decline"));
		}

		[Fact]
		public void ExtractData_InitialHasDraftTransition()
		{
			var reader = new StateReader();

			var initial = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"));

			initial.Transitions.ShouldContain(t => t.Name.Equals("Draft"));
		}



		[Fact]
		public void ExtractData_ValidInitialHasBehaviours()
		{
			var reader = new StateReader();

			var initial = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"));

			initial.Behaviours.Count.ShouldBe(2);
		}

		[Fact]
		public void ExtractData_ValidInitialBehaviourIn()
		{
			var reader = new StateReader();

			var initial = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"));

			initial.Behaviours.ShouldContain(b => b.Name.Equals("OnInitialIn") && b.Type.Equals("In"));
            initial.Behaviours.Single(b => b.Name.Equals("OnInitialIn")).Expression.ShouldBe("this._name = \"Initial In Name\";");
		}

		[Fact]
		public void ExtractData_ValidInitialBehaviourOut()
		{
			var reader = new StateReader();

			var initial = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"));

			initial.Behaviours.ShouldContain(b => b.Name.Equals("OnInitialOut") && b.Type.Equals("Out"));
            initial.Behaviours.Single(b => b.Name.Equals("OnInitialOut")).Expression.ShouldBe("this._name = \"Initial Out Name\";");
		}

		[Fact]
		public void ExtractData_ValidConfirmExpression()
		{
			var reader = new StateReader();

			var confirm = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"))
				.Transitions.Single(t => t.Name.Equals("Confirm"));

			confirm.Expression.ShouldBe("var test = \"tst\";\r\n\t\t\tthis._name = \"Confirmation\";\r\n\t\t\treturn true;");
		}

		[Fact]
		public void ExtractData_ValidDeclineExpression()
		{
			var reader = new StateReader();

			var decline = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"))
				.Transitions.Single(t => t.Name.Equals("Decline"));

			decline.Expression.ShouldBe("return true;");
		}

		[Fact]
		public void ExtractData_ValidDraftExpression()
		{
			var reader = new StateReader();

			var draft = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"))
				.Transitions.Single(t => t.Name.Equals("Draft"));

			draft.Expression.ShouldBe("return true;");
		}

		[Fact]
		public void ExtractData_ValidInitialAssignToExpression()
		{
			var reader = new StateReader();

			var initial = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Initial"));

			initial.AssignToExpression.ShouldBe("return null;");
		}

		[Fact]
		public void ExtractData_ValidRejectedAssignToExpression()
		{
			var reader = new StateReader();

			var rejected = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Rejected"));

			rejected.AssignToExpression.ShouldBe("return null;");
		}

		[Fact]
		public void ExtractData_ValidAcceptedAssignToExpression()
		{
			var reader = new StateReader();

			var accepted = reader.ExtractData(FileText)
				.Single(m => m.Name.Equals("Accepted"));

			accepted.AssignToExpression.ShouldBe("return null;");
		}
	}
}
