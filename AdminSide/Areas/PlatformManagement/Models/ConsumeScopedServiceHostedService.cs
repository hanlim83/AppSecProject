using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdminSide.Areas.PlatformManagement.Models
{
    internal class ConsumeScopedServiceHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;

        public ConsumeScopedServiceHostedService(ILogger<ConsumeScopedServiceHostedService> logger, IServiceProvider services)
        {
            _logger = logger;
            Services = services;
        }
        public IServiceProvider Services { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Update Background Service is starting.");

            _timer = new Timer(DoWorkAsync, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private async void DoWorkAsync(object state)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedProcessingService>();

                await scopedProcessingService.DoWorkAsync();
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Update Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
