namespace EventsPublisher.Models;

public class TradeMessage : EventMessage
{
    public TradeMessage()
    {
        Exchange = "publisher-events-exchange";
        Queue = "publisher-events-queue";
        RoutingKey = "events";
        
        ExchangeDeadLeatter = $"publisher-events-exchange-dead-leattler";
        QueueDeadLeatter  = "publisher-events-queue-dead-leattler";
    }
}

public abstract class EventMessage
{
    public bool AutoDelete = false;
    public bool Durable = true;
    public bool Exclusive = false;
    public bool AutomaticRecovery = false;
    
    public string Exchange { get; protected set; } = string.Empty;
    public string ExchangeDeadLeatter { get; protected set; } = string.Empty;
    public string Queue { get; protected set; } = string.Empty;
    public string QueueDeadLeatter { get; protected set; } = string.Empty;
    public int Messagettl { get; protected set; }
    public string RoutingKey { get; protected set; } = string.Empty;
    public string RoutingKeyDeadLeatter { get; protected set; } = string.Empty;
    
    public byte[] BodyMessage { get; set; } = new byte[0];
}