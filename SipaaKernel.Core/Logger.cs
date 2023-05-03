using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SipaaKernel.Core
{
    public class Logger
    {
        public enum LogType
        {
            Success,
            Info,
            Warning, 
            Error,
            Fatal,
        }

        public string LoggerSource { get; set; } = "SK:Core";
        private List<string> Logs;

        public Logger()
        {
            Logs = new();
        }

        public void Log(string message, LogType type, bool showInConsole = false) 
        {
            string typeStr = "";

            switch (type)
            {
                case LogType.Info:
                    typeStr = "INFO";
                    break;
                case LogType.Success:
                    typeStr = "SUCCESS";
                    break;
                case LogType.Error:
                    typeStr = "ERROR";
                    break;
                case LogType.Fatal:
                    typeStr = "FATAL";
                    break;
                case LogType.Warning:
                    typeStr = "WARN";
                    break;
            }

            string formattedLogString = $"[{LoggerSource}] [{typeStr}] {message}";

            Logs.Add(formattedLogString);
            Global.Debugger.Send(formattedLogString);
            if (showInConsole)
                System.Console.WriteLine(formattedLogString);
        }
    }
}
