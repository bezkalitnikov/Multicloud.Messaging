using Azure.Messaging.ServiceBus;
using Multicloud.Messaging.Azure;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Multicloud.Messaging.Tests
{
    public class AzureServiceBusRetrySettingsFactoryTests
    {
        public const string RetryModeKey = "RetryMode";

        public const string Fixed = "Fixed";

        public const string Exponential = "Exponential";

        public const string DelayKey = "Delay";

        public const string MaxDelayKey = "MaxDelay";

        public const string MaxRetriesKey = "MaxRetries";

        public class ValidDataCaseSource
        {
            public static IEnumerable Data
            {
                get
                {
                    yield return new TestCaseData(new Dictionary<string, string>());
                    yield return new TestCaseData(new Dictionary<string, string>
                    {
                        {RetryModeKey, Fixed},
                        {MaxRetriesKey, "7"},
                        {DelayKey, "1000"},
                        {MaxDelayKey, "1500"}
                    });
                    yield return new TestCaseData(new Dictionary<string, string>
                    {
                        {RetryModeKey, Exponential},
                        {MaxRetriesKey, "4"},
                        {DelayKey, "300"},
                        {MaxDelayKey, "1200"},
                    });
                }
            }
        }

        public class ArgumentNullExceptionCaseSource
        {
            public static IEnumerable Data
            {
                get
                {
                    yield return new TestCaseData(null);
                }
            }
        }

        public class ArgumentOutOfRangeExceptionCaseSource
        {
            public static IEnumerable Data
            {
                get
                {
                    yield return new TestCaseData(new Dictionary<string, string>
                    {
                        {RetryModeKey, "unexpected"}
                    });
                }
            }
        }

        public class ArgumentExceptionCaseSource
        {
            public static IEnumerable Data
            {
                get
                {
                    yield return new TestCaseData(new Dictionary<string, string>
                    {
                        {RetryModeKey, Fixed},
                        {MaxRetriesKey, "three"}
                    });
                    yield return new TestCaseData(new Dictionary<string, string>
                    {
                        {RetryModeKey, Exponential},
                        {MaxRetriesKey, "4.5"},
                        {DelayKey, "three hundred"},
                        {MaxDelayKey, null}
                    });
                }
            }
        }

        [TestCaseSource(typeof(ValidDataCaseSource), nameof(ValidDataCaseSource.Data))]
        [Parallelizable]
        public void FactoryReturnsOptionsTest(IReadOnlyDictionary<string, string> options)
        {
            // Arrange
            var factory = new AzureServiceBusRetrySettingsFactory();

            // Act
            var result = factory.Create(options);

            // Assert
            Assert.IsNotNull(result);

            if (!options.Any())
            {
                return;
            }

            Assert.AreEqual(TimeSpan.FromMilliseconds(int.Parse(options[DelayKey])), result.Delay);
            Assert.AreEqual(TimeSpan.FromMilliseconds(int.Parse(options[MaxDelayKey])), result.MaxDelay);
            Assert.AreEqual(int.Parse(options[MaxRetriesKey]), result.MaxRetries);
            Assert.AreEqual((ServiceBusRetryMode)Enum.Parse(typeof(ServiceBusRetryMode), options[RetryModeKey]), result.Mode);
        }

        [TestCaseSource(typeof(ArgumentNullExceptionCaseSource), nameof(ArgumentNullExceptionCaseSource.Data))]
        [Parallelizable]
        public void FactoryThrowsArgumentNullExceptionTest(IReadOnlyDictionary<string, string> options)
        {
            // Arrange
            var factory = new AzureServiceBusRetrySettingsFactory();

            // Act
            void Result() => factory.Create(options);

            // Assert
            Assert.Throws<ArgumentNullException>(Result);
        }

        [TestCaseSource(typeof(ArgumentOutOfRangeExceptionCaseSource), nameof(ArgumentOutOfRangeExceptionCaseSource.Data))]
        [Parallelizable]
        public void FactoryThrowsArgumentOutOfRangeExceptionTest(IReadOnlyDictionary<string, string> options)
        {
            // Arrange
            var factory = new AzureServiceBusRetrySettingsFactory();

            // Act
            void Result() => factory.Create(options);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(Result);
        }

        [TestCaseSource(typeof(ArgumentExceptionCaseSource), nameof(ArgumentExceptionCaseSource.Data))]
        [Parallelizable]
        public void FactoryThrowsArgumentExceptionTest(IReadOnlyDictionary<string, string> options)
        {
            // Arrange
            var factory = new AzureServiceBusRetrySettingsFactory();

            // Act
            void Result() => factory.Create(options);

            // Assert
            Assert.Throws<ArgumentException>(Result);
        }
    }
}
