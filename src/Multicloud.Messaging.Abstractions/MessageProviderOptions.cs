using System.Collections.Generic;

namespace Multicloud.Messaging.Abstractions
{
    public class MessageProviderOptions
    {
        public string Provider { get; set; }

        public IReadOnlyDictionary<string, string> Options { get; set; }
    }
}
