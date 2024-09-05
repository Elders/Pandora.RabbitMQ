using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.RabbitMQ.Bootstrap;
using Pandora.RabbitMQ.Consumer;
using Pandora.RabbitMQ.Publisher;

namespace Pandora.RabbitMQ;

public static class PandoraRabbitMqExtensions
{
    // TODO: Rethink all lifecycles of the services
    internal static IServiceCollection AddPandoraRabbitMqBase(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddSingleton<RabbitMqStartup>();
        services.AddSingleton<ConnectionResolver>();

        services.AddOptions<RabbitMqOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            configuration.GetSection("pandora:rabbitmq").Bind(options);
        });

        return services;
    }

    public static IServiceCollection AddPandoraRabbitMqPublisher(this IServiceCollection services)
    {
        services.AddPandoraRabbitMqBase();

        services.AddSingleton<PublisherChannelResolver>();
        services.AddSingleton<RabbitMqPublisher>();

        return services;
    }

    public static IServiceCollection AddPandoraRabbitMqConsumer(this IServiceCollection services)
    {
        services.AddPandoraRabbitMqBase();

        services.AddSingleton<ConsumerPerQueueChannelResolver>();
        services.AddSingleton<ConsumerFactory>();

        return services;
    }
}

