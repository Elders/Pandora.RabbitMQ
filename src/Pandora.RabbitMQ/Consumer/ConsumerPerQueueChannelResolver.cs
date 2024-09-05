using Pandora.RabbitMQ.Bootstrap;
using Pandora.RabbitMQ.Publisher;

namespace Pandora.RabbitMQ.Consumer;

public class ConsumerPerQueueChannelResolver : ChannelResolverBase // channels per queue
{
    public ConsumerPerQueueChannelResolver(ConnectionResolver connectionResolver) : base(connectionResolver) { }
}