using Moq;
using Omnia.CLI.Commands.Model.Import;
using Omnia.CLI.Infrastructure;
using System.IO;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace UnitTests.Commands.Model
{
    public class DirectoryImporterTest
    {
        private const string Tenant = "CliTesting";
        private const string Environment = "PRD";
        private readonly ManualResetEventSlim _eventManualWorker = new ManualResetEventSlim(false);


        [Fact]
        public void OnExecute_WithWatchAndFileHasBeenChanged_EntityIsPatched()
        {
            const string entity = "UserA";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
                "Commands", "Model", "TestData", "FakeModel", "Model");
            var apiClientMock = MockApiClient();
            apiClientMock.Setup(s => s.Get($"/api/v1/{Tenant}/{Environment}/model/{entity}"))
                .ReturnsAsync((true, "{\"Test\":\"Test\"}"));

            var importer = new DirectoryImporter(apiClientMock.Object, Tenant, Environment,
                pathToWatch);
            importer.OnFileChange += Importer_OnFileChange;

            importer.Watch();

            
            File.WriteAllText(Path.Combine(pathToWatch, "Agent", $"{entity}.json"), "{}");

            _eventManualWorker.Wait();

            apiClientMock.Verify(client =>
                    client.Patch($"/api/v1/{Tenant}/{Environment}/model/{entity}", 
                        It.IsAny<StringContent>()),
                Times.Exactly(2)); //TODO: Should be once
        }

        private void Importer_OnFileChange(object sender, FileSystemEventArgs e)
        {
            _eventManualWorker.Set();
        }

        private static Mock<IApiClient> MockApiClient()
        {
            return new Mock<IApiClient>();
            
        }
    }
}
