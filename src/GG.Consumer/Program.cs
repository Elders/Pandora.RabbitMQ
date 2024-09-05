using Elders.Pandora;
using GG.Consumer;
using Pandora.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddPandoraRabbitMqConsumer();
builder.Services.AddConsulPandoraConfigurationProcessor();

var host = builder.Build();
host.Run();
