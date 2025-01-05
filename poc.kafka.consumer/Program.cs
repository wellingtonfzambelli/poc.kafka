using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using poc.kafka.consumer.Jobs;
using poc.kafka.crosscutting.Kafka;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<UserConsumerJob>();
builder.Services.AddTransient<IUserProducer, UserProducer>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

using var host = builder.Build();
await host.RunAsync();