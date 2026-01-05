
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GemachApp.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "sqlserver";
            Console.WriteLine($"DB_PROVIDER = {dbProvider}");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            if (dbProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
            {
                var pgConn = Environment.GetEnvironmentVariable("DATABASE_URL");
                if (string.IsNullOrEmpty(pgConn))
                    throw new Exception("DATABASE_URL environment variable is not set for Postgres.");

                Console.WriteLine("Using PostgreSQL...");

                optionsBuilder
                    .UseNpgsql(pgConn)  
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine);
            }
            else
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Local.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                var localConn = config.GetConnectionString("ApplicationDbcontext");
                Console.WriteLine($"Using SQL Server: {localConn}");

                optionsBuilder
                    .UseSqlServer(localConn)  
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine);
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}


