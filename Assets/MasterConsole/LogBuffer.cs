namespace UnityEngine.Terminal
{
    using System.Collections.Generic;
    using UnityEngine;
    public enum TerminalLogType
    {
        Error     = LogType.Error,
        Assert    = LogType.Assert,
        Warning   = LogType.Warning,
        Message   = LogType.Log,
        Exception = LogType.Exception,
        Input,
        ShellMessage
    }

    public struct LogItem
    {
        public TerminalLogType Type;
        public string Message;
        public string StackTrace;
    }

    public class LogBuffer
    {
        private readonly int maxItems;

        public List<LogItem> Logs { get; } = new();

        public LogBuffer(int maxItems)
            => this.maxItems = maxItems;

        public void HandleLog(string message, TerminalLogType type)
            => this.HandleLog(message, "", type);

        public void HandleLog(string message, string stackTrace, TerminalLogType type) 
        {
            var log = new LogItem {
                Message = message,
                StackTrace = stackTrace,
                Type = type
            };

            this.Logs.Add(log);

            if (this.Logs.Count > this.maxItems) 
                this.Logs.RemoveAt(0);
        }

        public void Clear() 
            => this.Logs.Clear();
    }
}
