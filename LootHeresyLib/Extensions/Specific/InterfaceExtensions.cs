using System;
using LootHeresyLib.Exceptions;
using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Logger;

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
            throw new HereticException(typeof(TEx).Initialize<TEx>(message), message, sev, objectRef);
        }
    }
}
