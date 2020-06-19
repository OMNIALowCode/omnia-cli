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
using UnitTests.Stubs;
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
            var mockFactory = new Mock<IHttpClientFactory>();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost:8080")
            };

            mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var factory = mockFactory.Object;


            var command = new ImportCommand(_settings, factory, new FakeAuthenticationProvider())
            {
                Path = Path.Combine(Directory.GetCurrentDirectory(),
                    "Commands", "Model", "TestData", "FakeModel.zip"),
                Tenant = "CliTesting",
                Subscription = "Testing"
            };

            var result = await command.OnExecute(new CommandLineApplication<App>());

            result.ShouldBe((int)StatusCodes.Success);



            // also check the 'http' call was like we expected it
            var expectedUri = new Uri($"http://localhost:8080/api/v1/{command.Tenant}/PRD/model/import");

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1), // we expected a single external request
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post  // we expected a GET request
                        && req.RequestUri == expectedUri // to this uri
                ),
                ItExpr.IsAny<CancellationToken>()
            );

        }

    }
}
