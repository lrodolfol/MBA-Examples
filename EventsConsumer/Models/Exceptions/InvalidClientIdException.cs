namespace EventsConsumer.Models.Exceptions;

public class InvalidClientIdException : Exception
{
    public InvalidClientIdException(string message = "The position date is not a business day") : base(message)
    {
        
    }
}