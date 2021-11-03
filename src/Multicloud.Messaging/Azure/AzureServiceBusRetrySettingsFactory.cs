using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;

namespace Multicloud.Messaging.Azure
{
    internal class AzureServiceBusRetrySettingsFactory : IRetrySettingsFactory<ServiceBusRetryOptions>
    {
        private const string RetryModeKey = "RetryMode";

        private const string Fixed = "Fixed";

        private const string Exponential = "Exponential";

        private const string DelayKey = "Delay";

        private const string MaxDelayKey = "MaxDelay";

        private const string MaxRetriesKey = "MaxRetries";

        public ServiceBusRetryOptions Create(IReadOnlyDictionary<string, string> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var retryMode = GetRetryMode(options);

            if (!retryMode.HasValue)
            {
                if (options.ContainsKey(DelayKey))
                {
                    throw new ArgumentException(
                        $@"Invalid settings. ""{RetryModeKey}"" is missing, but ""{DelayKey}"" is present.");
                }

                if (options.ContainsKey(MaxDelayKey))
                {
                    throw new ArgumentException(
                        $@"Invalid settings. ""{RetryModeKey}"" is missing, but ""{MaxDelayKey}"" is present.");
                }

                if (options.ContainsKey(MaxRetriesKey))
                {
                    throw new ArgumentException(
                        $@"Invalid settings. ""{RetryModeKey}"" is missing, but ""{MaxRetriesKey}"" is present.");
                }

                return new ServiceBusRetryOptions();
            }

            TimeSpan delay;

            try
            {
                delay = TimeSpan.FromMilliseconds(int.Parse(options[DelayKey]));
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{DelayKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{DelayKey}"" has a value: {options[DelayKey] ?? "(null)"}. Must be integer.");
            }

            TimeSpan maxDelay;

            try
            {
                maxDelay = TimeSpan.FromMilliseconds(int.Parse(options[MaxDelayKey]));
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxDelayKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxDelayKey}"" has a value: {options[MaxDelayKey] ?? "(null)"}. Must be integer.");
            }

            int maxRetries;

            try
            {
                maxRetries = int.Parse(options[MaxRetriesKey]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxRetriesKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxRetriesKey}"" has a value: {options[MaxRetriesKey] ?? "(null)"}. Must be integer.");
            }

            return new ServiceBusRetryOptions
            {
                Mode = retryMode.Value,
                Delay = delay,
                MaxDelay = maxDelay,
                MaxRetries = maxRetries
            };
        }

        private ServiceBusRetryMode? GetRetryMode(IReadOnlyDictionary<string, string> options)
        {
            if (!options.TryGetValue(RetryModeKey, out var retryMode))
            {
                return null;
            }

            switch (retryMode)
            {
                case Fixed:
                    return ServiceBusRetryMode.Fixed;
                case Exponential:
                    return ServiceBusRetryMode.Exponential;
                default:
                    throw new ArgumentOutOfRangeException($"Retry mode: {retryMode} not supported.");
            }
        }
    }
}
