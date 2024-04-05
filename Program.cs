using HigLabo.OpenAI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ReadVideo.Server.Data;
using ReadVideo.Server.Middleware;
using ReadVideo.Server.Models;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Services.YoutubeManagement;
using System.Text;

namespace ReadVideo.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            // builder.Services.AddNewtonsoftJson();
            builder.Services.AddTransient<IYoutubeSubtitleService, YoutubeSubtitleService>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add services to the container.
            builder.Services.AddSingleton<OpenAIClient>(serviceProvider =>
            {
                string apiKey = Environment.GetEnvironmentVariable(Consts.OpenAIApiKey); // Ensure you have this setting in your appsettings.json or other configuration sources
                return new OpenAIClient(apiKey);
            });

            builder.Services.AddSingleton<IAssistantServiceBase, OpenAIAssistantService>();

            var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var mongoDbConnectionString = Environment.GetEnvironmentVariable(mongoDbSettings.ConnectionStringEnvVar);
            
            builder.Services.AddSingleton<IMongoClient>(ServiceProvider =>
            {
                return new MongoClient(mongoDbConnectionString);
            });
            builder.Services.AddSingleton(serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDbSettings.DatabaseName);
            });
            // builder.Services.AddScoped<MongoDbContext>();
            builder.Services.AddSingleton(new MongoDbContext(Environment.GetEnvironmentVariable(mongoDbSettings.ConnectionStringEnvVar), mongoDbSettings.DatabaseName));

            builder.Services.AddMemoryCache();
            //var mongoConnectionString = builder.Configuration.GetConnectionString("MongoConnection");
            ////var mongoDatabaseName = builder.Configuration["MongoSettings:DatabaseName"];

            //builder.Services.AddSingleton(new MongoDbContext(mongoConnectionString, mongoDatabaseName));


            var app = builder.Build();
            
            // Configure the HTTP request pipeline.
            app.UseRouting(); // Ensure UseRouting is called before UseCors

            app.UseCors("AllowAnyOrigin"); // Use the named CORS policy here

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseAuthorization();

            // app.UseMiddleware<UserProcessingMiddleware>();
            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
