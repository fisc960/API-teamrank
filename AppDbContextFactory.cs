using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace GemachApp.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<AppDbContext>();

            // Only PostgreSQL is supported
            var provider =
                Environment.GetEnvironmentVariable("DB_PROVIDER")
                ?? config["DB_PROVIDER"]
                ?? "postgres";

            if (!provider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Only PostgreSQL is supported");

            // 🔐 IMPORTANT:
            // EF CLI MUST use direct DB (DATABASE_URL_MIGRATIONS)
            // Runtime may use pooler (DATABASE_URL)
            var databaseUrl =
                Environment.GetEnvironmentVariable("DATABASE_URL_MIGRATIONS")
                ?? Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrWhiteSpace(databaseUrl))
                throw new Exception("DATABASE_URL_MIGRATIONS or DATABASE_URL is required");

            // Convert postgres:// URL → ADO.NET connection string if needed
            if (databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                databaseUrl = ConvertPostgresUrl(databaseUrl);
            }

            options.UseNpgsql(databaseUrl, o =>
            {
                o.CommandTimeout(300); // long timeout for migrations
                o.EnableRetryOnFailure(5);
            });

            return new AppDbContext(options.Options);
        }

        private static string ConvertPostgresUrl(string url)
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':', 2);

            return
                $"Host={uri.Host};" +
                $"Port={uri.Port};" +
                $"Database={uri.AbsolutePath.TrimStart('/')};" +
                $"Username={userInfo[0]};" +
                $"Password={userInfo[1]};" +
                $"SslMode=Require;" +
                $"Trust Server Certificate=true;";
        }
    }
}