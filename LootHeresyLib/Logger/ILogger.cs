using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Logger
{
    public interface ILogger
    {
        void Log(string message, LoggerSeverity severity, object reference = null);
    }
}
