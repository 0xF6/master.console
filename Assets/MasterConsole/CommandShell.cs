namespace UnityEngine.Terminal
{
    using Cysharp.Threading.Tasks;
    using System.Linq;
    using System;
    using System.Reflection;
    using System.Collections.Generic;

    public unsafe struct CommandInfo
    {
        public delegate*<int> f;
        public Func<string[], UniTask> proc;
        public int MaxArgs;
        public int MinArgs;
        public string help;
        public string hint;
    }
    
    public class CommandShell
    {
        private readonly CommandHandlerContext handlerCtx;
        public string IssuedErrorMessage { get; private set; }

        public Dictionary<string, CommandInfo> Commands { get; } = new();

        public CommandShell(CommandHandlerContext handlerCtx) => this.handlerCtx = handlerCtx;


        /// <summary>
        /// Uses reflection to find all RegisterCommand attributes
        /// and adds them to the commands dictionary.
        /// </summary>
        public void RegisterCommands()
        {
            var rejectedCommands = new Dictionary<string, CommandInfo>();
            
            foreach (var (commandName, data) in handlerCtx.GetCommands())
                this.AddCommand(commandName, x => data.Activate(x), data.ArgumentCaster.Count,
                    data.ArgumentCaster.Count, data.Help, data.Hint);

            foreach (var (commandName, data) in handlerCtx.GetVariables())
                this.AddCommand(commandName, x => data.Activate(x), 0, 1, data.Help, data.Hint);

            this.HandleRejectedCommands(rejectedCommands);
        }

        public void AddDynamicCommand(string name, CommandHandlerContext.CommandExecutionData data) 
            => this.AddCommand(name, data.Activate, 0, 1, data.Help, data.Hint);

        /// <summary>
        /// Parses an input line into a command and runs that command.
        /// </summary>
        public void RunCommand(string line)
        {
            string remaining = line;
            this.IssuedErrorMessage = null;
            var arguments = new List<string>();
            while (remaining != "")
            {
                var argument = this.EatArgument(ref remaining);

                if (argument.Length == 0)
                    continue;

                arguments.AddRange(argument);
            }

            if (arguments.Count == 0)
            {
                // Nothing to run
                return;
            }

            var commandName = arguments[0].ToUpperInvariant();
            arguments.RemoveAt(0); // Remove command name from arguments

            if (commandName.StartsWith('-') || commandName.StartsWith("+"))
            {
                arguments.Add(commandName.StartsWith("+") ? "UP" : "DOWN");
                commandName = commandName.Trim('-', '+');
            }

            if (!this.Commands.ContainsKey(commandName))
            {
                this.IssueErrorMessage("Command '{0}' could not be found", commandName.ToLowerInvariant());
                return;
            }

            this.RunCommand(commandName, arguments.ToArray());
        }

        public void RunCommand(string commandName, string[] args)
        {
            var command = this.Commands[commandName];
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

                this.IssueErrorMessage(
                    "{0} requires {1} {2} argument{3}",
                    commandName,
                    errorMessage,
                    requiredArg,
                    pluralFix
                );

                if (!string.IsNullOrEmpty(command.hint)) 
                    this.IssuedErrorMessage += $"\n    -> Usage: {command.hint}";

                return;
            }

            command.proc(args).Forget();
        }

        public void AddCommand(string name, CommandInfo info)
        {
            name = name.ToUpper();

            if (this.Commands.ContainsKey(name))
            {
                this.IssueErrorMessage("Command {0} is already defined.", name);
                return;
            }

            this.Commands.Add(name, info);
        }

        public void AddCommand(string name, Func<string[], UniTask> proc, int minArgs = 0, int maxArgs = -1, string help = "", string hint = null)
        {
            var info = new CommandInfo()
            {
                proc = proc,
                MinArgs = minArgs,
                MaxArgs = maxArgs,
                help = help,
                hint = hint
            };

            this.AddCommand(name, info);
        }
        
        public void IssueErrorMessage(string format, params object[] message)
        {
            this.IssuedErrorMessage = string.Format(format, message);
        }

        private string InferCommandName(string methodName)
        {
            string commandName;
            int index = methodName.IndexOf("COMMAND", StringComparison.CurrentCultureIgnoreCase);

            // Method is prefixed, suffixed with, or contains "COMMAND".
            commandName = index >= 0 ? methodName.Remove(index, 7) : methodName;

            return commandName;
        }

        private string InferFrontCommandName(string method_name)
        {
            int index = method_name.IndexOf("FRONT", StringComparison.CurrentCultureIgnoreCase);
            return index >= 0 ? method_name.Remove(index, 5) : null;
        }

        private void HandleRejectedCommands(Dictionary<string, CommandInfo> rejectedCommands)
        {
            foreach (var command in rejectedCommands)
            {
                if (this.Commands.ContainsKey(command.Key))
                {
                    this.Commands[command.Key] = new CommandInfo()
                    {
                        proc = this.Commands[command.Key].proc,
                        MinArgs = command.Value.MinArgs,
                        MaxArgs = command.Value.MaxArgs,
                        help = command.Value.help
                    };
                }
                else
                {
                    this.IssueErrorMessage("{0} is missing a front command.", command);
                }
            }
        }

        private CommandInfo CommandFromParamInfo(ParameterInfo[] parameters, string help)
        {
            var optionalArgs = parameters.Count(param => param.IsOptional);

            return new CommandInfo()
            {
                proc = null,
                MinArgs = parameters.Length - optionalArgs,
                MaxArgs = parameters.Length,
                help = help
            };
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
