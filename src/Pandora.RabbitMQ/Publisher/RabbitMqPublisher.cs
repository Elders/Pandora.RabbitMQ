using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.RabbitMQ.Bootstrap;
using Pandora.RabbitMQ.PandoraConfigurationMessageProcessors;
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

    public void Publish(ConfigurationRequest message)
    {
        string exchangeName = PandoraRabbitMqNamer.GetExchangeName();

        try
        {
            string routingKey = PandoraRabbitMqNamer.GetRoutingKey(message.ServiceKey);

            IModel exchangeModel = _channelResolver.Resolve(exchangeName, options, message.ServiceKey);
            IBasicProperties props = exchangeModel.CreateBasicProperties();
            props.Persistent = true;
            props.Headers = new Dictionary<string, object>();
            props.Headers.Add("pandora-message-type", ConfigurationRequest.ContractId);

            byte[] body = JsonSerializer.SerializeToUtf8Bytes(message);

            exchangeModel.BasicPublish(exchangeName, routingKey, false, props, body);

            _logger.LogInformation("Published message: {message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message: {message} to {exchange}", message, exchangeName);
        }
    }

    /// <summary>
    /// Publishes the response of the configured service
    /// </summary>
    /// <param name="message">The message that contains the response of the configured service</param>
    /// <param name="serviceKey">The key that will be used to construct the routing key <see cref="PandoraRabbitMqNamer.GetRoutingKey"/> where the message will be published to</param>
    public void Publish(ConfigurationResponse message, string serviceKey)
    {
        string exchangeName = PandoraRabbitMqNamer.GetExchangeName();

        try
        {
            string routingKey = PandoraRabbitMqNamer.GetRoutingKey(serviceKey);

            IModel exchangeModel = _channelResolver.Resolve(exchangeName, options, serviceKey);
            IBasicProperties props = exchangeModel.CreateBasicProperties();
            props.Persistent = true;
            props.Headers = new Dictionary<string, object>();
            props.Headers.Add("pandora-message-type", ConfigurationResponse.ContractId);

            byte[] body = JsonSerializer.SerializeToUtf8Bytes(message);

            exchangeModel.BasicPublish(exchangeName, routingKey, false, props, body);

            _logger.LogInformation("Published response message: {@message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish response message: {@message} to {exchange}", message, exchangeName);
        }
    }
}
