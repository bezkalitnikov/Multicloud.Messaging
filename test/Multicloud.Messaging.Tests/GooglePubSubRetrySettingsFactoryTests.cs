using Multicloud.Messaging.Google;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Multicloud.Messaging.Tests
{
    public class GooglePubSubRetrySettingsFactoryTests
    {
        public const string Constant = "Constant";

        public const string Exponential = "Exponential";

        public const string RetryModeKey = "RetryMode";

        public const string MaxAttemptsKey = "MaxAttempts";

        public const string BackoffKey = "Backoff";

        public const string InitialBackoffKey = "InitialBackoff";

        public const string MaxBackoffKey = "MaxBackoff";

        public const string BackoffMultiplierKey = "BackoffMultiplier";

        public class ValidDataCaseSource
        {
            public static IEnumerable Data(string retryMode)
            {
                switch (retryMode)
                {
                    case Constant:
                        yield return new TestCaseData(new Dictionary<string, string>
                        {
                            {RetryModeKey, Constant},
                            {MaxAttemptsKey, "5"},
                            {BackoffKey, "300"}
                        });
                        break;
                    case Exponential:
                        yield return new TestCaseData(new Dictionary<string, string>
                        {
                            {RetryModeKey, Exponential},
                            {MaxAttemptsKey, "4"},
                            {InitialBackoffKey, "300"},
                            {MaxBackoffKey, "1500"},
                            {BackoffMultiplierKey, "2.0"}
                        });
                        break;
                    default:
                        yield return new TestCaseData(new Dictionary<string, string>());
                        break;
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

        public class ArgumentExceptionCaseSource
        {
            public static IEnumerable Data
            {
                get
                {
                    yield return new TestCaseData(new Dictionary<string, string>
                    {
                        {RetryModeKey, Constant},
                        {MaxAttemptsKey, "three"}
                    });
                    yield return new TestCaseData(new Dictionary<string, string>
                    {
                        {RetryModeKey, Exponential},
                        {MaxAttemptsKey, "4.5"},
                        {InitialBackoffKey, "three hundred"},
                        {BackoffMultiplierKey, null}
                    });
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

        [TestCaseSource(typeof(ValidDataCaseSource), nameof(ValidDataCaseSource.Data), new object[] { Constant })]
        [Parallelizable]
        public void FactoryReturnsOptionsConstantTest(IReadOnlyDictionary<string, string> options)
        {
            // Arrange
            var factory = new GooglePubSubRetrySettingsFactory();

            // Act
            var result = factory.Create(options);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(TimeSpan.FromMilliseconds(int.Parse(options[BackoffKey])), result.InitialBackoff);
            Assert.AreEqual(TimeSpan.FromMilliseconds(int.Parse(options[BackoffKey])), result.MaxBackoff);
            Assert.AreEqual(int.Parse(options[MaxAttemptsKey]), result.MaxAttempts);
        }

        [TestCaseSource(typeof(ValidDataCaseSource), nameof(ValidDataCaseSource.Data), new object[] { Exponential })]
        [Parallelizable]
        public void FactoryReturnsOptionsExponentialTest(IReadOnlyDictionary<string, string> options)
        {
            // Arrange
            var factory = new GooglePubSubRetrySettingsFactory();

            // Act
            var result = factory.Create(options);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(double.Parse(options[BackoffMultiplierKey]), result.BackoffMultiplier);
            Assert.AreEqual(TimeSpan.FromMilliseconds(int.Parse(options[InitialBackoffKey])), result.InitialBackoff);
            Assert.AreEqual(TimeSpan.FromMilliseconds(int.Parse(options[MaxBackoffKey])), result.MaxBackoff);
            Assert.AreEqual(int.Parse(options[MaxAttemptsKey]), result.MaxAttempts);
        }

        [TestCaseSource(typeof(ValidDataCaseSource), nameof(ValidDataCaseSource.Data), new object[] { null })]
        [Parallelizable]
        public void FactoryReturnsNullEmptyTest(IReadOnlyDictionary<string, string> options)
        {
            // Arrange
            var factory = new GooglePubSubRetrySettingsFactory();

            // Act
            var result = factory.Create(options);

            // Assert
            Assert.IsNull(result);
        }

        [TestCaseSource(typeof(ArgumentNullExceptionCaseSource), nameof(ArgumentNullExceptionCaseSource.Data))]
        [Parallelizable]
        public void FactoryThrowsArgumentNullExceptionTest(IReadOnlyDictionary<string, string> options)
        {
            // Arrange
            var factory = new GooglePubSubRetrySettingsFactory();
        
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
            var factory = new GooglePubSubRetrySettingsFactory();

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
            var factory = new GooglePubSubRetrySettingsFactory();

            // Act
            void Result() => factory.Create(options);

            // Assert
            Assert.Throws<ArgumentException>(Result);
        }
    }
}
