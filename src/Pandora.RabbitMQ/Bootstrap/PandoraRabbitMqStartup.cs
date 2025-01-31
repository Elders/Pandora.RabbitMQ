using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.RabbitMQ.Bootstrap.Management;
using Pandora.RabbitMQ.Bootstrap.Management.Model;
using RabbitMQ.Client;

namespace Pandora.RabbitMQ.Bootstrap;

public sealed class PandoraRabbitMqStartup
{
    private readonly RabbitMqOptions _options;
    private readonly IRabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<PandoraRabbitMqStartup> _logger;

    public PandoraRabbitMqStartup(IOptionsMonitor<RabbitMqOptions> optionsMonitor, IRabbitMqConnectionFactory connectionFactory, ILogger<PandoraRabbitMqStartup> logger)
    {
        _options = optionsMonitor.CurrentValue;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public void Start(string queuePrefix)
    {
        try
        {
            RabbitMqManagementClient rmqClient = new RabbitMqManagementClient(_options);
            CreateVHost(rmqClient, _options);

            using var connection = _connectionFactory.CreateConnectionWithOptions(_options);
            using var channel = connection.CreateModel();
            RecoverModel(channel, queuePrefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ infrastructure.");
            throw;
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
