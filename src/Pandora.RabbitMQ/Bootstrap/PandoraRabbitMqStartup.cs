using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.RabbitMQ.Bootstrap.Management;
using Pandora.RabbitMQ.Bootstrap.Management.Model;
using RabbitMQ.Client;

namespace Pandora.RabbitMQ.Bootstrap;

public sealed class PandoraRabbitMqStartup
{
    private readonly RabbitMqClusterOptions _options;
    private readonly IRabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<PandoraRabbitMqStartup> _logger;

    public PandoraRabbitMqStartup(IOptionsMonitor<RabbitMqClusterOptions> optionsMonitor, IRabbitMqConnectionFactory connectionFactory, ILogger<PandoraRabbitMqStartup> logger)
    {
        _options = optionsMonitor.CurrentValue;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public void Start(string queuePrefix)
    {
        try
        {
            foreach (RabbitMqOptions clusterOption in _options.Clusters)
            {
                RabbitMqManagementClient rmqClient = new RabbitMqManagementClient(clusterOption);
                CreateVHost(rmqClient, clusterOption);

                using var connection = _connectionFactory.CreateConnectionWithOptions(clusterOption);
                using var channel = connection.CreateModel();
                RecoverModel(channel, queuePrefix);
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Pandora.RabbitMQ infrastructure.");
        }
    }

    private void RecoverModel(IModel model, string queuePrefix)
    {
        string exchangeName = PandoraRabbitMqNamer.GetExchangeName();
        string queueName = PandoraRabbitMqNamer.GetQueueName(queuePrefix);
        string routingKey = PandoraRabbitMqNamer.GetRoutingKey(queuePrefix);

        model.ExchangeDeclare(exchangeName, ExchangeType.Direct, true);
        model.QueueDeclare(queueName, true, false, false, null);
        model.QueueBind(queueName, exchangeName, routingKey);
    }

    private void CreateVHost(RabbitMqManagementClient client, RabbitMqOptions options)
    {
        if (client.GetVHosts().Any(vh => vh.Name == options.VHost) == false)
        {
            var vhost = client.CreateVirtualHost(options.VHost);
            var rabbitMqUser = client.GetUsers().SingleOrDefault(x => x.Name == options.Username);
            var permissionInfo = new PermissionInfo(rabbitMqUser, vhost);
            client.CreatePermission(permissionInfo);
        }
    }
}
