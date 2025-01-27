namespace Pandora.RabbitMQ.PandoraConfigurationMessageProcessors;

/// <summary>
/// Implement and register this interface in your service consumer to be be able to recieve the messages
/// </summary>
public interface IPandoraConfigurationMessageProcessor
{
    public Task ProcessAsync(IConfigurationMessage message);
}
