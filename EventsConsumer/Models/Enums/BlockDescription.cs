using System.ComponentModel;

namespace EventsConsumer.Models.Enums;

public enum BlockDescription
{
    [Description("The amount of operations of the day is greater than the amount of positions of the day")]
    DailyOperationGreaterThanDailyPosition,
    
    [Description("The amount of operations of the day is less than the amount of positions of the day")]
    DailyOperationLessThanDailyPosition,
    
    [Description("The position of the day was not found")]
    DailyPositionNotFound
}