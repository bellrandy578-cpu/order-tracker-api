OrderTracker – Order Status Tracking Web API with ASP.NET Core
A modern, controller-based web API built with ASP.NET Core to track order status with full CRUD operations, in-memory database, order history, Swagger UI, and realistic dummy data seeding.
Ideal for learning Entity Framework relationships, DTOs, dependency injection, logging, and OpenAPI (Swagger).

## Quick Start

```bash
git clone https://github.com/bellrandy578-cpu/OrderTracker-api.git
cd OrderTracker-api
dotnet run

Features

FeatureDescriptionOrder TrackingCreate, read, update, delete orders with statusOrder HistoryTrack status changes with timestamps and userIn-Memory DBMicrosoft.EntityFrameworkCore.InMemory – zero setupSwagger UIInteractive docs at /swaggerDummy Data7 orders + 3 history entries seeded on startupDTO PatternSecure, clean API surfaceError LoggingSeeding failures logged safely

API Endpoints
Orders (/api/orders)



MethodEndpointDescriptionGET/api/ordersGet all ordersGET/api/orders/{id}Get order by IDPOST/api/ordersCreate new orderPUT/api/orders/{id}Update orderDELETE/api/orders/{id}Delete order
Order History (/api/orderhistory)



MethodEndpointDescriptionGET/api/orderhistoryGet all status changesGET/api/orderhistory/{id}Get history by ID

Project Structure
textOrderTracker/
│
├── Controllers/
│   ├── OrdersController.cs
│   └── OrderHistoryController.cs
│
├── Models/
│   ├── Order.cs
│   ├── OrderHistory.cs
│   ├── OrderDTO.cs
│   ├── OrderHistoryDTO.cs
│   └── OrderContext.cs
│
├── Program.cs                     # Startup + DI + seeding + logging
│
└── OrderTracker.csproj

Prerequisites

.NET 8 SDK
Visual Studio 2022+ or VS Code + C# Dev Kit


Getting Started
1. Clone & Navigate
bash git clone https://github.com/yourusername/OrderTracker.git
cd OrderTracker
2. Restore Packages
bash dotnet restore
3. Run the App
bash dotnet run

App runs on https://localhost:7xxx and http://localhost:5xxx


Test with Swagger UI

Open:
https://localhost:7xxx/swagger
Try:

GET → /api/orders → See 7 seeded orders
GET → /api/orderhistory → See 3 status changes
POST → Add a new order
PUT → Change status of an order


Seeded Dummy Data
Orders (OrderItems)



IDProductStatusOrder DateAmount1Sunglassespending2025-09-2332Pursedelivered2025-06-0313Watchpending2025-07-1314Dressshipped2025-03-0345Slacksdelivered2025-06-0436Tiedelivered2025-08-2817Beltcanceled2025-05-232
Order History (OrderHistoryItems)



IDOrderIdStatusChanged AtChanged By15shipped2025-09-24Rob25delivered2025-09-28Rob37cancelled2025-09-24Susan

Key Code: Data Seeding (Program.cs)
csharpstatic async Task SeedDataAsync(WebApplication webApp)
{
    await using var scope = webApp.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (!await db.OrderItems.AnyAsync())
        {
            var orders = new[]
            {
                new Order { Product = "Sunglasses", Status = "pending",   OrderDate = new DateOnly(2025, 9, 23), Amount = 3 },
                new Order { Product = "Purse",      Status = "delivered", OrderDate = new DateOnly(2025, 6, 3),  Amount = 1 },
                new Order { Product = "Watch",      Status = "pending",   OrderDate = new DateOnly(2025, 7, 13), Amount = 1 },
                new Order { Product = "Dress",      Status = "shipped",   OrderDate = new DateOnly(2025, 3, 3),  Amount = 4 },
                new Order { Product = "Slacks",     Status = "delivered", OrderDate = new DateOnly(2025, 6, 4),  Amount = 3 },
                new Order { Product = "Tie",        Status = "delivered", OrderDate = new DateOnly(2025, 8, 28), Amount = 1 },
                new Order { Product = "Belt",       Status = "canceled",  OrderDate = new DateOnly(2025, 5, 23), Amount = 2 },
            };
            await db.OrderItems.AddRangeAsync(orders);
        }

        if (!await db.OrderHistoryItems.AnyAsync())
        {
            var history = new[]
            {
                new OrderHistory { OrderId = 5, Status = "shipped",   ChangedAt = new DateOnly(2025, 9, 24), ChangedBy = "Rob" },
                new OrderHistory { OrderId = 5, Status = "delivered", ChangedAt = new DateOnly(2025, 9, 28), ChangedBy = "Rob" },
                new OrderHistory { OrderId = 7, Status = "cancelled", ChangedAt = new DateOnly(2025, 9, 24), ChangedBy = "Susan" },
            };
            await db.OrderHistoryItems.AddRangeAsync(history);
        }

        await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Seeding failed.");
    }
}

Sample cURL Commands
bash# Get all orders
curl https://localhost:7260/api/orders

# Create new order
curl -X POST https://localhost:7260/api/orders \
  -H "Content-Type: application/json" \
  -d '{"product":"Shoes","status":"pending","orderDate":"2025-10-01","amount":1}'

Development Notes

In-Memory DB: Data resets on restart
Hot Reload: Edit & save → instant updates
Trust SSL: Accept dev certificate on first run
DTOs: Prevent over-posting & hide internal fields


Future Enhancements

 Add SQL Server support
 JWT Authentication
 Email notifications on status change
 Frontend with React/Vue
 Filtering by status/date


Deploy to Azure
bashdotnet publish -c Release
# Deploy bin/Release/net8.0/publish/ to Azure App Service

Deploy Guide


Learning Resources

ASP.NET Core Web API Tutorial
EF Core In-Memory Database
Swagger/OpenAPI


License
MIT License – Free to use, modify, and share.

Built with .NET 8 • ASP.NET Core • EF Core • Swagger

Track orders like a pro — from pending to delivered!
Extend this project with real databases, auth, and frontend!