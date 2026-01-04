using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using OrderTracker.Models;
using OrderTracker.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUserService, InMemoryUserService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")  // Allow Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT Setup
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

Console.WriteLine($"Key size in bytes: {key.KeySize}");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

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
    // Seed users once at startup
    await SeedUsersAsync(app);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// /api/auth/login
app.MapPost("/api/auth/login", async (LoginRequest request, IUserService userService, IConfiguration config) =>
{
    // Optional debug logging (remove later if you want)
    // Console.WriteLine($"Login attempt for username: '{request.Username}'");

    var user = await userService.GetByUsernameAsync(request.Username);

    // Optional debug logging
    // Console.WriteLine($"User found: {user != null}");
    // if (user != null) Console.WriteLine($"Password verify result: {VerifyPassword(request.Password, user.PasswordHash)}");

    if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        return Results.Unauthorized();

    var jwtSettings = config.GetSection("Jwt");
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Name, user.Username),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpiryMinutes")),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    return Results.Ok(new
    {
        token = new JwtSecurityTokenHandler().WriteToken(token),
        expires = token.ValidTo,
        username = user.Username,
        role = user.Role
    });
})
.Accepts<LoginRequest>("application/json")
.Produces(200)
.Produces(401)
.WithName("Login");


// Protected endpoints
app.MapGet("/api/secret", (HttpContext ctx) =>
    $"Hello {ctx.User.Identity?.Name ?? "anonymous"} — you are authenticated!")
    .RequireAuthorization();

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



static async Task SeedUsersAsync(WebApplication webApp)
{
    var userService = webApp.Services.GetRequiredService<IUserService>() as InMemoryUserService;

    // Only seed if empty
    if (userService is { } service && await service.GetByUsernameAsync("admin") is null)
    {
        var admin = new AppUser
        {
            Username = "admin",
            PasswordHash = HashPassword("Password123!"),
            Role = "Admin"
        };

        var regular = new AppUser
        {
            Username = "john",
            PasswordHash = HashPassword("MySecret123"),
            Role = "User"
        };

        service.AddUser(admin);
        service.AddUser(regular);

        webApp.Logger.LogInformation("In-memory users seeded: admin (Password123!), john (MySecret123)");
    }
}
static string HashPassword(string password)
{
    byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // 16-byte salt

    byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
        password: password,
        salt: salt,
        iterations: 100_000,
        hashAlgorithm: HashAlgorithmName.SHA256,
        outputLength: 256 / 8); // 32-byte hash

    // Combine salt + hash into one string: "base64(salt):base64(hash)"
    return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
}

static bool VerifyPassword(string password, string storedHash)
{
    var parts = storedHash.Split(':');
    if (parts.Length != 2) return false;

    var salt = Convert.FromBase64String(parts[0]);
    var storedHashBytes = Convert.FromBase64String(parts[1]);

    var testHash = Rfc2898DeriveBytes.Pbkdf2(
        password,
        salt,
        100_000,
        HashAlgorithmName.SHA256,
        storedHashBytes.Length);

    return CryptographicOperations.FixedTimeEquals(testHash, storedHashBytes);
}

