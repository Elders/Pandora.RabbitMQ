using RabbitMQ.Client;

namespace Pandora.RabbitMQ.Bootstrap;

public interface IRabbitMqConnectionFactory
{
    IConnection CreateConnectionWithOptions(RabbitMqOptions options);
}
