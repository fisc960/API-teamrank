using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
using GemachApp.Services;

namespace WebApplication4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbcontext"))
             .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information)
           );
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyCorsPolicy", builder =>
                {
                    // builder.WithOrigins("http://localhost:5173") // Your React app's origin
                    builder.WithOrigins(
                        "http://localhost:5173",    // Vite dev server
                        "http://localhost:3000",    // Create React App dev server
                        "https://localhost:5173",   // HTTPS versions
                        "https://localhost:3000"
                    )
                .AllowAnyMethod() // Or specify allowed methods: .WithMethods("POST", "GET", "PUT", "DELETE")
                           .AllowAnyHeader() // Or specify allowed headers: .WithHeaders("Content-Type", "Authorization")
                           .AllowCredentials() // Important if you're using cookies or authorization headers
                 .SetIsOriginAllowed(origin => true); // Allow any origin in development
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

            
            

            // Use CORS before routing
            app.UseCors("MyCorsPolicy");

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

