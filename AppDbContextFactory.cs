using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GemachApp.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var dbProvider = configuration["DB_PROVIDER"] ?? "sqlserver";

        if (dbProvider == "sqlserver")
        {
            // LOCAL — SQL SERVER
            var cs = configuration.GetConnectionString("SqlServerConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new Exception("❌ SqlServerConnection missing in appsettings.json");

            optionsBuilder.UseSqlServer(cs, sql =>
            {
                sql.EnableRetryOnFailure(5);
                sql.CommandTimeout(120);
            });
        }
        else if (dbProvider == "postgres")
        {
            // PRODUCTION — SUPABASE
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (string.IsNullOrWhiteSpace(databaseUrl))
                throw new Exception("❌ DATABASE_URL missing");

            optionsBuilder.UseNpgsql(ConvertDatabaseUrl(databaseUrl), npgsql =>
            {
                npgsql.EnableRetryOnFailure(5);
                npgsql.CommandTimeout(120);
            });
        }

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ConvertDatabaseUrl(string databaseUrl)
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
}