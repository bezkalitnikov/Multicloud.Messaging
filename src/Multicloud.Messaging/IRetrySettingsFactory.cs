using System.Collections.Generic;

namespace Multicloud.Messaging
{
    internal interface IRetrySettingsFactory<out T>
    {
        T Create(IReadOnlyDictionary<string, string> options);
    }
}
