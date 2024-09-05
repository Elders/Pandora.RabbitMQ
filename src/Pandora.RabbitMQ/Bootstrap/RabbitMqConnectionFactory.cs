using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Pandora.RabbitMQ.Bootstrap;

public sealed class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly ILogger<RabbitMqConnectionFactory> logger;

    public RabbitMqConnectionFactory(ILogger<RabbitMqConnectionFactory> logger)
    {
        this.logger = logger;
    }

    public IConnection CreateConnectionWithOptions(RabbitMqOptions options)
    {
        logger.LogDebug("Loaded RabbitMQ options are {@Options}", options);

        while (true)
        {
            try
            {
                var connectionFactory = new ConnectionFactory();
                connectionFactory.Port = options.Port;
                connectionFactory.UserName = options.Username;
                connectionFactory.Password = options.Password;
                connectionFactory.VirtualHost = options.VHost;
                connectionFactory.DispatchConsumersAsync = true;
                connectionFactory.AutomaticRecoveryEnabled = true;
                connectionFactory.Ssl.Enabled = options.UseSsl;
                connectionFactory.EndpointResolverFactory = (_) => MultipleEndpointResolver.ComposeEndpointResolver(options);

                return connectionFactory.CreateConnection();
            }
            catch (Exception ex)
            {
                if (ex is BrokerUnreachableException)
                    logger.LogDebug("Failed to create RabbitMQ connection using options {@options}. Retrying...", options);
                else
                    logger.LogWarning(ex, "Failed to create RabbitMQ connection using options {@options}. Retrying...", options);

                Task.Delay(5000).GetAwaiter().GetResult();
            }
        }
    }

    private class MultipleEndpointResolver : DefaultEndpointResolver
    {
        MultipleEndpointResolver(AmqpTcpEndpoint[] amqpTcpEndpoints) : base(amqpTcpEndpoints) { }

        public static MultipleEndpointResolver ComposeEndpointResolver(RabbitMqOptions options)
        {
            AmqpTcpEndpoint[] endpoints = AmqpTcpEndpoint.ParseMultiple(options.Server);

            if (options.UseSsl is false)
                return new MultipleEndpointResolver(endpoints);

            foreach (AmqpTcpEndpoint endp in endpoints)
            {
                endp.Ssl.Enabled = true;
                endp.Ssl.ServerName = options.Server;
            }

            return new MultipleEndpointResolver(endpoints);
        }
    }
}