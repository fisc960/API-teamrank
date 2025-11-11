
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


/*        #2 


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GemachApp.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Check DB_PROVIDER env variable
            var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "sqlserver";
            Console.WriteLine($"DB_PROVIDER = {dbProvider}");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            if (dbProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
            {
                // Postgres connection string from env variable
                var pgConn = Environment.GetEnvironmentVariable("DATABASE_URL");
                Console.WriteLine($"Postgres connection string: {pgConn}");
                optionsBuilder.UseNpgsql(pgConn, b => b.MigrationsAssembly("GemachApp.PostgresMigrations"));
            }
            else
            {
                // Local SQL Server
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Local.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                var localConn = config.GetConnectionString("ApplicationDbcontext");
                optionsBuilder.UseSqlServer(localConn, b => b.MigrationsAssembly("GemachApp"));
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}



/*
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using GemachApp.Data;

namespace WebApplication4
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Local.json", optional: true) // local SQL Server
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("ApplicationDbcontext");

            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
*/