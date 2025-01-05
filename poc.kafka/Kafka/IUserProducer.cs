using poc.kafka.producer.Domain;

namespace poc.kafka.producer.Kafka;

public interface IUserProducer
{
    Task ProduceAsync(User user, CancellationToken cancellationToken);
}