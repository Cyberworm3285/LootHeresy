using System;

namespace LootHeresyLib.Logger
{
    [Flags]
    public enum LoggerSeverity
    {
        None            = 0,
        Result          = 1 << 0,
        PathInfo        = 1 << 1,
        Info            = 1 << 2,
        Availability    = 1 << 3,
        Warning         = 1 << 4,
        InputValidation = 1 << 5,
        Error           = 1 << 6,
        All             = int.MaxValue,
    }
}
