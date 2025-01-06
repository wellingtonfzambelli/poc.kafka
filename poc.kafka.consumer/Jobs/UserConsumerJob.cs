using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using poc.kafka.crosscutting.Domain;
using poc.kafka.crosscutting.Kafka;
using System.Text.Json;

namespace poc.kafka.consumer.Jobs;

internal sealed class UserConsumerJob : BackgroundService
{
    private readonly ILogger<UserConsumerJob> _logger;
    private readonly IConsumer<Ignore, string>? _consumer;

    public UserConsumerJob(IUserKafka userKafka, ILogger<UserConsumerJob> logger)
        : this(userKafka) =>
        _logger = logger;

    private UserConsumerJob(IUserKafka userKafka)
    {
        _consumer = userKafka.GetConsumer();
        _consumer.Subscribe(userKafka.GetTopicName());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_consumer.Consume(TimeSpan.FromSeconds(5)) // waiting when doesn't have any message
                is var consumeResult && consumeResult is null)
                    continue;

                _logger.LogInformation($"Getting message: {consumeResult.Message.Value}");

                User? user = JsonSerializer.Deserialize<User>(consumeResult.Message.Value);

                _consumer.Commit(consumeResult);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError($"UserConsumerJob error: {ex.Message}");
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application has finished");

        if (_consumer is not null)
        {
            _consumer.Close(); // Close  the cosumer 
            _consumer.Dispose(); // dispose the resources
        }

        return Task.CompletedTask;
    }
}