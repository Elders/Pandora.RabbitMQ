using Elders.Pandora.PandoraConfigurationMessageProcessors;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Pandora.RabbitMQ.Consumer;

public sealed class AsyncConsumer : AsyncEventingBasicConsumer
{
    private bool isСurrentlyConsuming;

    private readonly IPandoraConfigurationMessageProcessor _pandoraConfigurationMessageProcessor;
    private readonly IModel _model;
    private readonly ILogger _logger;

    public AsyncConsumer(string queuName, IPandoraConfigurationMessageProcessor pandoraConfigurationMessageProcessor, IModel model, ILogger logger) : base(model)
    {
        model.BasicQos(0, 1, false); // prefetch allow to avoid buffer of messages on the flight
        model.BasicConsume(queuName, false, string.Empty, this); // we should use autoAck: false to avoid messages loosing

        _pandoraConfigurationMessageProcessor = pandoraConfigurationMessageProcessor;
        _model = model;
        _logger = logger;
        isСurrentlyConsuming = false;
        Received += AsyncListener_Received;
    }

    public async Task StopAsync()
    {
        // 1. We detach the listener so ther will be no new messages coming from the queue
        Received -= AsyncListener_Received;

        // 2. Wait to handle any messages in progress
        while (isСurrentlyConsuming)
        {
            // We are trying to wait all consumers to finish their current work.
            // Ofcourse the host could be forcibly shut down but we are doing our best.

            await Task.Delay(10).ConfigureAwait(false);
        }

        if (_model.IsOpen)
            _model.Abort();
    }

    private async Task AsyncListener_Received(object sender, BasicDeliverEventArgs @event)
    {
        try
        {
            _logger.LogDebug("Message received. Sender {sender}.", sender.GetType().Name);
            isСurrentlyConsuming = true;

            if (sender is AsyncEventingBasicConsumer consumer)
                await ProcessAsync(@event, consumer).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver message");
            throw;
        }
        finally
        {
            isСurrentlyConsuming = false;
        }
    }

    private async Task ProcessAsync(BasicDeliverEventArgs ev, AsyncEventingBasicConsumer consumer)
    {
        ConfigurationMessage message;
        try
        {
            message = JsonSerializer.Deserialize<ConfigurationMessage>(ev.Body.ToArray());
        }
        catch (Exception ex)
        {
            // TODO: send to dead letter exchange/queue
            _logger.LogError(ex, $"Failed to process message. Failed to deserialize: {Convert.ToBase64String(ev.Body.ToArray())}");
            Ack(ev, consumer);
            return;
        }

        try
        {
            await _pandoraConfigurationMessageProcessor.ProcessMessageAsync(message).ConfigureAwait(false);
            // Do some work with the message
            _logger.LogInformation("Received message: {message}", message.ServiceKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message. {message}", JsonSerializer.Serialize(message));
        }
        finally
        {
            // Acknowledge the message
            Ack(ev, consumer);
        }

        static void Ack(BasicDeliverEventArgs ev, AsyncEventingBasicConsumer consumer)
        {
            if (consumer.Model.IsOpen)
            {
                consumer.Model.BasicAck(ev.DeliveryTag, false);
            }
        }
    }
}
