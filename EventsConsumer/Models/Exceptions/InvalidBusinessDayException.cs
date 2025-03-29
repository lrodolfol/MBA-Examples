namespace EventsConsumer.Models.Exceptions;

public class InvalidBusinessDayException : Exception
{
    public InvalidBusinessDayException(string message = "The position date is not a business day") : base(message)
    {
        
    }
}