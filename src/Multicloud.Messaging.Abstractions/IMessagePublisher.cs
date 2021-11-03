using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Multicloud.Messaging.Abstractions
{
    public interface IMessagePublisher
    {
        Task PublishAsync<TEntity>(string topic, TEntity message, CancellationToken cancellationToken = default);

        Task PublishBatchAsync<TEntity>(string topic, IEnumerable<TEntity> messages, CancellationToken cancellationToken = default);
    }
}
