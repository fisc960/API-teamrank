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
            npgsql.CommandTimeout(30);
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

#region MIDDLEWARE — ORDER MATTERS

// Global exception handler MUST come first in the pipeline.
//    When a controller throws an unhandled exception, ASP.NET resets the
//    response — stripping any CORS headers already written — which makes the
//    browser report a CORS error instead of the real 500.
//    Catching here before UseCors ensures we write a clean JSON error response
//    and UseCors can still attach its headers on the way out.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message = "An unexpected server error occurred.",
            status = 500
        });
    });
});

app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));

#endregion

//  Admin seeding runs in background — never blocks port from opening
_ = Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(3));
    Console.WriteLine("⚠️ Starting admin reset (background)...");

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
    try
    {
        using var scope = app.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = new PasswordHasher<Admin>();

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
        Console.WriteLine("⏱️ Admin reset timed out after 25s — app continues running.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Admin reset failed: {ex.Message}");
    }
});

app.Run();

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