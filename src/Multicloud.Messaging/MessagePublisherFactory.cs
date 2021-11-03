using Multicloud.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Multicloud.Messaging.Tests")]

namespace Multicloud.Messaging
{
    public class MessagePublisherFactory : IMessagePublisherFactory
    {
        private readonly ILogger _logger = new NullLogger<MessagePublisherFactory>();

        private readonly Func<MessageProviderOptions, IMessagePublisher> _publisherFactory;

        public MessagePublisherFactory(Func<MessageProviderOptions, IMessagePublisher> publisherFactory, MessagingOptions messagingOptions, ILoggerFactory loggerFactory = null)
        {
            _publisherFactory = publisherFactory ?? throw new ArgumentNullException(nameof(publisherFactory));

            if (messagingOptions == null)
            {
                throw new ArgumentNullException(nameof(messagingOptions));
            }

            if (messagingOptions.EnableLogging && loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<MessagePublisherFactory>();
            }
        }

        public IMessagePublisher Create(MessageProviderOptions messageProviderOptions)
        {
            return _publisherFactory(messageProviderOptions);
        }
    }
}
