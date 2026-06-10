using Microsoft.AspNetCore.SignalR;
using PriceBlotter.Api.Hubs;
using PriceBlotter.Api.Models;

namespace PriceBlotter.Api.Services;

public class PriceService : BackgroundService
{
    private readonly IHubContext<PriceHub> _hubContext;
    private readonly Random _random = new();
    private readonly ILogger<PriceService> _logger;

    private readonly List<PriceItem> _mockItems;

    public IReadOnlyList<PriceItem> Items => _mockItems.AsReadOnly();

    public PriceService(IHubContext<PriceHub> hubContext, ILogger<PriceService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        _mockItems = GenerateStartingPrices();

    }

    private List<PriceItem> GenerateStartingPrices()
    {
        return
        [
            new(1,  "EURUSD", Midpoint(1.1000m),  DateTime.UtcNow),
            new(2,  "GBPUSD", Midpoint(1.2700m),  DateTime.UtcNow),
            new(3,  "USDJPY", Midpoint(149.50m),  DateTime.UtcNow),
            new(4,  "AUDUSD", Midpoint(0.6500m),  DateTime.UtcNow),
            new(5,  "USDCHF", Midpoint(0.9000m),  DateTime.UtcNow),
            new(6,  "USDCAD", Midpoint(1.3600m),  DateTime.UtcNow),
            new(7,  "NZDUSD", Midpoint(0.6000m),  DateTime.UtcNow),
            new(8,  "EURGBP", Midpoint(0.8650m),  DateTime.UtcNow),
            new(9,  "EURJPY", Midpoint(161.20m),  DateTime.UtcNow),
            new(10, "GBPJPY", Midpoint(191.40m),  DateTime.UtcNow),
        ];
    }

    private decimal Midpoint(decimal mid) => Math.Round(mid + (decimal)(_random.NextDouble() * 0.01 - 0.005), 4);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceService.ExecuteAsync(CancellationToken stoppingToken) Started broadcasting prices");

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var count = _random.Next(3, 6);
                var indices = Enumerable.Range(0, _mockItems.Count)
                    .OrderBy(_ => _random.Next())
                    .Take(count)
                    .ToList();

                var changed = new List<PriceItem>();

                foreach (var i in indices)
                {
                    var item = _mockItems[i];
                    var delta = item.Name.Contains("JPY")
                        ? (decimal)(_random.NextDouble() * 0.10 - 0.05)
                        : (decimal)(_random.NextDouble() * 0.0002 - 0.0001);

                    var updated = item with
                    {
                        Price = Math.Round(item.Price + delta, 4),
                        UpdatedAt = DateTime.UtcNow
                    };

                    _mockItems[i] = updated;
                    changed.Add(updated);
                }

                await _hubContext.Clients.Group("prices")
                    .SendAsync("ReceivePriceUpdate", changed, stoppingToken);

                _logger.LogDebug("PriceService.ExecuteAsync(CancellationToken stoppingToken) Pushed {ChangedCount} price update(s): {Pairs}",
                    changed.Count, string.Join(", ", changed.Select(c => c.Name)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PriceService.ExecuteAsync() error on tick");
                }
        }
    }
}
