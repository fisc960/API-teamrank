using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GemachApp.Data;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(int.Parse(port)));
Console.WriteLine($"Backend listening on port {port}");

// --------------------
// CONFIG
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

// --------------------
// DB PROVIDER
var provider =
    !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL"))
        ? "postgres"
        : builder.Configuration["DB_PROVIDER"]?.ToLower()
            ?? "sqlserver";

Console.WriteLine($"DB_PROVIDER: {provider}");

string connectionString;

if (provider == "postgres")
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? throw new Exception("DATABASE_URL is required");

    connectionString = ConvertPostgresUrlToConnectionString(dbUrl);
}
else
{
    connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("SQL Server connection string missing");
}

// --------------------
// DB CONTEXT
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (provider == "postgres")
        options.UseNpgsql(connectionString);
    else
        options.UseSqlServer(connectionString);
});

// --------------------
// SERVICES
builder.Services.AddControllers();
builder.Services.AddCors(p =>
{
    p.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// --------------------
// MIDDLEWARE
app.UseRouting();
app.UseCors("AllowAll");
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));

// ============================
// RUN MIGRATIONS *AFTER STARTUP*
// ============================
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.Migrate();

    if (!context.Admins.Any())
    {
        var hasher = new PasswordHasher<Admin>();
        var admin = new Admin { Name = "admin" };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
        context.Admins.Add(admin);
        context.SaveChanges();
    }
}
catch (Exception ex)
{
    Console.WriteLine("❌ MIGRATION FAILED:");
    Console.WriteLine(ex.Message);
}


// Add this BEFORE app.Run();   remove the endpoint from your code for security  until line  141
app.MapPost("/admin/reset-database", async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Drop and recreate schema
        await context.Database.ExecuteSqlRawAsync("DROP SCHEMA public CASCADE;");
        await context.Database.ExecuteSqlRawAsync("CREATE SCHEMA public;");

        // Run migrations
        await context.Database.MigrateAsync();

        // Seed admin
        if (!context.Admins.Any())
        {
            var hasher = new PasswordHasher<Admin>();
            var admin = new Admin { Name = "admin" };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
            context.Admins.Add(admin);
            await context.SaveChangesAsync();
        }

        return Results.Ok(new { message = "Database reset successfully!", timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}\n\nStack: {ex.StackTrace}");
    }
});

app.MapGet("/admin/check-users", async () =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var admins = await context.Admins.ToListAsync();
    return Results.Ok(new
    {
        count = admins.Count,
        users = admins.Select(a => new { a.Id, a.Name }).ToList()
    });
});
app.Run();

// --------------------
// HELPERS
static string ConvertPostgresUrlToConnectionString(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');

    return
        $"Host={uri.Host};" +
        $"Port={uri.Port};" +
        $"Username={userInfo[0]};" +
        $"Password={userInfo[1]};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"SslMode=Require;" +
        $"Trust Server Certificate=true;";
}


