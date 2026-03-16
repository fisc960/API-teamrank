using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GemachApp.Data;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

#region CONFIG LOADING (VERY IMPORTANT)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
#endregion

#region ENV INFO
var env = builder.Environment.EnvironmentName;
Console.WriteLine($"🧠 Environment = {env}");
#endregion

#region PORT (Render / Local / IIS safe)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenAnyIP(int.Parse(port));
});
Console.WriteLine($"🌐 Listening on port {port}");
#endregion

#region DATABASE SELECTION (SQL Server vs Supabase)

var dbProvider = builder.Configuration["DB_PROVIDER"] ?? "sqlserver";
Console.WriteLine($"🗄️ DB_PROVIDER = {dbProvider}");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider == "sqlserver")
    {
        // =========================
        // LOCAL — SQL SERVER
        Console.WriteLine("💻 Using SQL Server");

        var cs = builder.Configuration.GetConnectionString("SqlServerConnection");
        if (string.IsNullOrWhiteSpace(cs))
            throw new Exception("❌ SqlServerConnection missing");

        options.UseSqlServer(cs, sql =>
        {
            sql.EnableRetryOnFailure(5);
            sql.CommandTimeout(120);
        });
    }
    else if (dbProvider == "postgres")
    {
        // =========================
        // PRODUCTION — SUPABASE
        Console.WriteLine("📦 Using PostgreSQL (Supabase)");

        if (builder.Environment.IsDevelopment())
            throw new Exception("❌ Postgres is disabled in local development");

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(databaseUrl))
            throw new Exception("❌ DATABASE_URL missing");

        options.UseNpgsql(ConvertDatabaseUrl(databaseUrl), npgsql =>
        {
            npgsql.EnableRetryOnFailure(5);
            npgsql.CommandTimeout(120);
        });
    }
    else
    {
        throw new Exception($"❌ Invalid DB_PROVIDER: {dbProvider}");
    }
});

#endregion

#region CORS (Vercel + Local)

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
        policy
            /*.WithOrigins(
                "https://team-rank-banking.vercel.app",
                "http://localhost:5173",
                "http://localhost:3000"
            )*/
            .AllowAnyOrigin()  // Temporarily allow all origins for testing
            .AllowAnyHeader()
            .AllowAnyMethod()
           // .AllowCredentials()
    );
});

#endregion

#region SERVICES
builder.Services.AddControllers();
builder.Services.AddHttpClient();
#endregion

var app = builder.Build();

#region MIDDLEWARE
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));
#endregion


#region MIGRATIONS + SAFE ADMIN SEED (Background task)
_ = Task.Run(async () =>
{
    try
    {
        await Task.Delay(3000); // Give app time to start

        Console.WriteLine("🔄 Starting migration task...");

        using var scope = app.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("🔄 Applying migrations...");
        await ctx.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrations applied");

        Console.WriteLine("🔍 Checking for existing admin...");
        var adminExists = await ctx.Admins.AnyAsync();

        if (!adminExists)
        {
            Console.WriteLine("🌱 Seeding admin...");
            var hasher = new PasswordHasher<Admin>();
            ctx.Admins.Add(new Admin
            {
                Name = "admin",
                PasswordHash = hasher.HashPassword(new Admin(), "Admin123!")
            });
            await ctx.SaveChangesAsync();
            Console.WriteLine("✅ Default admin created");
        }

        Console.WriteLine("🎉 Migration task completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ MIGRATION ERROR: {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
    }
});
#endregion

app.Run();


// =======================
//  SUPABASE DATABASE_URL PARSER
static string ConvertDatabaseUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);

    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    return
        $"Host={uri.Host};" +
        $"Port={uri.Port};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={username};" +
        $"Password={password};" +
        $"Ssl Mode=Require;" +
        $"Trust Server Certificate=true;" +
        $"Pooling=true;";
}
