using Microsoft.AspNetCore.SignalR;

namespace backEnd.Hubs;

public class StockHub : Hub
{
    // Live stock reduction (delta)
    public async Task SendStockUpdate(long productId, int quantity, string productType)
    {
        await Clients.All.SendAsync("ReceiveStockUpdate", productId, quantity, productType);
    }

    // Absolute stock update
    public async Task SendAbsoluteStock(long productId, int absoluteQuantity, string productType)
    {
        await Clients.All.SendAsync("ReceiveAbsoluteStock", productId, absoluteQuantity, productType);
    }

    // Generic product attribute update (Price, Name, etc.)
    public async Task SendProductUpdate(object product, string productType)
    {
        await Clients.All.SendAsync("ReceiveProductUpdate", product, productType);
    }
}
