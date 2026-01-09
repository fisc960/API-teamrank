
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
                .AddJsonFile("appsettings.Local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var provider =
                Environment.GetEnvironmentVariable("DB_PROVIDER")
                ?? config["DB_PROVIDER"]
                ?? "postgres";

            var options = new DbContextOptionsBuilder<AppDbContext>();

            if (provider.ToLower() == "postgres")
            {
                var dbUrl =
                    Environment.GetEnvironmentVariable("DATABASE_URL")
                    ?? throw new Exception("DATABASE_URL missing for Postgres");

                options.UseNpgsql(ConvertPostgresUrl(dbUrl));
            }
            else
            {
                var cs = config.GetConnectionString("DefaultConnection")
                    ?? throw new Exception("DefaultConnection missing");

                options.UseSqlServer(cs);
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