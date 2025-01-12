namespace poc.kafka.crosscutting.Kafka;

public interface IMessageService
{
    Task ProduceAsync(string message, CancellationToken ct);
}