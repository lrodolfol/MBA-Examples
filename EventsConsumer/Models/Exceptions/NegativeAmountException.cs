namespace EventsConsumer.Models.Exceptions;

public class NegativeAmountException : Exception
{
    public NegativeAmountException(string message = "Amount cannot be negative") : base(message)
    {
        
    }
}