using Confluent.Kafka;
using poc.kafka.producer.Domain;
using System.Text.Json;

namespace poc.kafka.producer.Kafka;

public sealed class UserProducer : IUserProducer
{
    private readonly ProducerConfig _producerConfig;
    private readonly ILogger<UserProducer> _logger;
    private readonly string _topicName;

    public UserProducer(IConfiguration configuration, ILogger<UserProducer> logger)
    {
        _logger = logger;

        _topicName = configuration.GetSection("kafkaConfig").GetSection("TopicName").Value;

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = configuration.GetSection("kafkaConfig").GetSection("BootstrapServer").Value,
            AllowAutoCreateTopics = true,
            Acks = Acks.All
        };
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
}