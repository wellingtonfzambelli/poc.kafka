using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using poc.kafka.crosscutting.Domain;
using poc.kafka.crosscutting.Kafka;

namespace poc.kafka.consumer.Jobs;

internal sealed class UserConsumerJob : BackgroundService
{
    private readonly IUserKafka _userKafka;
    private readonly ILogger<UserConsumerJob> _logger;

    public UserConsumerJob(IUserKafka userKafka, ILogger<UserConsumerJob> logger)
    {
        _userKafka = userKafka;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserConsumerJob started in background.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation($"UserConsumerJob task started at {DateTime.Now}");

            User user = await _userKafka.ConsumeAsync(stoppingToken);
        }

        _logger.LogInformation("UserConsumerJob service was cancelled.");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application has finished");
        return Task.CompletedTask;
    }
}