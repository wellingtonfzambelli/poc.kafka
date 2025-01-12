using Confluent.Kafka;

namespace poc.kafka.crosscutting.Kafka;

public interface IKafkaService : IMessageService
{
    IConsumer<Ignore, string> GetConsumer();
    string GetTopicName();
}