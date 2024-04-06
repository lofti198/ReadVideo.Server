using HigLabo.OpenAI;
using MongoDB.Driver;
using ReadVideo.Server.Data;
using ReadVideo.Server.Middleware;
using ReadVideo.Server.Models;
using ReadVideo.Server.Services;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.AIAssistants.Decorators;
using ReadVideo.Server.Services.Embeddings.Generation;
using ReadVideo.Server.Services.Embeddings.Storage;
using ReadVideo.Services.YoutubeManagement;

namespace ReadVideo.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //   string openAIApiKey = 
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            // builder.Services.AddNewtonsoftJson();
            builder.Services.AddTransient<IYoutubeSubtitleService, YoutubeSubtitleService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IJivoSiteService, JivoSiteService>();

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
                return new OpenAIClient(Environment.GetEnvironmentVariable(Consts.OpenAIApiKey));
            });

            builder.Services.AddSingleton<IAssistant, OpenAIAssistant>();

            builder.Services.AddSingleton<IEmbeddingStorageService, SingleStoreService>(serviceProvider =>
            {
                return new SingleStoreService(Environment.GetEnvironmentVariable(Consts.SingleStoreConnectionStr));
            });
            
            builder.Services.AddSingleton<IEmbeddingGenerator, OpenAIEmbeddingGenerator>(serviceProvider =>
            {
                return new OpenAIEmbeddingGenerator(Environment.GetEnvironmentVariable(Consts.OpenAIApiKey));
            });



            // Register the decorator, ensuring it wraps the original IAssistant
            builder.Services.Decorate<IAssistant>((inner, serviceProvider) =>
            {
                var embeddingGenerator = serviceProvider.GetRequiredService<IEmbeddingGenerator>();
                var embeddingStorageService = serviceProvider.GetRequiredService<IEmbeddingStorageService>();
                return new EmbeddingsAssistantDecorator(inner, embeddingGenerator, embeddingStorageService);
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
