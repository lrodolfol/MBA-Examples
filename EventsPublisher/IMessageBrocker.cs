﻿namespace EventsPublisher;

public interface IMessageBrocker
{
    public Task PublishAsync();
    public Task<bool> PreparePublish(Object message);
}