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
        [Fact]
        public async Task ReplaceBehaviours_Successful()
        {
            const string tenant = "Template";
            const string environment = "PRD";
            const string definition = "Agent";
            const string entity = "Customer";
            var behaviours = new List<Behaviour>()
            {
                new Behaviour()
                {
                    Name = "ExecuteInitialize",
                    Expression = "_code = \"Hi\";"
                }
            };
            var apiClientMock = new Mock<IApiClient>();
            apiClientMock.Setup(r => r.Get($"/api/v1/{tenant}/{environment}/model/output/definitions/{entity}"))
                .ReturnsAsync((new ApiResponse(true), "{\"instanceOf\":\"Agent\"}"));
            apiClientMock.Setup(r => r.Patch(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync((new ApiResponse(true)));

            var service = new DefinitionService(apiClientMock.Object);

            await service.ReplaceBehaviours(tenant, environment, entity, behaviours)
                .ConfigureAwait(false);

            apiClientMock.Verify(r => r.Patch($"/api/v1/{tenant}/{environment}/model/{definition}/{entity}",
            It.IsAny<StringContent>()));

        }
    }
}
