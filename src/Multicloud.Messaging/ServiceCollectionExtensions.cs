using Azure.Messaging.ServiceBus;
using Multicloud.Messaging.Abstractions;
using Multicloud.Messaging.Azure;
using Multicloud.Messaging.Google;
using Google.Api.Gax.Grpc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Multicloud.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCloudMessaging(this IServiceCollection serviceCollection, Action<MessagingOptions> configure = null)
        {
            return serviceCollection
                .AddSingleton(sp =>
                {
                    var opt = new MessagingOptions();

                    try
                    {
                        configure?.Invoke(opt);
                    }
                    catch
                    {
                        // Just swallow, options can't be initialized
                        // use default settings
                        // due to the caller's handler throws exception
                    }

                    return opt;
                })
                .AddSingleton<IMessagePublisherFactory, MessagePublisherFactory>()
                .AddSingleton(sp =>
                {
                    var publisherTypes = Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(x => x.IsClass
                                    && !x.IsAbstract
                                    && typeof(IMessagePublisher).IsAssignableFrom(x)
                                    && x.GetCustomAttributes<PublisherProviderAttribute>().Any())
                        .ToDictionary(
                            x => x.GetCustomAttributes<PublisherProviderAttribute>().First().Provider,
                            x => x);

                    return new Func<MessageProviderOptions, IMessagePublisher>(messageProviderOptions =>
                    {
                        if (messageProviderOptions == null)
                        {
                            throw new ArgumentNullException(nameof(messageProviderOptions));
                        }

                        if (!publisherTypes.TryGetValue(messageProviderOptions.Provider, out var publisherType))
                        {
                            throw new ArgumentException(
                                $"There is no publisher type connected to provider: {messageProviderOptions.Provider ?? "(null)"}. Check provider name.");
                        }

                        if (messageProviderOptions.Options == null)
                        {
                            throw new ArgumentException($"{nameof(messageProviderOptions.Options)} can't be null.");
                        }

                        return (IMessagePublisher)ActivatorUtilities.CreateInstance(sp, publisherType, messageProviderOptions.Options);
                    });
                })
                .AddSingleton<IRetrySettingsFactory<ServiceBusRetryOptions>, AzureServiceBusRetrySettingsFactory>()
                .AddSingleton<IRetrySettingsFactory<RetrySettings>, GooglePubSubRetrySettingsFactory>();
        }
    }
}
