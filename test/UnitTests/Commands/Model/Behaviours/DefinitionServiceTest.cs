using Moq;
using Omnia.CLI.Commands.Model.Behaviours;
using Omnia.CLI.Commands.Model.Behaviours.Data;
using Omnia.CLI.Infrastructure;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Commands.Model.Behaviours
{
	public class DefinitionServiceTest
	{
		private const string Tenant = "Template";
		private const string Environment = "PRD";
		private const string Definition = "Agent";
		private const string Entity = "Customer";
		private const string Namespace = "Omnia.Behaviours.Template.Internal.System.Model";

		private const string DefinitionStateMachine = "StateMachine";
		private const string StateMachineEntity = "RequestAssistanceStateMachine";

		[Fact]
		public async Task ReplaceData_WithBehavioursList_Successful()
		{
			var entityData = new Entity(Namespace,
			new List<EntityBehaviour>()
			{
				new EntityBehaviour()
				{
					Name = "ExecuteInitialize",
					Expression = "_code =\"Hi\";"
				}
			}, null, null);

			var apiClientMock = new Mock<IApiClient>();
			apiClientMock.Setup(r => r.Get($"/api/v1/{Tenant}/{Environment}/model/output/definitions/{Entity}"))
				.ReturnsAsync((new ApiResponse(true), "{\"instanceOf\":\"Agent\"}"));
			apiClientMock.Setup(r => r.Patch(It.IsAny<string>(), It.IsAny<HttpContent>()))
				.ReturnsAsync((new ApiResponse(true)));

			var service = new DefinitionService(apiClientMock.Object);

			await service.ReplaceData(Tenant, Environment, Entity, entityData)
				.ConfigureAwait(false);

			apiClientMock.Verify(r => r.Patch($"/api/v1/{Tenant}/{Environment}/model/{Definition}/{Entity}",
			It.IsAny<StringContent>()));
		}

		[Fact]
		public async Task ReplaceData_WithState_Successful()
		{
			var states = new List<State>(){

				new State
				{
					Name = "Initial",
					Behaviours = new List<StateBehaviour>()
					{
						new StateBehaviour {
							Expression = "",
							Name = "OnInitialIn",
							Type = "In"
						}
					},
					Transitions = new List<Transition>()
					{
						new Transition
						{
							Expression = "",
							Name = "Confirm"
						}
					},
					AssignToExpression = "this._name=\"test\""
				},
				new State
				{
					Name = "Accepted",
					Behaviours = new List<StateBehaviour>()
					{
						new StateBehaviour {
							Expression = "",
							Name = "OnAcceptedIn",
							Type = "In"
						}
					},
						Transitions = new List<Transition>()
					{
						new Transition
						{
							Expression = "",
							Name = "Revert"
						}
					},
					AssignToExpression = null
				}
			};

			var apiClientMock = new Mock<IApiClient>();
			apiClientMock.Setup(r => r.Get($"/api/v1/{Tenant}/{Environment}/model/StateMachine/{StateMachineEntity}"))
				.ReturnsAsync((new ApiResponse(true), "{\"name\":\"RequestAssistanceStateMachine\",\"states\": [ {\"name\":\"Initial\",\"decisions\": [ {\"name\":\"Accept\",\"order\": 1,\"commentType\":\"None\",\"description\":\"\" }, {\"name\":\"Reject\",\"order\": 2,\"commentType\":\"None\",\"description\":\"\" } ],\"isInitial\": true,\"behaviours\": [ {\"name\":\"OnInitialIn\",\"type\":\"In\",\"expression\":\"//comment onInitialIn\",\"description\":\"\" }, {\"name\":\"OnInitialOut\",\"type\":\"Out\",\"expression\":\"//comment onInitialOut\",\"description\":\"\" } ],\"description\": null,\"transitions\": [ {\"to\":\"Accepted\",\"name\":\"Confirm\",\"type\":\"Decision\",\"order\": 1,\"decision\":\"Accept\",\"expression\":\"\",\"description\":\"\" }, {\"to\":\"Rejected\",\"name\":\"Decline\",\"type\":\"Decision\",\"order\": 2,\"decision\":\"Reject\",\"expression\":\"\",\"description\":\"\" }, {\"to\":\"Initial\",\"name\":\"Draft\",\"type\":\"Auto\",\"order\": 3,\"decision\":\"\",\"expression\":\"\",\"description\":\"\" } ],\"disableAttributes\": false,\"disableOperations\": false,\"enabledAttributes\": [],\"enabledOperations\": [],\"assignToExpression\": null }, {\"name\":\"Rejected\",\"decisions\": [],\"isInitial\": false,\"behaviours\": [],\"description\":\"\",\"transitions\": [],\"disableAttributes\": true,\"disableOperations\": true,\"enabledAttributes\": [],\"enabledOperations\": [],\"assignToExpression\":\"\" }, {\"name\":\"Accepted\",\"decisions\": [],\"isInitial\": false,\"behaviours\": [],\"description\":\"\",\"transitions\": [],\"disableAttributes\": true,\"disableOperations\": true,\"enabledAttributes\": [],\"enabledOperations\": [],\"assignToExpression\":\"\" } ],\"definition\":\"RequestAssistance\",\"description\":\"State Machine of Request Assistance document\" }"));
			apiClientMock.Setup(r => r.Patch(It.IsAny<string>(), It.IsAny<HttpContent>()))
				.ReturnsAsync((new ApiResponse(true)));


			var service = new DefinitionService(apiClientMock.Object);
			await service.ReplaceStateData(Tenant, Environment, StateMachineEntity, states)
				.ConfigureAwait(false);

			apiClientMock.Verify(r => r.Patch($"/api/v1/{Tenant}/{Environment}/model/{DefinitionStateMachine}/{StateMachineEntity}",
			It.IsAny<StringContent>()));
		}
	}
}
