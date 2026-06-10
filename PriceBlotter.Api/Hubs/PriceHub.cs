using Microsoft.AspNetCore.SignalR;
using PriceBlotter.Api.Services;

namespace PriceBlotter.Api.Hubs;

public class PriceHub(PriceService priceService, ILogger<PriceHub> logger) : Hub
{
    public async Task SubscribeToPrices()
    {
        logger.LogInformation("PriceHub.SubscribeToPrices() Client {ConnectionId} subscribed to prices", Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "prices");
        await Clients.Caller.SendAsync("ReceiveInitialPrices", priceService.Items);
    }

    public async Task UnsubscribeFromPrices()
    {
        logger.LogInformation("PriceHub.UnsubscribeFromPrices() Client {ConnectionId} unsubscribed from prices", Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "prices");
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("PriceHub.OnConnectedAsync() Client {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("PriceHub.OnDisconnectedAsync()  Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
