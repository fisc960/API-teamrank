using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GemachApp.Data;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// PORT (Render / Railway safe)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(int.Parse(port)));
Console.WriteLine($"Backend listening on port {port}");

// --------------------
// CONFIG
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

// --------------------
// DATABASE (POSTGRES ONLY)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"📍 DATABASE_URL = {databaseUrl}"); 
if (string.IsNullOrWhiteSpace(databaseUrl))
{
    throw new Exception("DATABASE_URL is missing");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        databaseUrl,
        o => o.EnableRetryOnFailure()
    );
});

// --------------------
// SERVICES
builder.Services.AddControllers();

builder.Services.AddCors(p =>
{
    p.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// --------------------
// MIDDLEWARE
app.UseRouting();
app.UseCors("AllowAll");
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));

// ============================
// CONTROLLED MIGRATIONS (SAFE)
var runMigrations =
    Environment.GetEnvironmentVariable("RUN_MIGRATIONS") == "true";

if (runMigrations)
{
    Console.WriteLine("🟢 RUN_MIGRATIONS=true — starting migrations");

    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("🔍 About to run Migrate()...");
        context.Database.Migrate();
        Console.WriteLine("✅ Database migrated");

        // ---- SAFE ADMIN SEED ----
        if (!context.Admins.Any())
        {
            var hasher = new PasswordHasher<Admin>();

            var admin = new Admin
            {
                Name = "admin"
            };

            admin.PasswordHash =
                hasher.HashPassword(admin, "Admin123!");

            context.Admins.Add(admin);
            context.SaveChanges();

            Console.WriteLine("✅ Default admin created");
        }
        else
        {
            Console.WriteLine("ℹ️ Admin already exists — skipping seed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ MIGRATION FAILED");
        Console.WriteLine($"Error Message: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"--- Inner Exception ---");
            Console.WriteLine($"Inner Type: {ex.InnerException.GetType().Name}");
            Console.WriteLine($"Inner Message: {ex.InnerException.Message}");
            Console.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
        }
    }
}
else
{
    Console.WriteLine("⏭️ RUN_MIGRATIONS=false — skipping migrations");
}

app.Run();

