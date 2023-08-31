namespace UnityEngine.Terminal
{
    using Cysharp.Threading.Tasks;
    using System.Linq;
    using System;
    using System.Reflection;
    using System.Collections.Generic;

    public struct CommandInfo
    {
        public Func<string[], UniTask> Procedure;
        public int MaxArgs;
        public int MinArgs;
        public string Help;
        public string Hint;
    }
    
    public class CommandShell
    {
        private readonly CommandHandlerContext handlerCtx;

        public Dictionary<string, CommandInfo> Commands { get; } = new();

        public CommandShell(CommandHandlerContext handlerCtx) => this.handlerCtx = handlerCtx;


        /// <summary>
        /// Uses reflection to find all RegisterCommand attributes
        /// and adds them to the commands dictionary.
        /// </summary>
        public void RegisterCommands()
        {
            foreach (var (commandName, data) in handlerCtx.GetCommands())
                this.AddCommand(commandName, x => data.Activate(x), data.ArgumentCaster.Count,
                    data.ArgumentCaster.Count, data.Help, data.Hint);

            foreach (var (commandName, data) in handlerCtx.GetVariables())
                this.AddCommand(commandName, x => data.Activate(x), 0, 1, data.Help, data.Hint);
        }

        public void AddDynamicCommand(string name, CommandHandlerContext.CommandExecutionData data) 
            => this.AddCommand(name, data.Activate, 0, 1, data.Help, data.Hint);

        /// <summary>
        /// Parses an input line into a command and runs that command.
        /// </summary>
        public bool RunCommand(string line, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(line))
                return false;
            
            var remaining = line;
            var arguments = new List<string>();
            while (remaining != "")
            {
                var argument = this.EatArgument(ref remaining);

                if (argument.Length == 0)
                    continue;

                arguments.AddRange(argument);
            }

            if (arguments.Count == 0)
                return false;

            var commandName = arguments[0].ToLowerInvariant();
            arguments.RemoveAt(0); // Remove command name from arguments

            if (commandName.StartsWith('-') || commandName.StartsWith("+"))
            {
                arguments.Add(commandName.StartsWith("+") ? "UP" : "DOWN");
                commandName = commandName.Trim('-', '+');
            }

            if (!this.Commands.ContainsKey(commandName))
            {
                error = $"Command '{commandName}' could not be found";
                return false;
            }

            return this.RunCommand(commandName, arguments.ToArray(), out error);
        }

        public bool RunCommand(string commandName, string[] args, out string error)
        {
            error = null;
            var command = this.Commands[commandName.ToLowerInvariant()];
            var argCount = args.Length;
            string errorMessage = null;
            var requiredArg = 0;

            if (argCount < command.MinArgs)
            {
                errorMessage = command.MinArgs == command.MaxArgs ? "exactly" : "at least";
                requiredArg = command.MinArgs;
            }
            else if (command.MaxArgs > -1 && argCount > command.MaxArgs)
            {
                // Do not check max allowed number of arguments if it is -1
                errorMessage = command.MinArgs == command.MaxArgs ? "exactly" : "at most";
                requiredArg = command.MaxArgs;
            }

            if (errorMessage != null)
            {
                var pluralFix = requiredArg == 1 ? "" : "s";

                error = $"{commandName} requires {errorMessage} {requiredArg} argument {pluralFix}";

                if (!string.IsNullOrEmpty(command.Hint))
                    error += $"\n    -> Usage: {command.Hint}";

                return false;
            }

            command.Procedure(args).Forget();
            return true;
        }

        public void AddCommand(string name, CommandInfo info)
        {
            name = name.ToLowerInvariant();

            if (this.Commands.ContainsKey(name))
                throw new Exception($"Command {name} is already defined.");

            this.Commands.Add(name, info);
        }

        public void AddCommand(string name, Func<string[], UniTask> proc, int minArgs = 0, int maxArgs = -1, string help = "", string hint = null)
        {
            var info = new CommandInfo()
            {
                Procedure = proc,
                MinArgs = minArgs,
                MaxArgs = maxArgs,
                Help = help,
                Hint = hint
            };

            this.AddCommand(name, info);
        }

        private string[] EatArgument(ref string s)
        {
            var arg = new List<string>();
            var spaceIndex = s.IndexOf(' ');

            if (spaceIndex >= 0)
            {
                arg.Add(s[..spaceIndex]);
                s = s[(spaceIndex + 1)..]; // Remaining
            }
            else
            {
                arg.Add(s);
                s = "";
            }
            return arg.ToArray();
        }
    }
}
