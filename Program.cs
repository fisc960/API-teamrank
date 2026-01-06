using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GemachApp.Data;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// PORT (LOCAL + RAILWAY)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});
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

// --------------------
// CONNECTION STRING
if (provider == "postgres")
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? throw new Exception("DATABASE_URL is required");

    connectionString = ConvertPostgresUrlToConnectionString(dbUrl);
    Console.WriteLine("Using PostgreSQL connection");
}
else
{
    connectionString =
        builder.Configuration.GetConnectionString("ApplicationDbcontext")
        ?? throw new Exception("SQL Server connection string missing");

    Console.WriteLine("Using SQL Server connection");
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
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "https://team-rank-banking.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();


// ============================
// MIGRATIONS + SAFE SEED
// ============================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("🚀 Running database migrations...");

    context.Database.SetCommandTimeout(120);
    context.Database.Migrate();

    Console.WriteLine("✅ Migrations completed");

    // Seed admin ONLY if table exists AND empty
    if (!context.Admins.Any())
    {
        var hasher = new PasswordHasher<Admin>();

        var admin = new Admin
        {
            Name = "Admin"
        };

        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        context.Admins.Add(admin);
        context.SaveChanges();

        Console.WriteLine("✅ Admin seeded");
    }
}

// --------------------
// MIDDLEWARE
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));

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
        $"Trust Server Certificate=true;" +
        $"Timeout=60;" +
        $"Command Timeout=60;" +
        $"Pooling=true;";
}





