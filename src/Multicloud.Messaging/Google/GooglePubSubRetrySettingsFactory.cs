using Google.Api.Gax.Grpc;
using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Multicloud.Messaging.Google
{
    internal class GooglePubSubRetrySettingsFactory : IRetrySettingsFactory<RetrySettings>
    {
        private const string Constant = "Constant";

        private const string Exponential = "Exponential";

        private const string RetryModeKey = "RetryMode";

        private const string MaxAttemptsKey = "MaxAttempts";

        private const string BackoffKey = "Backoff";

        private const string InitialBackoffKey = "InitialBackoff";

        private const string MaxBackoffKey = "MaxBackoff";

        private const string BackoffMultiplierKey = "BackoffMultiplier";

        public RetrySettings Create(IReadOnlyDictionary<string, string> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.TryGetValue(RetryModeKey, out var retryMode))
            {
                if (options.ContainsKey(MaxAttemptsKey))
                {
                    throw new ArgumentException(
                        $@"Invalid settings. ""{RetryModeKey}"" is missing, but ""{MaxAttemptsKey}"" is present.");
                }

                if (options.ContainsKey(BackoffKey))
                {
                    throw new ArgumentException(
                        $@"Invalid settings. ""{RetryModeKey}"" is missing, but ""{BackoffKey}"" is present.");
                }

                if (options.ContainsKey(InitialBackoffKey))
                {
                    throw new ArgumentException(
                        $@"Invalid settings. ""{RetryModeKey}"" is missing, but ""{InitialBackoffKey}"" present. Remove retry settings completely, or define them properly.");
                }

                if (options.ContainsKey(MaxBackoffKey))
                {
                    throw new ArgumentException(
                        $@"Invalid settings. ""{RetryModeKey}"" is missing, but ""{MaxBackoffKey}"" present. Remove retry settings completely, or define them properly.");
                }

                if (options.ContainsKey(BackoffMultiplierKey))
                {
                    throw new ArgumentException(
                        $@"Invalid settings. ""{RetryModeKey}"" is missing, but ""{BackoffMultiplierKey}"" present. Remove retry settings completely, or define them properly.");
                }

                return null;
            }

            switch (retryMode)
            {
                case Constant:
                    return GetConstantRetrySettings(options);
                case Exponential:
                    return GetExponentialRetrySettings(options);
                default:
                    throw new ArgumentOutOfRangeException($"Retry mode: {retryMode} not supported.");
            }
        }

        private RetrySettings GetConstantRetrySettings(IReadOnlyDictionary<string, string> options)
        {
            int maxAttempts;

            try
            {
                maxAttempts = int.Parse(options[MaxAttemptsKey]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxAttemptsKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxAttemptsKey}"" has a value: {options[MaxAttemptsKey] ?? "(null)"}. Must be integer.");
            }

            TimeSpan backoff;

            try
            {
                backoff = TimeSpan.FromMilliseconds(int.Parse(options[BackoffKey]));
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{BackoffKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{BackoffKey}"" has a value: {options[BackoffKey] ?? "(null)"}. Must be integer.");
            }

            return RetrySettings.FromConstantBackoff(
                maxAttempts,
                backoff,
                RetrySettings.FilterForStatusCodes(StatusCode.Unavailable));
        }

        private RetrySettings GetExponentialRetrySettings(IReadOnlyDictionary<string, string> options)
        {
            int maxAttempts;

            try
            {
                maxAttempts = int.Parse(options[MaxAttemptsKey]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxAttemptsKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxAttemptsKey}"" has a value: {options[MaxAttemptsKey] ?? "(null)"}. Must be integer.");
            }

            TimeSpan initialBackoff;

            try
            {
                initialBackoff = TimeSpan.FromMilliseconds(int.Parse(options[InitialBackoffKey]));
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{InitialBackoffKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{InitialBackoffKey}"" has a value: {options[InitialBackoffKey] ?? "(null)"}. Must be integer.");
            }

            TimeSpan maxBackoff;

            try
            {
                maxBackoff = TimeSpan.FromMilliseconds(int.Parse(options[MaxBackoffKey]));
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxBackoffKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{MaxBackoffKey}"" has a value: {options[MaxBackoffKey] ?? "(null)"}. Must be integer.");
            }

            double backoffMultiplier;

            try
            {
                backoffMultiplier = double.Parse(options[BackoffMultiplierKey]);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{BackoffMultiplierKey}"" not found.");
            }
            catch
            {
                throw new ArgumentException(
                    $@"Invalid settings. ""{BackoffMultiplierKey}"" has a value: {options[BackoffMultiplierKey] ?? "(null)"}. Must be integer.");
            }

            return RetrySettings.FromExponentialBackoff(
                maxAttempts,
                initialBackoff,
                maxBackoff,
                backoffMultiplier,
                RetrySettings.FilterForStatusCodes(StatusCode.Unavailable));
        }
    }
}
