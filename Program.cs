using FormsApi.Data;
using FormsApi.Middleware;
using FormsApi.Repositories;
using FormsApi.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FormsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("FormsDb");

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    options.UseInMemoryDatabase("FormsDb");
                }
                else
                {
                    options.UseSqlServer(connectionString);
                }
            });


            builder.Services.AddScoped<IFormDataRepository, FormDataRepository>();
            builder.Services.AddScoped<IFormAuthorizationService, Implementations.StubFormAuthorizationService>();

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddAntiforgery();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddSwaggerGen();

            // Configure Serilog for logging
            // Using Serilog.AspNetCore package for logging.
            // For now logging to console, but can be configured to log to file or other sinks as needed.
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // Added Swashbuckle.AspNetCore.SwaggerGen and Swashbuckle.AspNetCore.SwaggerUI packages for Swagger support
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();

            // Ensure the DB/schema exists for the in-memory / dev fallback path.
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}
