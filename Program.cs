using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ReadVideo.Server.Data;
using ReadVideo.Server.Middleware;
using ReadVideo.Server.Models;
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


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "Jwt:Issuer",
                    ValidAudience = "Jwt:Audience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("aTGeUGu2fBQstsUkLFryni51LpCxl0Mqg7pLGTPvt6c="))
                };
            });
            //var mongoConnectionString = builder.Configuration.GetConnectionString("MongoConnection");
            ////var mongoDatabaseName = builder.Configuration["MongoSettings:DatabaseName"];

            //builder.Services.AddSingleton(new MongoDbContext(mongoConnectionString, mongoDatabaseName));


            var app = builder.Build();
            
            app.UseCors("AllowAnyOrigin"); // Use the named CORS policy here

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<UserProcessingMiddleware>();
            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
