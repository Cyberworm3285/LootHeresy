using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Logger
{
    public class ConsoleLogger : ILogger
    {
        public bool ThrowOnError { get; set; }
        public LoggerSeverity Take { get; set; }

        private Dictionary<LoggerSeverity, ConsoleColor> _colorTable = new Dictionary<LoggerSeverity, ConsoleColor>
        {
            { LoggerSeverity.Info,              ConsoleColor.Green      },
            { LoggerSeverity.Warning,           ConsoleColor.DarkYellow },
            { LoggerSeverity.Error,             ConsoleColor.Red        },
            { LoggerSeverity.Result,            ConsoleColor.Blue       },
            { LoggerSeverity.PathInfo,          ConsoleColor.Cyan       },
            { LoggerSeverity.InputValidation,   ConsoleColor.DarkRed    },
        };
        public void Log(string message, LoggerSeverity severity, object reference = null)
        {
            if (ThrowOnError && severity.HasFlag(LoggerSeverity.Error))
                throw new Exception("Exception thrown alla");

            LoggerSeverity getMostSevere(int s)
            {
                int i = 0;
                while (s != 0)
                {
                    s /= 2;
                    i++;
                }

                return (LoggerSeverity)(1 << (i - 1));
            }

            int sev = ((int)severity & (int)Take);
            if (sev == 0)
                return;

            Console.ForegroundColor = _colorTable[getMostSevere(sev)];
            Console.WriteLine($"[{severity.ToString()}][{message}]");
            if (severity == LoggerSeverity.Error && ThrowOnError)
                throw new Exception(message);
            Console.ResetColor();
        }
    }
}
