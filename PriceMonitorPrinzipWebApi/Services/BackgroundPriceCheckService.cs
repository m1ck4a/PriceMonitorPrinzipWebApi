using Microsoft.EntityFrameworkCore;
using PriceMonitorPrinzipWebApi.Data;
using PriceMonitorPrinzipWebApi.Services.Interfaces;

namespace PriceMonitorPrinzipWebApi.Services
{
    public class BackgroundPriceCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundPriceCheckService> _logger;

        public BackgroundPriceCheckService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundPriceCheckService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Price check service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckPrices();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in price check service");
                }

                // Проверяем каждые 5 минут, чтобы дебажить мне было легче
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task CheckPrices()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var scraper = scope.ServiceProvider.GetRequiredService<IPriceScraper>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var subscriptions = await context.Subscriptions.ToListAsync();

            foreach (var sub in subscriptions)
            {
                try
                {
                    var newPrice = await scraper.GetPriceAsync(sub.AdUrl);

                    if (newPrice != sub.CurrentPrice)
                    {
                        decimal oldPrice = sub.CurrentPrice;

                        sub.CurrentPrice = newPrice;
                        sub.LastChecked = DateTime.UtcNow;

                        await context.SaveChangesAsync();

                        _logger.LogInformation(
                            "Цена изменилась с ссылки {AdUrl}: {OldPrice} -> {NewPrice}",
                            sub.AdUrl, oldPrice, newPrice);

                        await emailService.SendPriceChangeEmailAsync(
                            sub.Email, sub.AdUrl, oldPrice, newPrice);
                    }
                    else
                    {
                        sub.LastChecked = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка проверки на изменение цены по ссылке {AdUrl}", sub.AdUrl);
                }
            }
        }
    }
}
