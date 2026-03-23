using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GemachApp.Data;

var builder = WebApplication.CreateBuilder(args);

#region CONFIG LOADING
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
#endregion

#region ENV INFO
var env = builder.Environment.EnvironmentName;
Console.WriteLine($"🧠 Environment = {env}");
#endregion

#region PORT
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    serverOptions.ListenAnyIP(int.Parse(port));
    Console.WriteLine($"🌐 Listening on port {port}");
});
#endregion

#region DATABASE SELECTION
var dbProvider = builder.Configuration["DB_PROVIDER"] ?? "sqlserver";
Console.WriteLine($"🗄️ DB_PROVIDER = {dbProvider}");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider == "sqlserver")
    {
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
        Console.WriteLine("📦 Using PostgreSQL (Supabase)");

        if (builder.Environment.IsDevelopment())
            throw new Exception("❌ Postgres is disabled in local development");

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(databaseUrl))
            throw new Exception("❌ DATABASE_URL missing");

        options.UseNpgsql(ConvertDatabaseUrl(databaseUrl), npgsql =>
        {
            npgsql.EnableRetryOnFailure(5);
            npgsql.CommandTimeout(30); // ✅ reduced from 120 — seeding should be fast
        });
    }
    else
    {
        throw new Exception($"❌ Invalid DB_PROVIDER: {dbProvider}");
    }
});
#endregion

#region CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
        policy
            .WithOrigins(
                "https://team-rank-banking.vercel.app",
                "https://team-rank-banking-git-main-mr-fischs-projects.vercel.app",
                "http://localhost:5173",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
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

// ✅ FIX: Run admin seeding in a background task AFTER app starts listening.
//    The previous code ran BEFORE app.Run(), blocking the port from opening.
//    Render requires the port to open within ~60s — a hanging DB call kills the deploy.
_ = Task.Run(async () =>
{
    // Give the server a moment to fully start and open its port first
    await Task.Delay(TimeSpan.FromSeconds(3));

    Console.WriteLine("⚠️ Starting admin reset (background)...");

    // Use a cancellation token so a hung DB call doesn't hang forever
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));

    try
    {
        using var scope = app.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = new PasswordHasher<Admin>();

        // ✅ Use async EF methods + cancellation token throughout
        var existingAdmins = await ctx.Admins.ToListAsync(cts.Token);

        if (existingAdmins.Any())
        {
            ctx.Admins.RemoveRange(existingAdmins);
            await ctx.SaveChangesAsync(cts.Token);
        }

        ctx.Admins.Add(new Admin
        {
            Name = "admin",
            PasswordHash = hasher.HashPassword(new Admin(), "Admin123!")
        });

        await ctx.SaveChangesAsync(cts.Token);
        Console.WriteLine("✅ Admin reset complete: admin / Admin123!");
    }
    catch (OperationCanceledException)
    {
        // ✅ Timeout hit — log it but the app keeps running normally
        Console.WriteLine("⏱️ Admin reset timed out after 25s — app continues running.");
    }
    catch (Exception ex)
    {
        // ✅ Any other DB error — log it but never crash the running app
        Console.WriteLine($"❌ Admin reset failed: {ex.Message}");
    }
});

app.Run(); // ✅ This now runs immediately — port opens, Render is happy

// =======================
// SUPABASE DATABASE_URL PARSER
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
