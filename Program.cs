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
    Environment.GetEnvironmentVariable("DB_PROVIDER")?.ToLower()
    ?? builder.Configuration["DB_PROVIDER"]?.ToLower()
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
        builder.Configuration.GetConnectionString("ApplicationDbcontext")
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
app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        Console.WriteLine("🚀 Running database migrations...");
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Migrations completed");

        if (!await context.Admins.AnyAsync())
        {
            var hasher = new PasswordHasher<Admin>();
            var admin = new Admin { Name = "Admin" };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
            context.Admins.Add(admin);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Admin seeded");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex}");
    }
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


