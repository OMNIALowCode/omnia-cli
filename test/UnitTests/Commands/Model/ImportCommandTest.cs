using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Moq;
using Omnia.CLI;
using Omnia.CLI.Commands.Model.Import;
using Omnia.CLI.Infrastructure;
using Shouldly;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Commands.Model
{
    public class ImportCommandTest
    {
        private readonly IOptions<AppSettings> _settings;
        public ImportCommandTest()
        {
            _settings = new AppSettingsBuilder()
                .WithDefaults()
                .BuildAsOptions();
        }

        [Fact]
        public async Task OnExecute_UsingZip_ModelImportIsInvoked()
        {
            var apiClientMock = MockApiClient();
            var command = ImportCommand(apiClientMock);

            var result = await command.OnExecute(new CommandLineApplication<App>());

            result.ShouldBe((int)StatusCodes.Success);

            apiClientMock.Verify(client=> 
                client.Post($"/api/v1/{command.Tenant}/PRD/model/import", It.IsAny<MultipartFormDataContent>()),
                Times.Once);
        }

        private ImportCommand ImportCommand(IMock<IApiClient> apiClientMock)
        {
            var command = new ImportCommand(_settings, apiClientMock.Object)
            {
                Path = Path.Combine(Directory.GetCurrentDirectory(),
                    "Commands", "Model", "TestData", "FakeModel.zip"),
                Tenant = "CliTesting",
                Subscription = "Testing"
            };
            return command;
        }

        private static Mock<IApiClient> MockApiClient()
        {
            var apiClientMock = new Mock<IApiClient>();
            apiClientMock.Setup(s => s.Post(It.IsAny<string>(), It.IsAny<MultipartFormDataContent>()))
                .ReturnsAsync((true, null));
            apiClientMock.Setup(s => s.Post(It.IsAny<string>(), It.IsAny<StringContent>()))
                .ReturnsAsync((true, null));
            return apiClientMock;
        }
    }
}
