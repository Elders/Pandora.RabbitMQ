using GG.Consumer;
using Pandora.RabbitMQ;
using Pandora.RabbitMQ.PandoraConfigurationMessageProcessors;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddLogging();
builder.Services.AddPandoraRabbitMqConsumer();
builder.Services.AddSingleton<IPandoraConfigurationMessageProcessor, TestProcessor>();

var host = builder.Build();

host.Run();
