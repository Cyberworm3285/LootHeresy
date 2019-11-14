using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Extensions.Specific
{
    public static class InterfaceExtensions
    {
        public static void LogAndThrow<TEx>(this ILogger logger, LoggerSeverity sev, string message, object objectRef = null)
            where TEx : Exception
        {
            if (!sev.HasFlag(LoggerSeverity.Error))
                sev |= LoggerSeverity.Error;

            logger?.Log(message, sev, objectRef);
            throw typeof(TEx).Initialize<TEx>(message);
        }
    }
}
