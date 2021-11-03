using Multicloud.Messaging.Abstractions;
using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Multicloud.Messaging.Google
{
    [PublisherProvider(PublisherProviders.GooglePubSub)]
    internal class GooglePubSubPublisherAdapter : IMessagePublisher
    {
        private const string ProjectIdKey = "ProjectId";

        private readonly RetrySettings _retrySettings;

        private readonly ILogger<GooglePubSubPublisherAdapter> _logger = new NullLogger<GooglePubSubPublisherAdapter>();

        private readonly string _projectId;

        public GooglePubSubPublisherAdapter(IReadOnlyDictionary<string, string> options, IRetrySettingsFactory<RetrySettings> retrySettingsFactory, MessagingOptions messagingOptions, ILoggerFactory loggerFactory = null)
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
                _logger = loggerFactory.CreateLogger<GooglePubSubPublisherAdapter>();
            }

            if (!options.TryGetValue(ProjectIdKey, out var projectId))
            {
                throw new ArgumentException("ProjectId required.");
            }

            _projectId = projectId;
            _retrySettings = retrySettingsFactory.Create(options);
        }

        public async Task PublishAsync<TEntity>(string topic, TEntity message, CancellationToken cancellationToken = default)
        {
            var messageJson = JsonConvert.SerializeObject(message);
            var publisher = await GetClientAsync(topic, cancellationToken);
            await publisher.PublishAsync(messageJson);
        }

        public async Task PublishBatchAsync<TEntity>(string topic, IEnumerable<TEntity> messages, CancellationToken cancellationToken = default)
        {
            var arr = messages as TEntity[] ?? messages.ToArray();
            var publisher = await GetClientAsync(topic, cancellationToken);
            var publishTasks = arr.Select(x =>
                Task.Run(async () =>
                {
                    var messageJson = JsonConvert.SerializeObject(x);
                    await publisher.PublishAsync(messageJson);
                }, cancellationToken));
            await Task.WhenAll(publishTasks);
        }

        private async Task<PublisherClient> GetClientAsync(string topic, CancellationToken cancellationToken = default)
        {
            var topicName = TopicName.FromProjectTopic(_projectId, topic);

            if (_retrySettings == null)
            {
                return await PublisherClient.CreateAsync(
                    topicName, 
                    new PublisherClient.ClientCreationSettings(
                        publisherServiceApiSettings: new PublisherServiceApiSettings
                        {
                            CallSettings = CallSettings.FromCancellationToken(cancellationToken)
                        }
                    ));
            }

            return await PublisherClient.CreateAsync(
                topicName,
                new PublisherClient.ClientCreationSettings(
                    publisherServiceApiSettings: new PublisherServiceApiSettings
                    {
                        PublishSettings = CallSettings.FromRetry(_retrySettings),
                        CallSettings = CallSettings.FromCancellationToken(cancellationToken)
                    }
                ));
        }
    }
}
