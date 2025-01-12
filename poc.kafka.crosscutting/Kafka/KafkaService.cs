using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using poc.kafka.crosscutting.Settings;

namespace poc.kafka.crosscutting.Kafka;

public sealed class KafkaService : IKafkaService
{
    private readonly ProducerConfig _producerConfig;
    private readonly ConsumerConfig _consumerConfig;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<KafkaService> _logger;

    public KafkaService
    (
        KafkaSettings kafkaSettings,
        ILogger<KafkaService> logger
    )
    {
        _kafkaSettings = kafkaSettings;

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServer,
            AllowAutoCreateTopics = true,
            Acks = Acks.All
        };

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServer,
            GroupId = _kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest // removes from the topic
        };
        _consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();

        _logger = logger;
    }

    public async Task ProduceAsync(string message, CancellationToken cancellationToken)
    {
        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();

        try
        {
            var deliveryResult = await producer.ProduceAsync
                (
                    topic: _kafkaSettings.TopicName,
                    new Message<Null, string>
                    {
                        Value = message
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

    public IConsumer<Ignore, string> GetConsumer() =>
        _consumer;

    public string GetTopicName() =>
        _kafkaSettings.TopicName;
}