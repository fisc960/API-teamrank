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
            options.UseNpgsql(connectionString);*/

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
           );*/
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

            // Clear providers after CORS configuration
            builder.Logging.ClearProviders();
            builder.Services.AddTransient<IEmailService, EmailService>();


            var app = builder.Build();

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

