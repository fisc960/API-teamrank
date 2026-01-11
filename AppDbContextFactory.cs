
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

            var provider =
                Environment.GetEnvironmentVariable("DB_PROVIDER")
                ?? config["DB_PROVIDER"]
                ?? "postgres";

            if (provider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                // CASE 1: Railway-style postgres:// URL
                if (!string.IsNullOrWhiteSpace(databaseUrl) &&
                    (databaseUrl.StartsWith("postgres://") ||
                     databaseUrl.StartsWith("postgresql://")))
                {
                    options.UseNpgsql(ConvertPostgresUrl(databaseUrl));
                }
                // CASE 2: Already a connection string (Aiven / Railway)
                else if (!string.IsNullOrWhiteSpace(databaseUrl))
                {
                    options.UseNpgsql(databaseUrl);
                }
                // CASE 3: Local dev
                else
                {
                    var localPg = config.GetConnectionString("PostgresConnection");
                    if (string.IsNullOrWhiteSpace(localPg))
                        throw new Exception("PostgresConnection missing");

                    options.UseNpgsql(localPg);
                }
            }
            else
            {
                throw new Exception("SQL Server is disabled for this app");
            }

            return new AppDbContext(options.Options);
        }

        private static string ConvertPostgresUrl(string url)
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
    }
}