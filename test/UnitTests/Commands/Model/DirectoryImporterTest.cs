using Moq;
using Omnia.CLI.Commands.Model.Import;
using Omnia.CLI.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Xunit;

namespace UnitTests.Commands.Model
{
    public class DirectoryImporterTest
    {
        /*
        private const string Tenant = "CliTesting";
        private const string Environment = "PRD";

        [Fact]
        public void Watch_WhenFileIsUpdated_EntityIsPatched()
        {
            const string entity = "EntityIsPatched";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
                "Commands", "Model", "TestData", "FakeModel", "Model");
            var fileToCreate = Path.Combine(pathToWatch, "Agent", $"{entity}.json");
            if (!File.Exists(fileToCreate))
                File.Create(fileToCreate).Close();
            var apiClientMock = MockApiClient();
            apiClientMock.Setup(s => s.Get($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}"))
                .ReturnsAsync((new ApiResponse(true, HttpStatusCode.OK), "{\"Test\":\"Test\"}"));
            apiClientMock.Setup(s => s.Patch($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}", It.IsAny<StringContent>()))
                   .ReturnsAsync((new ApiResponse(true, HttpStatusCode.OK)));

            var eventManualWorker = new ManualResetEventSlim(false);

            var importer = new DirectoryImporter(apiClientMock.Object, Tenant, Environment,
                   pathToWatch);

            importer.OnFileChange += ImporterOnFileChange;

            importer.Watch();

            File.WriteAllText(fileToCreate, "{\"Test\":\"Bola\"}");

            eventManualWorker.Wait();

            apiClientMock.Verify(client =>
                    client.Patch($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}",
                        It.IsAny<StringContent>()),
                Times.AtLeastOnce);

            void ImporterOnFileChange(object sender, FileSystemEventArgs e)
                => eventManualWorker.Set();
        }

        [Fact]
        public void Watch_WhenEntityNotFoundInPatch()
        {
            const string entity = "NotFound";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
                "Commands", "Model", "TestData", "FakeModel", "Model");
            var apiClientMock = MockApiClient();
            apiClientMock.Setup(s => s.Get($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}"))
                .ReturnsAsync((new ApiResponse(true, HttpStatusCode.NotFound), string.Empty));
            apiClientMock.Setup(s => s.Post($"/api/v1/{Tenant}/{Environment}/model/Agent", It.IsAny<StringContent>()))
                    .ReturnsAsync((new ApiResponse(true, HttpStatusCode.OK)));
            var eventManualWorker = new ManualResetEventSlim(false);

            var importer = new DirectoryImporter(apiClientMock.Object, Tenant, Environment,
                    pathToWatch);
            importer.OnFileChange += ImporterOnFileChange;

            importer.Watch();

            File.WriteAllText(Path.Combine(pathToWatch, "Agent", $"{entity}.json"), "{\"Test\":\"Bola\"}");

            eventManualWorker.Wait();

            apiClientMock.Verify(client =>
                    client.Post($"/api/v1/{Tenant}/{Environment}/model/Agent",
                        It.IsAny<StringContent>()),
                Times.AtLeastOnce);

            void ImporterOnFileChange(object sender, FileSystemEventArgs e)
                => eventManualWorker.Set();
        }

        [Fact]
        public void Watch_CreateWithJsonFilter()

        {
            const string entity = "JsonFilter";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
               "Commands", "Model", "TestData", "FakeModel", "Model");

            var apiClientMock = MockApiClient();

            apiClientMock.Setup(s => s.Get($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}"))
             .ReturnsAsync((new ApiResponse(true, HttpStatusCode.OK), "{\"Test\":\"Bola\"}"));
            apiClientMock.Setup(s => s.Post($"/api/v1/{Tenant}/{Environment}/model/Agent", It.IsAny<StringContent>()))
                         .ReturnsAsync(new ApiResponse(true, HttpStatusCode.OK));
            apiClientMock.Setup(s => s.Patch($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}", It.IsAny<StringContent>()))
                    .ReturnsAsync((new ApiResponse(true, HttpStatusCode.OK)));
            var fileJsonToCreate = Path.Combine(pathToWatch, "Agent", $"{entity}.json");
            var fileTxtToCreate = Path.Combine(pathToWatch, "Agent", $"{entity}.txt");

            if (File.Exists(fileJsonToCreate))
                File.Delete(fileJsonToCreate);

            if (File.Exists(fileTxtToCreate))
                File.Delete(fileTxtToCreate);

            var eventManualWorker = new ManualResetEventSlim(false);

            var importer = new DirectoryImporter(apiClientMock.Object, Tenant, Environment,
                pathToWatch);
            importer.OnFileCreated += ImporterOnFileChange;

            importer.Watch();

            File.WriteAllText(fileJsonToCreate, "{}");
            File.WriteAllText(fileTxtToCreate, "Teste");

            eventManualWorker.Wait();

            apiClientMock.Verify(client =>
                    client.Post($"/api/v1/{Tenant}/{Environment}/model/Agent",
                        It.IsAny<StringContent>()),
                Times.Once);

            void ImporterOnFileChange(object sender, FileSystemEventArgs e)
                => eventManualWorker.Set();
        }

        [Fact]
        public void Watch_FileLock()
        {
            const string entity = "FileLock";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
               "Commands", "Model", "TestData", "FakeModel", "Model");

            var apiClientMock = MockApiClient();

            apiClientMock.Setup(s => s.Post($"/api/v1/{Tenant}/{Environment}/model/Agent", It.IsAny<StringContent>()))
                         .ReturnsAsync(new ApiResponse(true, HttpStatusCode.OK));
            apiClientMock.Setup(s => s.Get($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}"))
               .ReturnsAsync((new ApiResponse(true, HttpStatusCode.NotFound), string.Empty));
            apiClientMock.Setup(s => s.Patch($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}", It.IsAny<StringContent>()))
                    .ReturnsAsync((new ApiResponse(true, HttpStatusCode.OK)));

            var fileCreate = Path.Combine(pathToWatch, "Agent", $"{entity}.json");

            if (File.Exists(fileCreate))
                File.Delete(fileCreate);

            var eventManualWorker = new ManualResetEventSlim(false);

            var importer = new DirectoryImporter(apiClientMock.Object, Tenant, Environment,
                pathToWatch);
            importer.OnFileCreated += ImporterOnFileChange;

            importer.Watch();

            for (int i = 0; i < 1000; i++)
            {
                File.WriteAllText(fileCreate, i.ToString());
            }

            eventManualWorker.Wait();

            /*apiClientMock.Verify(client =>
                    client.Post($"/api/v1/{Tenant}/{Environment}/model/Agent",
                        It.IsAny<StringContent>()),
                Times.);

            void ImporterOnFileChange(object sender, FileSystemEventArgs e)
                => eventManualWorker.Set();
        }

        [Fact]
        public void Watch_WhenFileIsCreated_CreateRequested()
        {
            const string entity = "Create";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
                "Commands", "Model", "TestData", "FakeModel", "Model");
            var fileToCreate = Path.Combine(pathToWatch, "Agent", $"{entity}.json");
            if (File.Exists(fileToCreate))
                File.Delete(fileToCreate);

            var apiClientMock = MockApiClient();
            apiClientMock.Setup(s => s.Post($"/api/v1/{Tenant}/{Environment}/model/Agent", It.IsAny<StringContent>()))
                       .ReturnsAsync(new ApiResponse(true, HttpStatusCode.OK));
            apiClientMock.Setup(s => s.Get($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}"))
          .ReturnsAsync((new ApiResponse(true, HttpStatusCode.NotFound), string.Empty));
            apiClientMock.Setup(s => s.Patch($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}", It.IsAny<StringContent>()))
                    .ReturnsAsync((new ApiResponse(true, HttpStatusCode.OK)));

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
                Times.AtLeastOnce);

            void ImporterOnFileChange(object sender, FileSystemEventArgs e)
                => eventManualWorker.Set();
        }

        [Fact]
        public void Watch_WhenFileIsDeleted_DeleteRequested()
        {
            const string entity = "Delete";
            var pathToWatch = Path.Combine(Directory.GetCurrentDirectory(),
                "Commands", "Model", "TestData", "FakeModel", "Model");
            var fileToDelete = Path.Combine(pathToWatch, "Agent", $"{entity}.json");
            if (!File.Exists(fileToDelete))
                File.Create(fileToDelete).Close();
            var apiClientMock = MockApiClient();
            apiClientMock.Setup(s => s.Delete($"/api/v1/{Tenant}/{Environment}/model/Agent/{entity}"))
                      .ReturnsAsync(new ApiResponse(true, HttpStatusCode.OK));

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
    }*/
    }
}
