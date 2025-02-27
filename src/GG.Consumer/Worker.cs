using Pandora.RabbitMQ.Bootstrap;
using Pandora.RabbitMQ.Consumer;

namespace GG.Consumer;

public class Worker : BackgroundService
{
    private readonly PandoraRabbitMqStartup _rabbitMqStartup;
    private readonly PandoraRabbitMqConsumerFactory _consumerFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(PandoraRabbitMqStartup rabbitMqStartup, PandoraRabbitMqConsumerFactory consumerFactory, ILogger<Worker> logger)
    {
        _rabbitMqStartup = rabbitMqStartup;
        _consumerFactory = consumerFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _rabbitMqStartup.Start("giService");
        _rabbitMqStartup.Start("topService");

        _consumerFactory.CreateAndStartConsumer("giService", stoppingToken);
        _consumerFactory.CreateAndStartConsumer("topService", stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumerFactory.StopConsumerAsync();
    }
}
