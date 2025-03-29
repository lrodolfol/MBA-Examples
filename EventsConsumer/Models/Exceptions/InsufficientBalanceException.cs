namespace EventsConsumer.Models.Exceptions;

public class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException(string message) : base(message)
    {
        
    }
}