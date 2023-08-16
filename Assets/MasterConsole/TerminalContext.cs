namespace UnityEngine.Terminal
{
    using Microsoft.Extensions.Logging;

    public class TerminalContext
    {
        public TerminalContext(
            LogBuffer buffer, CommandShell shell, 
            CommandHistory history, CommandAutocomplete autocomplete,
            ILogger<Terminal> logger, TerminalSettings settings)
        {
            Buffer = buffer;
            Shell = shell;
            History = history;
            Autocomplete = autocomplete;
            Logger = logger;
            Settings = settings;
        }

        public LogBuffer Buffer { get; }
        public CommandShell Shell { get; }
        public CommandHistory History { get; }
        public CommandAutocomplete Autocomplete { get; }
        public ILogger<Terminal> Logger { get; }
        public TerminalSettings Settings { get; }
    }
}