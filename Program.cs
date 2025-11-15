using AngleSharp;
using HigLabo.OpenAI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using ReadVideo.Server.Data;
using ReadVideo.Server.Middleware;
using ReadVideo.Server.Models;
using ReadVideo.Server.Services;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.BotStateManagement;
using ReadVideo.Server.Services.BotStateManagement.Factory;
using ReadVideo.Server.Services.BotStateManagement.Factory.Datacol;
using ReadVideo.Server.Services.EmailSending;
using ReadVideo.Server.Services.Embeddings;
using ReadVideo.Server.Services.Embeddings.Generation;
using ReadVideo.Server.Services.Embeddings.Storage;
using ReadVideo.Server.Services.Support;
using ReadVideo.Server.Utils;
using ReadVideo.Services.YoutubeManagement;
using IEmailSender = ReadVideo.Server.Services.EmailSending.IEmailSender;

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

            builder.Services.AddSingleton<TokenToServiceKeyConverter>();

            // Register the generic factory for ISomeClass with the specific factory method
            builder.Services.AddSingleton<DITypeFactoryBase<string, IJivoSiteService>>(
                serviceProvider => new DITypeFactoryBase<string, IJivoSiteService>(
                    serviceProvider,
                    (sp, key) => new JivoSiteService(sp.GetRequiredService<IHttpClientFactory>(), "datacol")//key)
                ));

            builder.Services.AddSingleton<IChatDataStorageService, ChatDataStorageService>();
            builder.Services.AddKeyedSingleton<BotStateManagerFactoryBase, DatacolBotStateManagerFactory>("datacol");
            builder.Services.AddKeyedSingleton<BotStateManagerFactoryBase, StartSpeakingBotStateManagerFactory>("startspeaking");

        //    builder.Services.AddDbContext<DCStatsDbContext>(options =>
        //options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRE_CONNECTION")));
           
            builder.Services.AddSingleton<DITypeFactoryBase<string, BotStateManager>>(
                serviceProvider => new DITypeFactoryBase<string, BotStateManager>(
                    serviceProvider,
                    (sp, key) => sp.GetKeyedService<BotStateManagerFactoryBase>(key).Create()//sp.GetRequiredService<BotStateManagerFactory>().Create(key)
                )); 
            

            builder.Services.AddSingleton<DITypeFactoryBase<string, IEmailSender>>(
                serviceProvider => new DITypeFactoryBase<string, IEmailSender>(
                    serviceProvider,
                    (sp, key) => {
                        string settingKey = sp.GetRequiredService<TokenToServiceKeyConverter>().Convert(key);
                        
                        string jsonSettings = Environment.GetEnvironmentVariable($"{settingKey}_SMTP_SETTING");
                        // Console.WriteLine($":key {key} , email sender setting key {settingKey} and jsonsettings {jsonSettings}");
                        if (!string.IsNullOrEmpty(jsonSettings))
                        {
                            // Deserialize the JSON string to an SmtpSettings object
                            SmtpSettings smtpSettings = JsonConvert.DeserializeObject<SmtpSettings>(jsonSettings);

                            // Use smtpSettings as needed
                            Console.WriteLine($"Host: {smtpSettings.Host}");
                            return new EmailSender(smtpSettings);
                        }
                        else
                        {
                            Console.WriteLine("SMTP settings are not set in the environment variables.");
                            return null;
                        }
                      
                    }
                ));
            // builder.Services.AddScoped<IJivoSiteService, JivoSiteService>();

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
            builder.Services.AddSingleton<IMessageEvaluator, MessageEvaluator>();

            builder.Services.AddSingleton<IEmbeddingStorageService, SingleStoreService>(serviceProvider =>
            {
                return new SingleStoreService(Environment.GetEnvironmentVariable(Consts.SingleStoreConnectionStr));
            });
            builder.Services.AddSingleton<IEmbeddingGenerator, OpenAIEmbeddingGenerator>(serviceProvider =>
            {
                return new OpenAIEmbeddingGenerator(Environment.GetEnvironmentVariable(Consts.OpenAIApiKey));
            });

            builder.Services.AddSingleton<IAssistant, OpenAIAssistant>();
            builder.Services.AddSingleton<IEmbeddingManager, EmbeddingManager>();

            // Support Request Services
            builder.Services.AddScoped<ISupportRequestService, SupportRequestService>();
            builder.Services.AddScoped<Services.Support.IEmailService>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Services.Support.EmailService>>();
                var configuration = serviceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

                // Get SMTP settings from environment variable (same as existing email service)
                string jsonSettings = Environment.GetEnvironmentVariable("DATACOL_SMTP_SETTING");

                if (!string.IsNullOrEmpty(jsonSettings))
                {
                    SmtpSettings smtpSettings = JsonConvert.DeserializeObject<SmtpSettings>(jsonSettings);
                    return new Services.Support.EmailService(smtpSettings, logger, configuration);
                }
                else
                {
                    throw new InvalidOperationException("SMTP settings are not configured in environment variables.");
                }
            });


            //var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            //var mongoDbConnectionString = Environment.GetEnvironmentVariable(mongoDbSettings.ConnectionStringEnvVar);
            
            //builder.Services.AddSingleton<IMongoClient>(ServiceProvider =>
            //{
            //    return new MongoClient(mongoDbConnectionString);
            //});
            //builder.Services.AddSingleton(serviceProvider =>
            //{
            //    var client = serviceProvider.GetRequiredService<IMongoClient>();
            //    return client.GetDatabase(mongoDbSettings.DatabaseName);
            //});
            
            //builder.Services.AddSingleton(new MongoDbContext(Environment.GetEnvironmentVariable(mongoDbSettings.ConnectionStringEnvVar), mongoDbSettings.DatabaseName));

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
