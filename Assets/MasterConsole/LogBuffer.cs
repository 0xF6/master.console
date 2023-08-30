namespace UnityEngine.Terminal
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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

    public interface ITerminalBuffer
    {
        void HandleLog(string category, string message, Exception exception, LogLevel level);
        void HandleLog(string category, string message, LogLevel level);
        void Clear();

        IReadOnlyCollection<LogItem> GetLogItems();
    }


    public struct LogItem
    {
        public string FormattedPayload;
        public readonly string CategoryName;
        public readonly DateTimeOffset Timestamp;
        public readonly LogLevel LogLevel;
        public readonly Exception? Exception;

        public LogItem(
            string categoryName,
            string formattedPayload,
            DateTimeOffset timestamp,
            LogLevel logLevel,
            Exception? exception)
        {
            this.FormattedPayload = formattedPayload;
            this.CategoryName = categoryName;
            this.Timestamp = timestamp;
            this.LogLevel = logLevel;
            this.Exception = exception;
        }
    }

    public class LogBuffer : ITerminalBuffer
    {
        private readonly int maxItems;

        public List<LogItem> Logs { get; } = new();

        public LogBuffer(int maxItems)
            => this.maxItems = maxItems;

        public void HandleLog(string category, string message, LogLevel level)
            => this.HandleLog(category, message, null, level);

        public void HandleLog(string category, string message, Exception exception, LogLevel level)
        {
            var log = new LogItem(category, message, DateTimeOffset.Now, level, exception);
            
            this.Logs.Add(log);

            if (this.Logs.Count > this.maxItems) 
                this.Logs.RemoveAt(0);
        }

        public void Clear() 
            => this.Logs.Clear();

        public IReadOnlyCollection<LogItem> GetLogItems() => Logs.AsReadOnly();
    }
}
