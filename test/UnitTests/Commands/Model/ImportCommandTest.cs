using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Omnia.CLI;
using Omnia.CLI.Commands.Model;
using Shouldly;
using UnitTests.Fakes;
using UnitTests.Extensions;
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
            var mockHttpMessageHandler = MockHttpMessageHandler();
            var factoryMock = MockHttpClientFactory(mockHttpMessageHandler);

            var command = ImportCommand(factoryMock);

            var result = await command.OnExecute(new CommandLineApplication<App>());

            result.ShouldBe((int)StatusCodes.Success);

            mockHttpMessageHandler.VerifyRequestHasBeenMade(
                HttpMethod.Post,
                new Uri($"{AppSettingsBuilder.DefaultEndpoint}/api/v1/{command.Tenant}/PRD/model/import"),
                Times.Exactly(1));
        }

        private ImportCommand ImportCommand(IMock<IHttpClientFactory> factoryMock)
        {
            var command = new ImportCommand(_settings, factoryMock.Object, new FakeAuthenticationProvider())
            {
                Path = Path.Combine(Directory.GetCurrentDirectory(),
                    "Commands", "Model", "TestData", "FakeModel.zip"),
                Tenant = "CliTesting",
                Subscription = "Testing"
            };
            return command;
        }

        private static Mock<IHttpClientFactory> MockHttpClientFactory(IMock<HttpMessageHandler> mockHttpMessageHandler)
        {
            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri(AppSettingsBuilder.DefaultEndpoint)
            };

            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            return mockFactory;
        }

        private static Mock<HttpMessageHandler> MockHttpMessageHandler()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
            return mockHttpMessageHandler;
        }
    }
}
