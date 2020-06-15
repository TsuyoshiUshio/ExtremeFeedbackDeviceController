using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace ExtremeFeedbackDeviceController
{
    public interface IStatusRepository
    {
        Task<IEnumerable<Target>> DownloadStateAsync(string containerUrl);
    }
    public class StatusRepository : IStatusRepository
    {
        private Config _config;
        private HttpClient _client;
        public StatusRepository(HttpClient client, Config config)
        {
            this._config = config;
            this._client = client;
        }
        public Task<IEnumerable<Target>> DownloadStateAsync(string containerUrl)
        {
            IEnumerable<string> blobUrls = GetBlobUrls(containerUrl);
            return GetTargetsAsync(blobUrls);
        }

        private async Task<IEnumerable<Target>> GetTargetsAsync(IEnumerable<string> blobUrls)
        {
            var targets = new List<Target>();
            
            foreach (var url in blobUrls)
            {
                var fileName = url.Substring(url.LastIndexOf('/') + 1);
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var target = JsonConvert.DeserializeObject<Target>(body);
                    target.DownloadUrl = url;
                    target.FileName = fileName;
                    targets.Add(target);
                }
                else
                {
                    throw new FileNotFoundException($"Url: {url}, StatusCode: {response.StatusCode} Reason: {response.ReasonPhrase}");
                }
            }
            return targets;
        }

        private IEnumerable<string> GetBlobUrls(string containerUrl)
        {
            var container = XDocument.Load($"{containerUrl}?restype=container&comp=list");
            return container.Descendants("Blob")
                .Select(p => p.Element("Url")?.Value);
        }
    }
}
