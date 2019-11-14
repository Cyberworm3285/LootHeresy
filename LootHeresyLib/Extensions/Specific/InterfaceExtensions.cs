using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Extensions.Specific
{
    public static class InterfaceExtensions
    {
        public static void LogOrThrow<TEx>(this ILogger logger, LoggerSeverity sev, string message, object objectRef = null)
            where TEx : Exception
        {
            if (logger.IsNull())
                throw typeof(TEx).Initialize<TEx>(message);

            logger.Log(message, sev, objectRef);
        }
    }
}
