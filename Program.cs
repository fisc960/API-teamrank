using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;

var builder = WebApplication.CreateBuilder(args);

#region PORT (Render / Railway safe)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(int.Parse(port)));
Console.WriteLine($"🌐 Backend listening on port {port}");
#endregion

#region CONFIG
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);
#endregion

#region DATABASE CONFIG

var runtimeDb = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(runtimeDb))
{
    throw new Exception("❌ DATABASE_URL is missing");
}

Console.WriteLine("📍 Runtime DB configured");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(runtimeDb, o =>
    {
        o.EnableRetryOnFailure(5);
        o.CommandTimeout(120);
    });
});

#endregion

#region SERVICES
builder.Services.AddControllers();

builder.Services.AddCors(p =>
{
    p.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});
#endregion

var app = builder.Build();

#region MIDDLEWARE
app.UseRouting();
app.UseCors("AllowAll");
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));
#endregion

#region CONTROLLED MIGRATIONS (SAFE)

var runMigrations =
    Environment.GetEnvironmentVariable("RUN_MIGRATIONS")?.ToLower() == "true";

if (runMigrations)
{
    Console.WriteLine("🟢 RUN_MIGRATIONS=true — preparing migration context");

    var migrationDb =
        Environment.GetEnvironmentVariable("DATABASE_URL_MIGRATIONS")
        ?? runtimeDb;

    try
    {
        using var scope = app.Services.CreateScope();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(migrationDb, o =>
            {
                o.CommandTimeout(300);
                o.EnableRetryOnFailure(5);
            })
            .Options;

        using var context = new AppDbContext(options);

        Console.WriteLine("🔄 Running EF Core migrations...");
        context.Database.Migrate();
        Console.WriteLine("✅ Database migrated");

        // ---------- SAFE ADMIN SEED ----------
        if (!context.Admins.Any())
        {
            var hasher = new PasswordHasher<Admin>();

            var admin = new Admin
            {
                Name = "admin",
                PasswordHash = hasher.HashPassword(null!, "Admin123!")
            };

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
        Console.WriteLine(ex.Message);

        if (ex.InnerException != null)
        {
            Console.WriteLine("— Inner Exception —");
            Console.WriteLine(ex.InnerException.Message);
        }
    }
}
else
{
    Console.WriteLine("⏭️ RUN_MIGRATIONS=false — skipping migrations");
}

#endregion

app.Run();

