using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HarvestHub.WebApp.Services
{
    public class MarketRateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _sp;

        public MarketRateBackgroundService(IServiceProvider sp)
        {
            _sp = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // On startup run once immediately (optional)
            await RunUpdateOnceAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Wait for 24 hours
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                    await RunUpdateOnceAsync(stoppingToken);
                }
                catch (TaskCanceledException) { /* stopping */ }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Background service error: {ex.Message}");
                }
            }
        }

        private async Task RunUpdateOnceAsync(CancellationToken token)
        {
            using var scope = _sp.CreateScope();
            var updater = scope.ServiceProvider.GetRequiredService<MarketRateUpdater>();
            await updater.UpdateMarketRatesAsync();
        }
    }
}
