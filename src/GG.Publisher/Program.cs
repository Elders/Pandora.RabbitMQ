using GG.Publisher;
using Pandora.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddPandoraRabbitMqPublisher();

var host = builder.Build();
host.Run();
