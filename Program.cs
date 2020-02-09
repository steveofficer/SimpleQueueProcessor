using System;
using System.Collections.Generic;
using BackgroundQueue.Messages;
using BackgroundQueue.MessageHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace BackgroundQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            using (var ctx = host.Services.GetRequiredService<DatabaseContext>()) {
                ctx.Database.Migrate();

                // Add some messages
                ctx.MessageQueue.Add(new Message { 
                    MessageType = ConsoleMessage.MessageType,
                    Data = JObject.FromObject(new ConsoleMessage {
                        Message = "Hello World",
                        Color = ConsoleColor.Yellow
                    }),
                    NextRunTime = System.DateTime.UtcNow.AddSeconds(1)
                });

                ctx.MessageQueue.Add(new Message { 
                    MessageType = ConsoleMessage.MessageType,
                    Data = JObject.FromObject(new ConsoleMessage {
                        Message = "Hello World again",
                        Color = ConsoleColor.Magenta
                    }),
                    NextRunTime = System.DateTime.UtcNow.AddSeconds(1)
                });

                ctx.MessageQueue.Add(new Message { 
                    MessageType = "WebHookMessage",
                    Data = JObject.FromObject(new WebHookMessage {
                        Endpoint = new Uri("http://localhost:8080/"),
                        Payload = JObject.FromObject(new { field1 = "something "})
                    }),
                    NextRunTime = System.DateTime.UtcNow.AddSeconds(1)
                });

                ctx.MessageQueue.Add(new Message { 
                    MessageType = ConsoleMessage.MessageType,
                    Data = JObject.FromObject(new ConsoleMessage {
                        Message = "Blah blah blah",
                        Color = ConsoleColor.Cyan
                    }),
                    NextRunTime = System.DateTime.UtcNow.AddSeconds(1)
                });

                ctx.MessageQueue.Add(new Message { 
                    MessageType = "WebHookMessage",
                    Data = JObject.FromObject(new WebHookMessage {
                        Endpoint = new Uri("http://localhost:8080/"),
                        Payload = JObject.FromObject(new { message = "Hello this is a message"})
                    }),
                    NextRunTime = System.DateTime.UtcNow.AddSeconds(1)
                });

                ctx.SaveChanges();
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            var handlerMappings = new Dictionary<string, Type> {
                [ConsoleMessage.MessageType] = typeof(ConsoleMessageHandler),
                [WebHookMessage.MessageType] = typeof(WebHookMessageHandler)
            };
            
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, configuration) => {
                    configuration.AddJsonFile("appsettings.json", false);
                })
                .ConfigureServices((hostContext, services) => {
                    services
                        .AddDbContext<DatabaseContext>(options => {
                            options.UseSqlServer(hostContext.Configuration.GetConnectionString("PrimaryDatabase"));
                        })
                        .AddSingleton<IMessageHandlerFactory>(new MessageHandlerFactory(handlerMappings))
                        .AddScoped<IQueueSource, DbQueueSource>()
                        .AddHostedService<QueueProcessingService>()
                        .AddScoped<ConsoleMessageHandler>()
                        .AddHttpClient<WebHookMessageHandler>();
                    ;
                });
        }
    }
}
