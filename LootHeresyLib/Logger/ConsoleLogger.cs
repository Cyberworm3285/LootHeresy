using System;
using System.Collections.Generic;

namespace LootHeresyLib.Logger
{
    public class ConsoleLogger : ILogger
    {
        public LoggerSeverity Take { get; set; }

        public event Action<string, LoggerSeverity, object> OnLog;

        private Dictionary<LoggerSeverity, ConsoleColor> _colorTable = new Dictionary<LoggerSeverity, ConsoleColor>
        {
            { LoggerSeverity.Info,              ConsoleColor.Green      },
            { LoggerSeverity.Warning,           ConsoleColor.DarkYellow },
            { LoggerSeverity.Error,             ConsoleColor.Red        },
            { LoggerSeverity.Result,            ConsoleColor.Blue       },
            { LoggerSeverity.PathInfo,          ConsoleColor.Cyan       },
            { LoggerSeverity.InputValidation,   ConsoleColor.DarkRed    },
            { LoggerSeverity.Availability,      ConsoleColor.Yellow     },    
        };
        public void Log(string message, LoggerSeverity severity, object reference = null)
        {
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
            Console.ResetColor();

            OnLog?.Invoke(message, severity, reference);
        }
    }
}
