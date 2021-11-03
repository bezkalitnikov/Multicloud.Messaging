using Azure.Messaging.ServiceBus;
using Multicloud.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Multicloud.Messaging.Azure
{
    [PublisherProvider(PublisherProviders.AzureServiceBus)]
    internal class AzureServiceBusSenderAdapter : IMessagePublisher
    {
        private const string ConnectionStringKey = "ConnectionString";

        private readonly ServiceBusRetryOptions _retryOptions;

        private readonly string _connectionString;

        private readonly ILogger _logger = new NullLogger<AzureServiceBusSenderAdapter>();

        public AzureServiceBusSenderAdapter(IReadOnlyDictionary<string, string> options, IRetrySettingsFactory<ServiceBusRetryOptions> retrySettingsFactory, MessagingOptions messagingOptions, ILoggerFactory loggerFactory = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (retrySettingsFactory == null)
            {
                throw new ArgumentNullException(nameof(retrySettingsFactory));
            }

            if (messagingOptions == null)
            {
                throw new ArgumentNullException(nameof(messagingOptions));
            }

            if (messagingOptions.EnableLogging && loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<AzureServiceBusSenderAdapter>();
            }

            if (!options.TryGetValue(ConnectionStringKey, out var connectionString))
            {
                throw new ArgumentException("Connection string required.");
            }

            _retryOptions = retrySettingsFactory.Create(options);
            _connectionString = connectionString;
        }

        public async Task PublishAsync<TEntity>(string topic, TEntity message, CancellationToken cancellationToken = default)
        {
            ServiceBusClient serviceBusClient = null;
            ServiceBusSender serviceBusSender = null;

            try
            {
                serviceBusClient = new ServiceBusClient(_connectionString, new ServiceBusClientOptions
                {
                    RetryOptions = _retryOptions
                });

                serviceBusSender = serviceBusClient.CreateSender(topic);
                var messageJson = JsonConvert.SerializeObject(message);
                var serviceBusMessage = new ServiceBusMessage(messageJson);
                await serviceBusSender.SendMessageAsync(serviceBusMessage, cancellationToken);
            }
            finally
            {
                if (serviceBusSender != null)
                {
                    await serviceBusSender.DisposeAsync();
                }

                if (serviceBusClient != null)
                {
                    await serviceBusClient.DisposeAsync();
                }
            }
        }

        public async Task PublishBatchAsync<TEntity>(string topic, IEnumerable<TEntity> messages, CancellationToken cancellationToken = default)
        {
            ServiceBusClient serviceBusClient = null;
            ServiceBusSender serviceBusSender = null;

            try
            {
                var arr = messages as TEntity[] ?? messages.ToArray();
                serviceBusClient = new ServiceBusClient(_connectionString, new ServiceBusClientOptions
                {
                    RetryOptions = _retryOptions
                });

                serviceBusSender = serviceBusClient.CreateSender(topic);

                var batch = await serviceBusSender.CreateMessageBatchAsync(cancellationToken);

                foreach (var message in arr)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var messageJson = JsonConvert.SerializeObject(message);
                    var serviceBusMessage = new ServiceBusMessage(messageJson);

                    if (batch.TryAddMessage(serviceBusMessage))
                    {
                        continue;
                    }

                    await serviceBusSender.SendMessagesAsync(batch, cancellationToken);
                    batch.Dispose();
                    batch = await serviceBusSender.CreateMessageBatchAsync(cancellationToken);

                    if (batch.TryAddMessage(serviceBusMessage))
                    {
                        continue;
                    }

                    batch.Dispose();
                    throw new Exception($"The message is too large to fit in the batch: {message}.");
                }

                await serviceBusSender.SendMessagesAsync(batch, cancellationToken);
                batch.Dispose();
            }
            finally
            {
                if (serviceBusSender != null)
                {
                    await serviceBusSender.DisposeAsync();
                }

                if (serviceBusClient != null)
                {
                    await serviceBusClient.DisposeAsync();
                }
            }
        }
    }
}
