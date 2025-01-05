using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using poc.kafka.crosscutting.Domain;
using System.Text.Json;

namespace poc.kafka.crosscutting.Kafka;

public sealed class UserKafka : IUserKafka
{
    private readonly ProducerConfig _producerConfig;

    private readonly ConsumerConfig _consumerConfig;
    private readonly IConsumer<Ignore, string> _consumer;

    private readonly string _topicName;
    private readonly ILogger<UserKafka> _logger;

    public UserKafka
    (
        string topicName,
        string bootstrapServers,
        string groupId,
        ILogger<UserKafka> logger
    )
    {
        _topicName = topicName;

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            AllowAutoCreateTopics = true,
            Acks = Acks.All
        };

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest // removes from the topic
        };
        _consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();

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
        _consumer.Subscribe(_topicName);


        if (_consumer.Consume(cancellationToken)
            is var result && result?.Message?.Value is null)
            return null;

        _logger.LogInformation($"Message: {result.Message.Value}");
        User user = JsonSerializer.Deserialize<User>(result.Message.Value);

        _consumer.Commit(result);

        return user;



    }
}