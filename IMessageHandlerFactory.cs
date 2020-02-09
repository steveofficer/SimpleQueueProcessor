using System;
using System.Collections.Generic;
using System.Linq;

namespace BackgroundQueue
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler GetHandlerForMessage(string messageType, IServiceProvider services);
    }

    public class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IDictionary<string, Type> _mapping;
        
        public MessageHandlerFactory(IDictionary<string, Type> mapping) {
            _mapping = mapping;
            var handlerType = typeof(IMessageHandler);
            if (mapping.Values.Any(t => !handlerType.IsAssignableFrom(t))) {
                throw new ArgumentException(nameof(mapping));
            }
        }

        public IMessageHandler GetHandlerForMessage(string messageType, IServiceProvider services)
        {
            var handlerType = _mapping[messageType];
            return services.GetService(handlerType) as IMessageHandler;
        }
    }
}