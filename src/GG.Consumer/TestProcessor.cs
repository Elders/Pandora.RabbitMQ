using Pandora.RabbitMQ.PandoraConfigurationMessageProcessors;

namespace GG.Consumer
{
    public class TestProcessor : IPandoraConfigurationMessageProcessor
    {
        private readonly ILogger<TestProcessor> logger;

        public TestProcessor(ILogger<TestProcessor> logger)
        {
            this.logger = logger;
        }

        public Task ProcessAsync(IConfigurationMessage message)
        {
            logger.LogInformation("Hello service {service} with tenant {tenant}", message.ServiceKey, message.Tenant);
            return Task.CompletedTask;
        }
    }
}
