using Elders.Pandora.PandoraConfigurationMessageProcessors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.RabbitMQ.Bootstrap;
using RabbitMQ.Client;

namespace Pandora.RabbitMQ.Consumer;

public sealed class ConsumerFactory
{
    private AsyncConsumer _consumer;

    private readonly IPandoraConfigurationMessageProcessor _pandoraConfigurationMessageProcessor;
    private readonly RabbitMqOptions options;
    private readonly ConsumerPerQueueChannelResolver _channelResolver;
    private readonly ILogger<ConsumerFactory> _logger;

    public ConsumerFactory(IPandoraConfigurationMessageProcessor pandoraConfigurationMessageProcessor, IOptionsMonitor<RabbitMqOptions> optionsMonitor, ConsumerPerQueueChannelResolver channelResolver, ILogger<ConsumerFactory> logger)
    {
        _pandoraConfigurationMessageProcessor = pandoraConfigurationMessageProcessor;
        options = optionsMonitor.CurrentValue; //TODO: Implement onChange event
        _channelResolver = channelResolver;
        _logger = logger;
    }

    public void CreateAndStartConsumer(string serviceKey, CancellationToken cancellationToken)
    {
        string consumerChannelKey = PandoraRabbitMqNamer.GetConsumerChannelName(serviceKey);
        IModel channel = _channelResolver.Resolve(consumerChannelKey, options, options.VHost);
        string queueName = PandoraRabbitMqNamer.GetQueueName(serviceKey);

        _consumer = new AsyncConsumer(queueName, _pandoraConfigurationMessageProcessor, channel, _logger);
    }

    public async Task StopConsumerAsync()
    {
        if (_consumer is null)
            return;

        await _consumer.StopAsync();
    }
}