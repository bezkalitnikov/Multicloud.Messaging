namespace Multicloud.Messaging.Abstractions
{
    public interface IMessagePublisherFactory
    {
        IMessagePublisher Create(MessageProviderOptions messageProviderOptions);
    }
}
