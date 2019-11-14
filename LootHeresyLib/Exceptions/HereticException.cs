using System;

using LootHeresyLib.Logger;


namespace LootHeresyLib.Exceptions
{
    internal class HereticException : Exception
    {
        public LoggerSeverity Severity { get; }
        public object Cause { get; }

        public HereticException(Exception inner, string message, LoggerSeverity severity = LoggerSeverity.Error, object cause = null)
            : base(message, inner)
        {
            if (!severity.HasFlag(LoggerSeverity.Error))
                severity |= LoggerSeverity.Error;

            Cause = cause;
            Severity = severity;
        } 
    }
}
