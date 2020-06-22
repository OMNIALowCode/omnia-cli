using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Omnia.CLI.Infrastructure;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.CLI.Commands.Model.Import
{
    public class DirectoryImporter
    {
        private readonly string _tenant;
        private readonly string _environment;
        private readonly string _path;
        private readonly IApiClient _apiClient;

        public DirectoryImporter(IApiClient apiClient, string tenant, string environment, string path)
        {
            _tenant = tenant;
            _environment = environment;
            _apiClient = apiClient;
            _path = path;

        }

        public void Watch()
        {
            var watcher =
                new FileSystemWatcher(_path) { IncludeSubdirectories = true, NotifyFilter = NotifyFilters.LastWrite };
            watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;
            watcher.Error += Watcher_Error;

            watcher.EnableRaisingEvents = true;
        }

        public event FileSystemEventHandler OnFileChange;

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File has been changed {e.FullPath}");

            PatchEntity(_tenant, _environment, Path.GetFileNameWithoutExtension(e.Name),
                    File.ReadAllText(e.FullPath))
                .GetAwaiter().GetResult();

            OnFileChange?.Invoke(this, e);
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }




        private async Task<bool> PatchEntity(string tenant, string environment, string entity, string newJson)
        {
            var (success, currentJson) = await
                _apiClient.Get($"/api/v1/{tenant}/{environment}/model/{entity}");

            if (!success)
                return false;

            var patch = new JsonDiffPatch().Diff(currentJson, newJson);

            //TODO: Send ETAG
            var response = await _apiClient.Patch($"/api/v1/{tenant}/{environment}/model/{entity}",
                new StringContent(patch, Encoding.UTF8, "application/json"));

            return response.Success;
        }

        //TODO: Extract to a common area
        private static async Task<ApiError> GetErrorFromApiResponse(HttpResponseMessage response)
            => JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());
    }
}
