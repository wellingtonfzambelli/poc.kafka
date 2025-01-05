using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace poc.kafka.consumer.Jobs;

internal sealed class UserConsumerJob : BackgroundService
{
    private readonly ILogger<UserConsumerJob> _logger;

    public UserConsumerJob(ILogger<UserConsumerJob> logger) =>
        _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UserConsumerJob started in background.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation($"UserConsumerJob task started at {DateTime.Now}");

            await Task.Delay(5000, stoppingToken); // Simulating some work
        }

        _logger.LogInformation("UserConsumerJob service was cancelled.");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application has finished");
        return Task.CompletedTask;
    }
}