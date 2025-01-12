using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using poc.kafka.consumer.Jobs;
using poc.kafka.crosscutting.Kafka;
using poc.kafka.crosscutting.Settings;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<UserConsumerJob>();

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<KafkaSettings>>().Value);

builder.Services.AddTransient<IKafkaService, KafkaService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

using var host = builder.Build();
await host.RunAsync();