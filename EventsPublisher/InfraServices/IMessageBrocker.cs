﻿namespace EventsPublisher.InfraServices;

public interface IMessageBrocker
{
    public Task PublishAsync();
    public Task<bool> PreparePublish(string messageJsonFormated);
}