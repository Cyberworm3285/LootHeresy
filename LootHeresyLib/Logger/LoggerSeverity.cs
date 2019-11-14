using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Logger
{
    [Flags]
    public enum LoggerSeverity
    {
        None            = 0,
        Result          = 2 << 0,
        PathInfo        = 2 << 1,
        Info            = 2 << 2,
        Availability    = 2 << 3,
        Warning         = 2 << 4,
        InputValidation = 2 << 5,
        Error           = 2 << 6,
        All             = int.MaxValue,
    }
}
