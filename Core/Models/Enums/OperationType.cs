using System.ComponentModel;

namespace Core.Models.Enums;

public enum OperationType
{
    [Description("INPUT")]
    INPUT = 1,
    [Description("OUTPUT")]
    OUTPUT = 2,
}