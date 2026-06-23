using backEnd.Data;
using backEnd.DTOs;
using backEnd.Interfaces;
using backEnd.Models;
using Microsoft.EntityFrameworkCore;

using backEnd.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace backEnd.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IActivityService _activityService;
    private readonly IHubContext<StockHub> _stockHub;
    private readonly INotificationService _notificationService;

    public OrderService(AppDbContext context, IActivityService activityService, IHubContext<StockHub> stockHub, INotificationService notificationService)
    {
        _context = context;
        _activityService = activityService;
        _stockHub = stockHub;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            TotalAmount = o.TotalAmount,
            OrderDate = o.OrderDate,
            Status = o.Status,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        });
    }

    public async Task<OrderDto?> GetOrderByIdAsync(long orderId, string userId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null) return null;

        return new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(string userId, CreateOrderDto dto)
    {
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        // 1. Process Stock Reductions & Collect Owner Emails
        Console.WriteLine($"[ORDER]: Processing stock for {cartItems.Count} items...");
        var notifications = new List<(string Email, string Product, int Qty, decimal Price)>();

        foreach (var item in cartItems)
        {
            if (string.Equals(item.ProductType, "Machinery", StringComparison.OrdinalIgnoreCase))
            {
                var product = await _context.Machineries.FirstOrDefaultAsync(m => m.Id == item.ProductId);
                if (product != null)
                {
                    if (product.Quantity < item.Quantity)
                    {
                        Console.WriteLine($"[ORDER]: Insufficient stock for {product.Name}. Have: {product.Quantity}, Need: {item.Quantity}");
                        throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {product.Quantity}");
                    }
                    
                    product.Quantity -= item.Quantity;
                    _context.Machineries.Update(product);

                    // Track for notification
                    notifications.Add((product.OwnerEmail, product.Name, item.Quantity, item.Price));

                    Console.WriteLine($"[ORDER]: Stock reduced for {product.Name}. New quantity: {product.Quantity}");
                }
            }
            else if (string.Equals(item.ProductType, "AgriItem", StringComparison.OrdinalIgnoreCase))
            {
                var product = await _context.AgriItems.FirstOrDefaultAsync(m => m.Id == item.ProductId);
                if (product != null)
                {
                    if (product.Quantity < item.Quantity)
                    {
                        Console.WriteLine($"[ORDER]: Insufficient stock for {product.Name}. Have: {product.Quantity}, Need: {item.Quantity}");
                        throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {product.Quantity}");
                    }
                    
                    product.Quantity -= item.Quantity;
                    _context.AgriItems.Update(product);

                    // Track for notification
                    notifications.Add((product.OwnerEmail, product.Name, item.Quantity, item.Price));

                    Console.WriteLine($"[ORDER]: Stock reduced for {product.Name}. New quantity: {product.Quantity}");
                }
            }
        }

        // 2. Create Order
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Completed",
            TransactionId = dto.TransactionId,
            ShippingAddress = dto.ShippingAddress,
            TotalAmount = cartItems.Sum(c => c.Price * c.Quantity),
            Items = cartItems.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.ProductName,
                Price = c.Price,
                Quantity = c.Quantity,
                ImageUrl = c.ImageUrl,
                ProductType = c.ProductType ?? "Machinery"
            }).ToList()
        };

        try
        {
            Console.WriteLine("[ORDER]: Saving order and stock updates to DB...");
            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[ORDER]: Success. Order ID: {order.Id}");

            var frontendUrl = (Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "https://farmease-24.netlify.app").TrimEnd('/');
            // ─── NOTIFICATION LOGIC ───
            foreach (var n in notifications)
            {
                _ = Task.Run(async () => {
                    try {
                        var body = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e0e0e0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                <div style='background: #16a34a; padding: 30px; text-align: center;'>
                                    <h1 style='color: white; margin: 0; font-size: 24px;'>New Sale on FarmEase! 🎉</h1>
                                </div>
                                <div style='padding: 30px; line-height: 1.6; color: #333;'>
                                    <p style='font-size: 18px;'>Congratulations! You've just sold a product.</p>
                                    <div style='background: #f8fafc; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                        <p style='margin: 5px 0;'><strong>📦 Product:</strong> {n.Product}</p>
                                        <p style='margin: 5px 0;'><strong>🔢 Quantity:</strong> {n.Qty}</p>
                                        <p style='margin: 5px 0;'><strong>💰 Total Earned:</strong> <span style='color: #16a34a; font-size: 18px; font-weight: bold;'>₹{n.Qty * n.Price}</span></p>
                                    </div>
                                    <p style='color: #64748b; font-size: 14px;'>The order details have been added to your Farmer Dashboard. Please prepare the items for dispatch.</p>
                                    <div style='text-align: center; margin-top: 30px;'>
                                        <a href='{frontendUrl}/#/my-sales' style='background: #16a34a; color: white; padding: 12px 30px; text-decoration: none; border-radius: 30px; font-weight: bold;'>View Dashboard</a>
                                    </div>
                                </div>
                                <div style='background: #f1f5f9; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8;'>
                                    <p style='margin: 0;'>FarmEase - Empowering Farmers, Connecting Communities</p>
                                </div>
                            </div>";
                        await _notificationService.SendEmailAsync(n.Email, $"FarmEase Sale: {n.Product}", body);
                    } catch (Exception ex) {
                        Console.WriteLine($"[EMAIL FAILED]: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ORDER CRITICAL ERROR]: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"INNER: {ex.InnerException.Message}");
            throw;
        }

        // 3. SignalR Broadcast (Non-blocking)
        _ = Task.Run(async () =>
        {
            try
            {
                foreach (var item in cartItems)
                {
                    await _stockHub.Clients.All.SendAsync("ReceiveStockUpdate", item.ProductId, item.Quantity, item.ProductType);
                }
                Console.WriteLine("[ORDER]: SignalR broadcast complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ORDER]: SignalR broadcast failed: {ex.Message}");
            }
        });

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            await _activityService.LogActivityAsync("Order", $"New order placed from cart: ₹{order.TotalAmount}. Stock updated.", user.Email, user.FullName);
        }

        return new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }

    public async Task<OrderDto> CreateOrderDirectlyAsync(string userId, CreateOrderDto dto)
    {
        var items = dto.Items ?? new List<AddToCartDto>();
        if (!items.Any())
            throw new InvalidOperationException("No items provided.");

        // 1. Process Stock Reductions & Collect Owner Emails
        Console.WriteLine($"[DIRECT ORDER]: Processing stock for {items.Count} items...");
        var notifications = new List<(string Email, string Product, int Qty, decimal Price)>();

        foreach (var item in items)
        {
            if (string.Equals(item.ProductType, "Machinery", StringComparison.OrdinalIgnoreCase))
            {
                var product = await _context.Machineries.FirstOrDefaultAsync(m => m.Id == item.ProductId);
                if (product != null)
                {
                    if (product.Quantity < item.Quantity)
                    {
                        Console.WriteLine($"[DIRECT ORDER]: Insufficient stock for {product.Name}. Have: {product.Quantity}, Need: {item.Quantity}");
                        throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {product.Quantity}");
                    }
                    
                    product.Quantity -= item.Quantity;
                    _context.Machineries.Update(product);
                    
                    // Track for notification
                    notifications.Add((product.OwnerEmail, product.Name, item.Quantity, item.Price));

                    Console.WriteLine($"[DIRECT ORDER]: Stock reduced for {product.Name}. New quantity: {product.Quantity}");
                }
            }
            else if (string.Equals(item.ProductType, "AgriItem", StringComparison.OrdinalIgnoreCase))
            {
                var product = await _context.AgriItems.FirstOrDefaultAsync(m => m.Id == item.ProductId);
                if (product != null)
                {
                    if (product.Quantity < item.Quantity)
                    {
                        Console.WriteLine($"[DIRECT ORDER]: Insufficient stock for {product.Name}. Have: {product.Quantity}, Need: {item.Quantity}");
                        throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {product.Quantity}");
                    }
                    
                    product.Quantity -= item.Quantity;
                    _context.AgriItems.Update(product);
                    
                    // Track for notification
                    notifications.Add((product.OwnerEmail, product.Name, item.Quantity, item.Price));

                    Console.WriteLine($"[DIRECT ORDER]: Stock reduced for {product.Name}. New quantity: {product.Quantity}");
                }
            }
        }

        // 2. Create Order
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Completed",
            TransactionId = dto.TransactionId,
            ShippingAddress = dto.ShippingAddress,
            TotalAmount = items.Sum(i => i.Price * i.Quantity),
            Items = items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl,
                ProductType = i.ProductType ?? "Machinery"
            }).ToList()
        };

        try
        {
            Console.WriteLine("[DIRECT ORDER]: Saving to DB...");
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[DIRECT ORDER]: Success. Order ID: {order.Id}");
            
            var frontendUrl = (Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "https://farmease-24.netlify.app").TrimEnd('/');
            // ─── NOTIFICATION LOGIC ───
            foreach (var n in notifications)
            {
                _ = Task.Run(async () => {
                    try {
                        var body = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e0e0e0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                                <div style='background: #16a34a; padding: 30px; text-align: center;'>
                                    <h1 style='color: white; margin: 0; font-size: 24px;'>New Sale on FarmEase! 🎉</h1>
                                </div>
                                <div style='padding: 30px; line-height: 1.6; color: #333;'>
                                    <p style='font-size: 18px;'>Congratulations! You've just sold a product.</p>
                                    <div style='background: #f8fafc; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                        <p style='margin: 5px 0;'><strong>📦 Product:</strong> {n.Product}</p>
                                        <p style='margin: 5px 0;'><strong>🔢 Quantity:</strong> {n.Qty}</p>
                                        <p style='margin: 5px 0;'><strong>💰 Total Earned:</strong> <span style='color: #16a34a; font-size: 18px; font-weight: bold;'>₹{n.Qty * n.Price}</span></p>
                                    </div>
                                    <p style='color: #64748b; font-size: 14px;'>The order details have been added to your Farmer Dashboard. Please prepare the items for dispatch.</p>
                                    <div style='text-align: center; margin-top: 30px;'>
                                        <a href='{frontendUrl}/#/my-sales' style='background: #16a34a; color: white; padding: 12px 30px; text-decoration: none; border-radius: 30px; font-weight: bold;'>View Dashboard</a>
                                    </div>
                                </div>
                                <div style='background: #f1f5f9; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8;'>
                                    <p style='margin: 0;'>FarmEase - Empowering Farmers, Connecting Communities</p>
                                </div>
                            </div>";
                        await _notificationService.SendEmailAsync(n.Email, $"FarmEase Sale: {n.Product}", body);
                    } catch (Exception ex) {
                        Console.WriteLine($"[EMAIL FAILED]: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DIRECT ORDER CRITICAL ERROR]: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"INNER: {ex.InnerException.Message}");
            throw;
        }

        // 3. SignalR Broadcast (Non-blocking)
        _ = Task.Run(async () =>
        {
            try
            {
                foreach (var item in items)
                {
                    await _stockHub.Clients.All.SendAsync("ReceiveStockUpdate", item.ProductId, item.Quantity, item.ProductType);
                }
                Console.WriteLine("[DIRECT ORDER]: SignalR broadcast complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DIRECT ORDER]: SignalR broadcast failed: {ex.Message}");
            }
        });

        return new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

        return orders.Select(o => 
        {
            var user = users.GetValueOrDefault(o.UserId);
            return new OrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate,
                Status = o.Status,
                RequesterName = user?.FullName,
                RequesterEmail = user?.Email,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl,
                    ProductType = i.ProductType
                }).ToList()
            };
        });
    }

    public async Task<IEnumerable<OrderItemDto>> GetFarmerOrdersAsync(string farmerEmail)
    {
        // Load all users once
        var users = await _context.Users
            .ToDictionaryAsync(x => x.Id);

        // Load order items with orders
        var orderItems = await _context.OrderItems
            .Include(x => x.Order)
            .ToListAsync();

        var result = new List<OrderItemDto>();

        foreach (var oi in orderItems)
        {
            var type = oi.ProductType?.ToLower();

            // =========================
            // MACHINERY
            // =========================

            if (type == "machinery")
            {
                var machinery = await _context.Machineries
                    .FirstOrDefaultAsync(x => x.Id == oi.ProductId);

                if (machinery == null)
                    continue;

                if (machinery.OwnerEmail != farmerEmail)
                    continue;

                var user = await _context.Users
     .FirstOrDefaultAsync(x => x.Email == oi.Order.UserId);

                Console.WriteLine($"ORDER USER ID: {oi.Order.UserId}");
                Console.WriteLine($"CUSTOMER EMAIL: {user?.Email}");

                result.Add(new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    ImageUrl = oi.ImageUrl,
                    ProductType = oi.ProductType,

                    OrderId = oi.OrderId,
                    OrderDate = oi.Order.OrderDate,

                    CustomerEmail = user?.Email,
                    CustomerName = user?.FullName,
                    CustomerPhone = user?.Phone,

                    CustomerAddress = oi.Order.ShippingAddress,

                    Category = machinery.Category,
                    StockLeft = machinery.Quantity
                });
            }

            // =========================
            // AGRI ITEM
            // =========================

            else if (type == "agriitem")
            {
                var agri = await _context.AgriItems
                    .FirstOrDefaultAsync(x => x.Id == oi.ProductId);

                if (agri == null)
                    continue;

                if (agri.OwnerEmail != farmerEmail)
                    continue;

                var user = await _context.Users
    .FirstOrDefaultAsync(x => x.Email == oi.Order.UserId);

                Console.WriteLine($"ORDER USER ID: {oi.Order.UserId}");
                Console.WriteLine($"CUSTOMER EMAIL: {user?.Email}");

                result.Add(new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    ImageUrl = oi.ImageUrl,
                    ProductType = oi.ProductType,

                    OrderId = oi.OrderId,
                    OrderDate = oi.Order.OrderDate,

                    CustomerEmail = user?.Email,
                    CustomerName = user?.FullName,
                    CustomerPhone = user?.Phone,

                    CustomerAddress = oi.Order.ShippingAddress,

                    Category = agri.Category,
                    StockLeft = agri.Quantity
                });
            }
        }

        Console.WriteLine($"TOTAL SALES FOUND: {result.Count}");

        return result
            .OrderByDescending(x => x.OrderDate)
            .ToList();
    }
}
