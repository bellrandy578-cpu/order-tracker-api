using Microsoft.EntityFrameworkCore;
using OrderTracker.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")  // Allow Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderContext>(opt =>
    opt.UseInMemoryDatabase("OrderList"));

var app = builder.Build();
app.UseCors();

// seed the data
//await SeedDataAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await SeedDataAsync(app);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedDataAsync(WebApplication webApp)
{
    await using var scope = webApp.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        //await db.Database.MigrateAsync();

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
                new Order { Product = "Belt",       Status = "canceled",   OrderDate = new DateOnly(2025, 5, 23), Amount = 2 },
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

