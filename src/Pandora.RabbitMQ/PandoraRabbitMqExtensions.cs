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
        services.AddSingleton<PandoraRabbitMqStartup>();
        services.AddSingleton<ConnectionResolver>();
      
        return services;
    }

    public static IServiceCollection AddPandoraRabbitMqPublisher(this IServiceCollection services)
    {
        services.AddPandoraRabbitMqBase();

        services.AddOptions<RabbitMqClusterOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            configuration.GetRequiredSection("pandora:rabbitmq:publisher").Bind(options.Clusters);
        });

        services.AddSingleton<PublisherChannelResolver>();
        services.AddSingleton<PandoraRabbitMqPublisher>();

        return services;
    }

    public static IServiceCollection AddPandoraRabbitMqConsumer(this IServiceCollection services)
    {
        services.AddPandoraRabbitMqBase();

        services.AddOptions<RabbitMqOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            configuration.GetRequiredSection("pandora:rabbitmq:consumer").Bind(options);
        });

        services.AddSingleton<ConsumerPerQueueChannelResolver>();
        services.AddSingleton<PandoraRabbitMqConsumerFactory>();

        return services;
    }
}

