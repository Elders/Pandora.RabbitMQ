using Elders.Pandora.PandoraConfigurationMessageProcessors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.RabbitMQ.Bootstrap;
using RabbitMQ.Client;
using System.Text.Json;

namespace Pandora.RabbitMQ.Publisher;

public sealed class RabbitMqPublisher
{
    private readonly RabbitMqOptions options;
    private readonly PublisherChannelResolver _channelResolver;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IOptionsMonitor<RabbitMqOptions> optionsMonitor, PublisherChannelResolver channelResolver, ILogger<RabbitMqPublisher> logger)
    {
        options = optionsMonitor.CurrentValue; // TODO: Implement onChange event
        _channelResolver = channelResolver;
        _logger = logger;
    }

    public void Publish(ConfigurationMessage message)
    {
        string exchangeName = PandoraRabbitMqNamer.GetExchangeName();

        try
        {
            string routingKey = PandoraRabbitMqNamer.GetRoutingKey(message.ServiceKey);

            IModel exchangeModel = _channelResolver.Resolve(exchangeName, options, message.ServiceKey);
            IBasicProperties props = exchangeModel.CreateBasicProperties();
            props.Persistent = true;

            byte[] body = JsonSerializer.SerializeToUtf8Bytes(message);

            exchangeModel.BasicPublish(exchangeName, routingKey, false, props, body);

            _logger.LogInformation("Published message: {message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message: {message} to {exchange}", message, exchangeName);
        }
    }
}