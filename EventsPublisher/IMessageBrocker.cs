namespace EventsPublisher;

public interface IMessageBrocker
{
    public Task PublishAsync(byte[] bodyJsonMessage);
}