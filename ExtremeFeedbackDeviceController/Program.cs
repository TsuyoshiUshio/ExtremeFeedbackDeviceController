using System;
using System.Net.Http;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace ExtremeFeedbackDeviceController
{
    class Program
    {
        private static Timer _timer;
        private static ServiceProvider _serviceProvider;
        static void Main(string[] args)
        {
            ServiceCollection service = new ServiceCollection();
            service.AddSingleton<Config>(Config.Initialize());
            service.AddHttpClient<IStatusRepository, StatusRepository>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(GetRetryPolicy());
            service.AddHttpClient<IExtremeFeedbackDeviceRepository, HueExtremeFeedbackDeviceRepository>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(GetRetryPolicy());
            service.AddSingleton<IHealthCheckService, HealthCheckService>();
            _serviceProvider = service.BuildServiceProvider();

            _timer = new System.Timers.Timer
            {
                Interval = 1000 * 60 * 10  // TODO should be 60 (min)
            };

            _timer.Elapsed += OnTimerEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            Console.WriteLine("Press the enter key to exit anytime");
            Console.ReadLine();
        }
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var jitterier = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                                      + TimeSpan.FromMilliseconds(jitterier.Next(0, 100)),
                    onRetry: (response, delay, retryCount, context) =>
                    {
                        Console.WriteLine($"Retrying: StatusCode: {response.Result.StatusCode} Message: {response.Result.ReasonPhrase} RequestUri: {response.Result.RequestMessage.RequestUri}");
                    });
        }

        private static void OnTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine($"Fire: {e.SignalTime}");
            var service = _serviceProvider.GetService<IHealthCheckService>();
            service.ExecuteAsync().GetAwaiter().GetResult();
        }
    }
}
