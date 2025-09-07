
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
using GemachApp.Services;
using Microsoft.Extensions.Logging;

namespace WebApplication4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Determine environment
            var env = builder.Environment.EnvironmentName;
            Console.WriteLine($"Environment: {env}");
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Assembly Location: {System.Reflection.Assembly.GetExecutingAssembly().Location}");

            // Database configuration
            var railwayUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "sqlserver";

            Console.WriteLine($"DATABASE_URL present: {!string.IsNullOrEmpty(railwayUrl)}");
            Console.WriteLine($"DB_PROVIDER: {dbProvider}");

            if (!string.IsNullOrEmpty(railwayUrl) && builder.Environment.IsProduction())
            {
                try
                {
                    // Production: Railway PostgreSQL
                    var npgsqlConn = ConvertRailwayUrlToNpgsql(railwayUrl);
                    Console.WriteLine($"Connection string (masked): {MaskConnectionString(npgsqlConn)}");

                    builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(npgsqlConn)
                               .EnableSensitiveDataLogging()
                               .LogTo(Console.WriteLine, LogLevel.Information)
                    );
                    Console.WriteLine("Using Railway PostgreSQL database");

                    // Use PORT env variable for binding in production
                    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
                    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
                    Console.WriteLine($"Binding to port {port} for production");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database configuration error: {ex.Message}");
                    throw;
                }
            }
            else
            {
                // Local development: switch provider based on DB_PROVIDER
                var localConn = builder.Configuration.GetConnectionString("ApplicationDbcontext");
                var pgConn = builder.Configuration.GetConnectionString("PostgresLocal")
                             ?? "Host=localhost;Port=5432;Database=TestDb;Username=postgres;Password=yourpassword";

                if (dbProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(pgConn, b => b.MigrationsAssembly("GemachApp.PostgresMigrations"))
                               .EnableSensitiveDataLogging()
                               .LogTo(Console.WriteLine, LogLevel.Information)
                    );
                    Console.WriteLine("Using local PostgreSQL database");
                }
                else
                {
                    builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(localConn, b => b.MigrationsAssembly("GemachApp"))
                               .EnableSensitiveDataLogging()
                               .LogTo(Console.WriteLine, LogLevel.Information)
                    );
                    Console.WriteLine("Using local SQL Server database");
                }
            }

            // Services
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddTransient<IEmailService, EmailService>();

            // Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add logging
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // CORS with more comprehensive configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.WithOrigins("http://localhost:5173", "https://localhost:5174", "https://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                    else
                    {
                        // Production origins - add both your known URLs and be more flexible
                        policy.WithOrigins(
                            "https://team-rank-banking.vercel.app",
                            "https://team-rank-banking-mltsy6680-mr-fischs-projects.vercel.app"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // Cache preflight for 24 hours
                    }
                });

                // Temporary debug policy (remove after fixing)
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            Console.WriteLine("Building application complete, configuring middleware...");

            // Global exception handling
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    var contextFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        logger.LogError(contextFeature.Error, "Unhandled exception occurred");
                        await context.Response.WriteAsync($"Error: {contextFeature.Error.Message}");
                    }
                });
            });

            // Request logging middleware
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
                Console.WriteLine($"Origin: {context.Request.Headers.Origin}");
                Console.WriteLine($"User-Agent: {context.Request.Headers.UserAgent}");

                try
                {
                    await next();
                    Console.WriteLine($"Response: {context.Response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Middleware exception: {ex.Message}");
                    throw;
                }
            });

            // Development-specific middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                });
            }

            // Don't redirect to HTTPS in production (Railway handles this)
            if (!app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            // Use CORS (you can temporarily switch to "AllowAll" for testing)
            //app.UseCors("FrontendPolicy");
            app.UseCors("AllowAll"); // <-- temporarily use the debug policy


            app.UseAuthorization();

            // Add health check endpoints
            app.MapGet("/", () => new {
                message = "API is running",
                timestamp = DateTime.UtcNow,
                environment = app.Environment.EnvironmentName
            });

            app.MapGet("/health", () => new {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });

            app.MapGet("/cors-test", (HttpContext context) => new {
                origin = context.Request.Headers.Origin.ToString(),
                method = context.Request.Method,
                headers = context.Request.Headers.Keys.ToList(),
                timestamp = DateTime.UtcNow
            });

            app.MapControllers();

            Console.WriteLine("Middleware configuration complete. Starting application...");
            Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
            Console.WriteLine("Application should be accessible now.");

            app.Run();
        }

        private static string ConvertRailwayUrlToNpgsql(string databaseUrl)
        {
            try
            {
                Console.WriteLine($"Converting DATABASE_URL to Npgsql format...");

                if (databaseUrl.Contains("serviceHost=") || databaseUrl.Contains("Database="))
                {
                    Console.WriteLine("DATABASE_URL already in connection string format");
                    return databaseUrl;
                }

                if (databaseUrl.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
                {
                    var uri = new Uri(databaseUrl);
                    var userInfo = uri.UserInfo.Split(':');
                    var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
                    Console.WriteLine("Successfully converted DATABASE_URL");
                    return connectionString;
                }

                throw new ArgumentException($"Unrecognized DATABASE_URL format: {databaseUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting DATABASE_URL: {ex.Message}");
                return databaseUrl;
            }
        }

        private static string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return connectionString;

            // Mask password in connection string for logging
            return System.Text.RegularExpressions.Regex.Replace(
                connectionString,
                @"Password=([^;]+)",
                "Password=***",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }
    }
}


/*

/*using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
using GemachApp.Services;
using Microsoft.Extensions.Logging;

namespace WebApplication4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Railway port configuration
            if (!builder.Environment.IsDevelopment())
            {
                var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}"); //$"http://localhost:{port}")
                Console.WriteLine($"Configuring to listen on port {port}...");
            }

            // Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Database Configuration
            var railwayUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(railwayUrl))
            {
                // Railway PostgreSQL for production
                var npgsqlConn = ConvertRailwayUrlToNpgsql(railwayUrl);
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(npgsqlConn)
                           .EnableSensitiveDataLogging()
                           .LogTo(Console.WriteLine, LogLevel.Information)
                );
                Console.WriteLine("Using Railway PostgreSQL database");
            }
            else
            {
                // Local development: SQL Server
                var localConn = builder.Configuration.GetConnectionString("ApplicationDbcontext");
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(localConn)
                           .EnableSensitiveDataLogging()
                           .LogTo(Console.WriteLine, LogLevel.Information)
                );
                Console.WriteLine("Using local SQL Server database");
            }

            // Services
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddTransient<IEmailService, EmailService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        policy.WithOrigins(
                            "http://localhost:5173",
                            "http://localhost:5174",
                            "https://localhost:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    }
                    else
                    {
                         policy.WithOrigins(
                         /*"https://team-rank-banking-lnlxttazu-mr-fischs-projects.vercel.app"*/
//"https://team-rank-banking.vercel.app", //  actual Vercel frontend URL

//  "http://localhost:5173"                  //  Local dev (adjust if needed)

//      )
/* policy.SetIsOriginAllowed(origin =>
 {
     // Allow all Vercel deployments
     return origin.Contains("vercel.app") &&
            (origin.StartsWith("https://team-rank-banking") ||
             origin.Contains("mr-fischs-projects"));
 })*//*
.AllowAnyHeader()
 .AllowAnyMethod()
 .AllowCredentials();
}
});
});

var app = builder.Build();
// global exception handler
app.UseExceptionHandler(appBuilder =>
{
appBuilder.Run(async context =>
{
context.Response.StatusCode = 500;
await context.Response.WriteAsync("An error occurred");
});
});

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
app.UseDeveloperExceptionPage();
/* app.UseSwagger();
app.UseSwaggerUI(c =>
{
c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});*//*
}
else
{
app.UseExceptionHandler("/Error");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});

app.UseHttpsRedirection();
app.UseRouting();

// Apply CORS
app.UseCors("FrontendPolicy");

app.UseAuthorization();
app.MapControllers();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"Raw DATABASE_URL: {databaseUrl}");

if (string.IsNullOrEmpty(databaseUrl))
{
Console.WriteLine("DATABASE_URL is null or empty!");
}
else
{
Console.WriteLine($"DATABASE_URL length: {databaseUrl.Length}");
Console.WriteLine($"Starts with postgresql://: {databaseUrl.StartsWith("postgresql://")}");
Console.WriteLine($"Contains serviceHost=: {databaseUrl.Contains("serviceHost=")}");
}

// Database migrations: only apply PostgreSQL migrations in production
/* temporarily comment -->  using (var scope = app.Services.CreateScope())
{
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Testing database connection...");
    if (db.Database.CanConnect())
    {
        logger.LogInformation("Database connection successful!");

        // Apply migrations **only for PostgreSQL / production**
        if (!string.IsNullOrEmpty(railwayUrl))
        {
            logger.LogInformation("Applying PostgreSQL database migrations...");
            db.Database.Migrate();
            logger.LogInformation("PostgreSQL migrations completed successfully!");
        }
    }
    else
    {
        logger.LogError("Cannot connect to database!");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Database connection failed: {Message}", ex.Message);
}
}*/

// Railway port configuration
/*var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
  app.Urls.Add($"http://0.0.0.0:{port}");

Console.WriteLine($"Starting application on port {port}...");
app.Run();*/
// }

// Helper to convert Railway DATABASE_URL to Npgsql connection string
/* private static string ConvertRailwayUrlToNpgsql(string databaseUrl)
 {
     var uri = new Uri(databaseUrl);
     var userInfo = uri.UserInfo.Split(':');
     return $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};Pooling=true;SSL Mode=Require;Trust Server Certificate=True;";
 }*//*
private static string ConvertRailwayUrlToNpgsql(string databaseUrl)
{
    try
    {
        // Check if it's already in connection string format (Railway's new format)
        if (databaseUrl.Contains("serviceHost=") || databaseUrl.Contains("Database="))
        {
            Console.WriteLine("DATABASE_URL is already in connection string format");
            return databaseUrl; // Return as-is since it's already correct format
        }

        // Handle URI format (postgresql://user:password@host:port/database)
        if (databaseUrl.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
        {
            var uri = new Uri(databaseUrl);
            var host = uri.Host;
            var port = uri.Port;
            var database = uri.AbsolutePath.TrimStart('/');
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo[0];
            var password = userInfo.Length > 1 ? userInfo[1] : "";

            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }

        // If neither format is recognized, throw an exception
        throw new ArgumentException($"Unrecognized database URL format: {databaseUrl}");
    }
    catch (UriFormatException ex)
    {
        Console.WriteLine($"URI Format Error: {ex.Message}");
        Console.WriteLine($"DATABASE_URL value: {databaseUrl}");

        // Try to return the original string if it might already be a connection string
        if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.Contains("="))
        {
            Console.WriteLine("Attempting to use DATABASE_URL as connection string directly");
            return databaseUrl;
        }

        throw new InvalidOperationException($"Could not parse DATABASE_URL: {databaseUrl}", ex);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error parsing DATABASE_URL: {ex.Message}");
        throw;
    }
}
}
}
*/

/*
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
using GemachApp.Services;
using Microsoft.Extensions.Options;

namespace WebApplication4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Enhanced logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            /*
            // Get connection string from environment variables first (for Railway)
            var railwayConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            // Fallback to local connection from appsettings if env variable not set
            var connectionString = !string.IsNullOrEmpty(railwayConnectionString)
                ? railwayConnectionString
                : builder.Configuration.GetConnectionString("DefaultConnection");

            Console.WriteLine($"Using connection string: {connectionString?.Substring(0, Math.Min(50, connectionString.Length))}...");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {  
            options.UseNpgsql(connectionString);*//*

var connectionString = builder.Configuration.GetConnectionString("ApplicationDbcontext");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString); // <-- SQL Server
                if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine, LogLevel.Information);
            }
        });
            // Add services to the container.
           /* builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbcontext"))
             .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information)
           );*//*
            //builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Services.AddScoped<IAccountService, AccountService>();
              builder.Services.AddTransient<IEmailService, EmailService>();
            
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyCorsPolicy", corsBuilder =>
                { 
                    if (builder.Environment.IsDevelopment())
                    {
                        // builder.WithOrigins("http://localhost:5174") // Your React app's origin
                        corsBuilder.WithOrigins(
                        "http://localhost:5173",    // Vite dev server
                         "http://localhost:5174",   // 
                        "http://localhost:5175",   // 
                       "http://localhost:5177",
                        "http://localhost:3000",    // Create React App dev server
                        "https://localhost:5173",   // HTTPS versions
                          "https://localhost:5174",   // HTTPS versions
                        "https://localhost:5175",   // HTTPS versions
                        "https://localhost:5000"
                    )
                .AllowAnyMethod() // Or specify allowed methods: .WithMethods("POST", "GET", "PUT", "DELETE")
                           .AllowAnyHeader() // Or specify allowed headers: .WithHeaders("Content-Type", "Authorization")
                           .AllowCredentials(); // Important if you're using cookies or authorization headers
                                                //.SetIsOriginAllowed(origin => true); // Allow any origin in development
                    }
                    else
                    {
                        // Production - allow your deployed frontend domains
                        corsBuilder.WithOrigins(
                            "https://your-frontend-domain.vercel.app", // Replace with your actual frontend domain
                            "https://your-frontend-domain.netlify.app", // Add other domains as needed
                            "https://api-teamrank-production.up.railway.app" // If frontend is on same domain
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    }

                });
            });


            // 1. Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVercelFrontend",
                    policy =>
                    {
                        policy.WithOrigins("https://team-rank-banking-lnlxttazu-mr-fischs-projects.vercel.app")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // 2. Apply CORS
            var app = builder.Build();
            app.UseCors("AllowVercelFrontend");

            // Clear providers after CORS configuration
            /*  #2  builder.Logging.ClearProviders();
            builder.Services.AddTransient<IEmailService, EmailService>();*//*


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                });
            }
else
{
    // Add global exception handler for production
    app.UseExceptionHandler("/Error");
}



// Use CORS before routing
app.UseCors("MyCorsPolicy");

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllers();

//Test database connection and Apply migrations automatically
using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Testing database connection...");

        // Test connection
        var canConnect = db.Database.CanConnect();
        if (canConnect)
        {
            logger.LogInformation("Database connection successful!");

            // Apply migrations
            logger.LogInformation("Applying database migrations...");
            db.Database.Migrate();
         logger.LogInformation("Database migrations completed successfully!");
        }
        else
        {
            logger.LogError("Cannot connect to database!");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database connection failed: {Message}", ex.Message);
        // Don't throw - let app start but log the error
    }
}

Console.WriteLine("Starting application...");

            app.Run();
        }
    }
}
*/
