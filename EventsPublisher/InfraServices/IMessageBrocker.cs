namespace EventsPublisher.InfraServices;

public interface IMessageBrocker : IDisposable
{
    public Task PublishAsync();
    public Task<bool> PreparePublish(string messageJsonFormated);
}