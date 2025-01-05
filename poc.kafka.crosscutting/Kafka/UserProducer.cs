using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using poc.kafka.crosscutting.Domain;
using System.Text.Json;

namespace poc.kafka.crosscutting.Kafka;

public sealed class UserProducer : IUserProducer
{
    private readonly ILogger<UserProducer> _logger;
    private readonly ProducerConfig _producerConfig;
    private readonly ConsumerConfig _consumerConfig;
    private readonly string _topicName;

    public UserProducer
    (
        string topicName,
        string bootstrapServers,
        ILogger<UserProducer> logger
    )
    {
        _topicName = topicName;

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            AllowAutoCreateTopics = true,
            Acks = Acks.All
        };

        _logger = logger;
    }

    public async Task ProduceAsync(User user, CancellationToken cancellationToken)
    {
        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();

        try
        {
            var deliveryResult = await producer.ProduceAsync
                (
                    topic: _topicName,
                    new Message<Null, string>
                    {
                        Value = JsonSerializer.Serialize(user)
                    },
                    cancellationToken
                );

            _logger.LogInformation($"Delivered message to {deliveryResult.Value}, Offset: {deliveryResult.Offset}");
        }
        catch (ProduceException<Null, string> e)
        {
            _logger.LogError($"Delivery failed: {e.Error.Reason}");
        }

        producer.Flush(cancellationToken);
    }

    public async Task<User> ConsumeAsync(CancellationToken cancellationToken)
    {
        return new User(Guid.NewGuid(), "we", "we@gmail.com");
    }
}