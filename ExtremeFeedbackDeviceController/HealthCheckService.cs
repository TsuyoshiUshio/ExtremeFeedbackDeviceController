using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExtremeFeedbackDeviceController
{
    public interface IHealthCheckService
    {
        Task ExecuteAsync();
    }
    public class HealthCheckService : IHealthCheckService
    {
        private const int RED = 64919;
        private const int GREEN = 24726;
        private const int ORANGE = 5653;
        private const int NORMAL = 8418;
        private readonly Config _config;
        private readonly IStatusRepository _statusRepository;
        private readonly IExtremeFeedbackDeviceRepository _extremeFeedbackDeviceRepository;
        public HealthCheckService(Config config, IStatusRepository statusRepository, IExtremeFeedbackDeviceRepository extremeFeedbackDeviceRepository)
        {
            this._config = config;
            this._statusRepository = statusRepository;
            this._extremeFeedbackDeviceRepository = extremeFeedbackDeviceRepository;
        }
        public async Task ExecuteAsync()
        {
            IEnumerable<Target> targets = await _statusRepository.DownloadStateAsync(_config.ContainerUrl);
            var totalState = new HashSet<LightState>();

            foreach (var target in targets)
            {
                var lastTarget = new Target(); 
                if (File.Exists(target.FileName))
                {
                    var content = await File.ReadAllTextAsync(target.FileName);
                    lastTarget = JsonConvert.DeserializeObject<Target>(content);
                }

                switch(target.Status)
                {
                    case State.Success:
                        if (lastTarget.Status == State.Success)
                        {
                            totalState.Add(LightState.Normal);
                            break;
                        }
                        else
                        {
                            totalState.Add(LightState.Green);
                            break;
                        }
                    case State.Fail:
                        totalState.Add(LightState.Red);
                        break;
                    case State.Warning:
                        totalState.Add(LightState.Yellow);
                        break;
                }
            }

            if (totalState.Contains(LightState.Red))
            {
                await _extremeFeedbackDeviceRepository.SetColorAsync(RED);
            } else if (totalState.Contains(LightState.Yellow))
            {
                await _extremeFeedbackDeviceRepository.SetColorAsync(ORANGE);
            } else if (totalState.Contains(LightState.Green))
            {
                await _extremeFeedbackDeviceRepository.SetColorAsync(GREEN);
            }
            else
            {
                await _extremeFeedbackDeviceRepository.SetColorAsync(NORMAL);
            }
        }
    }
}
