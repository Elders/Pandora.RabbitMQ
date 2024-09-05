using Pandora.RabbitMQ.Bootstrap;
using Pandora.RabbitMQ.Consumer;

namespace GG.Consumer;

public class Worker : BackgroundService
{
    private readonly RabbitMqStartup _rabbitMqStartup;
    private readonly ConsumerFactory _consumerFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(RabbitMqStartup rabbitMqStartup, ConsumerFactory consumerFactory, ILogger<Worker> logger)
    {
        _rabbitMqStartup = rabbitMqStartup;
        _consumerFactory = consumerFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _rabbitMqStartup.Start("giService");
        _consumerFactory.CreateAndStartConsumer("giService", stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
