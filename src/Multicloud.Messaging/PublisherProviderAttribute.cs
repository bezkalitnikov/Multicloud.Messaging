using System;

namespace Multicloud.Messaging
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class PublisherProviderAttribute : Attribute
    {
        public string Provider { get; }

        public PublisherProviderAttribute(string provider)
        {
            Provider = provider;
        }
    }
}
