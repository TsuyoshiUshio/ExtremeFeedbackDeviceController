using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeFeedbackDeviceController
{
    public interface IExtremeFeedbackDeviceRepository
    {
        Task SetColorAsync(int color);
    }
    public class HueExtremeFeedbackDeviceRepository : IExtremeFeedbackDeviceRepository
    {
        private readonly HttpClient _client;
        private readonly Config _config;
        public HueExtremeFeedbackDeviceRepository(HttpClient client, Config config)
        {
            this._client = client;
            this._config = config;
        }

        public async Task SetColorAsync(int color)
        {
            var content = new StringContent($"{{\"on\":true, \"sat\":254, \"bri\":254, \"hue\":{color}}}");
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            for (int lampNumber = 1; lampNumber <= _config.NumberOfLumps; lampNumber++)
            {
                var url = $"{_config.HueBridgeIp}/api/{_config.HueUserName}/lights/{lampNumber}/state";
                var response = await _client.PutAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"Url: {url}, StatusCode: {response.StatusCode} Reason: {response.ReasonPhrase}");
                }
            }
        }
    }
}
