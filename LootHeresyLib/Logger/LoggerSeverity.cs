using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Logger
{
    [Flags]
    public enum LoggerSeverity
    {
        None,
        Result          = 2 << 0,
        PathInfo        = 2 << 1,
        Info            = 2 << 2,
        Warning         = 2 << 3,
        InputValidation = 2 << 4,
        Error           = 2 << 5,
        All             = int.MaxValue,
    }
}
