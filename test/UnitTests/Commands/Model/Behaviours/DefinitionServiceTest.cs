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

        [Fact]
        public async Task ReplaceData_WithBehavioursList_Successful()
        {
            var entityData = new Entity(Namespace,
            new List<EntityBehaviour>()
            {
                new EntityBehaviour()
                {
                    Name = "ExecuteInitialize",
                    Expression = "_code = \"Hi\";"
                }
            }, null);

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
    }
}
