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

        [Fact]
        public void Watch_WhenFileIsUpdated_EntityIsPatched()
        {
            const string entity = "UserA";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
                "Commands", "Model", "TestData", "FakeModel", "Model");
            var apiClientMock = MockApiClient();
            apiClientMock.Setup(s => s.Get($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}"))
                .ReturnsAsync((true, "{\"Test\":\"Test\"}"));

            var eventManualWorker = new ManualResetEventSlim(false);

            var importer = new DirectoryImporter(apiClientMock.Object, Tenant, Environment,
                    pathToWatch);
            importer.OnFileChange += ImporterOnFileChange;

            importer.Watch();


            File.WriteAllText(Path.Combine(pathToWatch, "Agent", $"{entity}.json"), "{\"Test\":\"Bola\"}");

            eventManualWorker.Wait();

            apiClientMock.Verify(client =>
                    client.Patch($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}",
                        It.IsAny<StringContent>()),
                Times.Once);

            void ImporterOnFileChange(object sender, FileSystemEventArgs e)
                => eventManualWorker.Set();

        }
        [Fact]
        public void Watch_WhenFileIsCreated_CreateRequested()
        {
            const string entity = "FakeEntity";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
                "Commands", "Model", "TestData", "FakeModel", "Model");
            var fileToCreate = Path.Combine(pathToWatch, "Agent", $"{entity}.json");
            if (File.Exists(fileToCreate))
                File.Delete(fileToCreate);

            var apiClientMock = MockApiClient();

            var eventManualWorker = new ManualResetEventSlim(false);

            var importer = new DirectoryImporter(apiClientMock.Object, Tenant, Environment,
                pathToWatch);
            importer.OnFileCreated += ImporterOnFileChange;

            importer.Watch();

            File.WriteAllText(fileToCreate, "{}");

            eventManualWorker.Wait();

            apiClientMock.Verify(client =>
                    client.Post($"/api/v1/{Tenant}/{Environment}/model/Agent",
                        It.IsAny<StringContent>()),
                Times.Once); 

            void ImporterOnFileChange(object sender, FileSystemEventArgs e)
                => eventManualWorker.Set();
        }

        [Fact]
        public void Watch_WhenFileIsDeleted_DeleteRequested()
        {
            const string entity = "UserB";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
                "Commands", "Model", "TestData", "FakeModel", "Model");
            var fileToDelete = Path.Combine(pathToWatch, "Agent", $"{entity}.json");
            
            var apiClientMock = MockApiClient();

            var eventManualWorker = new ManualResetEventSlim(false);

            var importer = new DirectoryImporter(apiClientMock.Object, Tenant, Environment,
                pathToWatch);
            importer.OnFileDeleted += ImporterOnFileChange;

            importer.Watch();

            File.Delete(fileToDelete);
            
            eventManualWorker.Wait();

            apiClientMock.Verify(client =>
                    client.Delete($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}"),
                Times.Once); 

            void ImporterOnFileChange(object sender, FileSystemEventArgs e)
                => eventManualWorker.Set();
        }

        private static Mock<IApiClient> MockApiClient()
        {
            return new Mock<IApiClient>();
        }
    }
}
