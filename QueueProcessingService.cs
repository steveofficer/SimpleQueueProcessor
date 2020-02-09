using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundQueue
{
    public class QueueProcessingService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IMessageHandlerFactory _handlerFactory;
        private readonly ILogger _logger;

        public QueueProcessingService(ILogger<QueueProcessingService> logger, IServiceProvider services, IMessageHandlerFactory handlerFactory) {
            _logger = logger;
            _services = services;
            _handlerFactory = handlerFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pollDelay = TimeSpan.FromSeconds(1);
            var inactivityCounter = 0;

            while (!stoppingToken.IsCancellationRequested) {
                var referenceTime = DateTime.UtcNow;
                var messageCounter = 0;

                using (var serviceScope = _services.CreateScope()) {
                    var queueSource = serviceScope.ServiceProvider.GetRequiredService<IQueueSource>();

                    using (var batch = (await queueSource.GetMessagesAsync(referenceTime))) {
                        foreach (var message in batch.Messages) {
                            messageCounter += 1;

                            try {
                                // Lookup the handler based on the type of the message
                                var handler = _handlerFactory.GetHandlerForMessage(message.MessageType, serviceScope.ServiceProvider);

                                if (handler == null) {
                                    _logger.LogWarning("Failed to find a handler for {MessageType}", message.MessageType);
                                }
                                
                                // handle the message
                                await handler.ExecuteAsync(message.MessageId, message.Data, stoppingToken);

                                // and mark it for removal
                                queueSource.RemoveMessage(message);
                            } catch (Exception e) {
                                _logger.LogError(e, "An error occurred while handling a message");

                                // Something went wrong and we couldn't handle the message so track the error and requeue the message
                                message.ErrorCount += 1;
                                message.ErrorMessage = e.Message;
                                message.LastRunTime = referenceTime;

                                message.NextRunTime = message.ErrorCount > 10 
                                    // If the message has failed too many times then dead-letter it
                                    ? DateTime.MaxValue 
                                    // Otherwise push it down the queue so it can be retried later
                                    : DateTime.UtcNow.AddMinutes(message.ErrorCount);

                                // requeue the message
                                queueSource.UpdateMessage(message);
                            }
                        }
                    }
                }

                if (messageCounter == 0) {
                    inactivityCounter += 1;
                } else {
                    inactivityCounter = 0;
                }

                // The longer we don't have any work to do, the longer we wait until the next check.
                // The benefit is that during periods of inactivity we aren't spamming the db too often
                // The downside is that when work does appear, the delay will impact the soonest time that it will be noticed.
                // We also cap the delay, so that the wait doesn't get too long.
                pollDelay = pollDelay.Add(TimeSpan.FromSeconds(Math.Min(Math.Floor(inactivityCounter / 120.0), 360)));
                _logger.LogInformation("Delay until next execution is {Delay}", pollDelay);
                await Task.Delay(pollDelay, stoppingToken);
            }
        }
    }
}
