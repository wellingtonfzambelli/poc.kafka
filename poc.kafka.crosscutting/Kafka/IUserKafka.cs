using Confluent.Kafka;
using poc.kafka.crosscutting.Domain;

namespace poc.kafka.crosscutting.Kafka;

public interface IUserKafka
{
    Task ProduceAsync(User user, CancellationToken cancellationToken);
    IConsumer<Ignore, string> GetConsumer();
    string GetTopicName();
}