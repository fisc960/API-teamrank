using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GemachApp.Data;


    var builder = WebApplication.CreateBuilder(args);

    // --------------------
    // PORT (LOCAL + RAILWAY)
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5234";
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(int.Parse(port));
    });
    Console.WriteLine($"Backend listening on port {port}");

    // Load local config if exists
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

// --------------------
// DB PROVIDER - Environment variable takes priority
var provider = Environment.GetEnvironmentVariable("DB_PROVIDER")?.ToLower()
    ?? builder.Configuration["DB_PROVIDER"]?.ToLower()
    ?? "sqlserver";
//var provider = builder.Configuration["DB_PROVIDER"]?.ToLower() ?? "sqlserver";
Console.WriteLine($"DB_PROVIDER: {provider}");

    string connectionString;

    // --------------------
    // CONNECTION STRING
    if (provider == "postgres")
    {
        var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrEmpty(dbUrl))
            throw new Exception("DATABASE_URL is required");

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
    // REGISTER DB
    if (provider == "postgres")
    {
        builder.Services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(connectionString));
    }
    else
    {
        builder.Services.AddDbContext<AppDbContext>(o =>
            o.UseSqlServer(connectionString));
    }

    // --------------------
    // SERVICES
    builder.Services.AddControllers();

    // Register Email Service
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
                  ).AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // --------------------
    // BUILD APP
    var app = builder.Build();

// --------------------
// --------------------
// MIGRATION + SAFE SEED
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    //  ALWAYS apply migrations (production-safe)
    //context.Database.Migrate();

    try
    {
        Console.WriteLine("Attempting database migration...");
        context.Database.SetCommandTimeout(60); // Increase timeout to 60 seconds
        context.Database.Migrate();
        Console.WriteLine("✅ Database migration successful");

        //  Seed ONLY in Development
        if (app.Environment.IsDevelopment())
    {
        // Make sure table exists before querying
        if (context.Database.GetPendingMigrations().Any() == false)
        {
            if (!context.Admins.Any())
            {
                var hasher = new PasswordHasher<Admin>();

                var admin = new Admin
                {
                    Name = "Admin",
         
                };

                admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

                context.Admins.Add(admin);
                context.SaveChanges();

                Console.WriteLine("✅ Admin seeded (Development)");
            }
        }
    }
}
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration failed: {ex.Message}");
        Console.WriteLine("⚠️  App will start without migrations - please run migrations manually");
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

    return $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};" +
        $"SslMode=Require;Trust Server Certificate=true;Timeout=30;Command Timeout=30;Pooling=true;Max Auto Prepare=0";
    return $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};" +
        $"SslMode=Require;Trust Server Certificate=true";
    }






