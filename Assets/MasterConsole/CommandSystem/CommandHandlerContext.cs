namespace UnityEngine.Terminal
{
    using Cysharp.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class CommandHandlerContext
    {
        private readonly ILogger<CommandHandlerContext> logger;
        private readonly Dictionary<string, CommandExecutionData> Commands = new();
        private readonly Dictionary<string, VariableExecutionData> Variables = new();


        public Dictionary<string, CommandExecutionData> GetCommands()
            => this.Commands.ToDictionary(x => x.Key, x => x.Value);
        public Dictionary<string, VariableExecutionData> GetVariables()
            => this.Variables.ToDictionary(x => x.Key, x => x.Value);

        public CommandHandlerContext(ILogger<CommandHandlerContext> logger) 
            => this.logger = logger;

        public class VariableExecutionData
        {
            private readonly Func<object> getter;
            private readonly Func<object, object> setter;
            private readonly ILogger<CommandHandlerContext> logger;
            public IConvertableFromString ArgumentCaster { get; private set; }
            public string Query { get; }
            public string Help { get; } = "";
            public string Hint { get; } = "";


            public VariableExecutionData(string query, Func<object> getter, Func<object, object> setter, ILogger<CommandHandlerContext> logger)
            {
                this.Query = query;
                this.getter = getter;
                this.setter = setter;
                this.logger = logger;
            }


            public static VariableExecutionData Create<T1>(string query, Func<T1> getter, Func<T1, object> setter, ILogger<CommandHandlerContext> logger)
            {
                var d = new VariableExecutionData(query, () => getter(), o => setter((T1)o), logger);
                Debug.Assert(ConvertableFromString<T1>.Convertor != null, $"ConvertableFromString<{typeof(T1).FullName}>.Convertor != null");
                d.ArgumentCaster = ConvertableFromString<T1>.Convertor;
                return d;
            }

            public UniTask Activate(string[] args)
            {
                if (args.Length == 0)
                {
                    try
                    {
                        var result = this.getter();
                        this.logger.LogInformation("{result}", result);
                    }
                    catch (Exception e)
                    {
                        this.logger.LogError(e, "failed execute variable getter '{cmdName}'", this.Query);
                    }
                }
                else
                {
                    var fromString = this.ArgumentCaster;
                    var value = default(object);
                    try
                    {
                        value = fromString.ConvertFromString(args.First());
                    }
                    catch (Exception e)
                    {
                        this.logger.LogError(e, "failed parse argument");
                        return UniTask.CompletedTask;
                    }

                    try
                    {
                        var output = this.setter(value);
                        this.logger.LogInformation("{result}", output);
                    }
                    catch (Exception e)
                    {
                        this.logger.LogError(e, "failed execute variable setter '{cmdName}'", this.Query);
                    }
                }
                return UniTask.CompletedTask;
            }
        }

        public class CommandExecutionData
        {
            private readonly Func<object[], object> actor;
            private readonly ILogger<CommandHandlerContext> logger;
            public List<IConvertableFromString> ArgumentCaster { get; } = new List<IConvertableFromString>();
            public string Query { get; }
            public string Help { get; } = "";
            public string Hint { get; } = "";

            private CommandExecutionData(string query, Func<object[], object> actor, ILogger<CommandHandlerContext> logger)
            {
                this.actor = actor;
                this.logger = logger;
                this.Query = query;
            }


            public static CommandExecutionData Create<T1>(string query, Func<T1, object> data, ILogger<CommandHandlerContext> logger)
            {
                var d = new CommandExecutionData(query, (x) => {
                    Debug.Assert(x[0] is T1, "x[0] is T1");
                    return data((T1)x[0]);
                }, logger);
                Debug.Assert(ConvertableFromString<T1>.Convertor != null, $"ConvertableFromString<{typeof(T1).FullName}>.Convertor != null");
                d.ArgumentCaster.Add(ConvertableFromString<T1>.Convertor);
                return d;
            }

            public static CommandExecutionData Create(string query, Func<object> data, ILogger<CommandHandlerContext> logger)
                => new(query, (_) => data(), logger);

            public static CommandExecutionData Create(string query, Func<UniTask> data, ILogger<CommandHandlerContext> logger)
                => new(query, (_) => {
                    data().Forget();
                    return null;
                }, logger);

            public static CommandExecutionData Create(string query, Action data, ILogger<CommandHandlerContext> logger)
                => new(query, (_) => {
                    data();
                    return null;
                }, logger);

            public static CommandExecutionData Create<T1, T2>(string query, Func<T1, T2, object> data, ILogger<CommandHandlerContext> logger)
            {
                var d = new CommandExecutionData(query, (x) => {
                    Debug.Assert(x[0] is T1, "x[0] is T1");
                    Debug.Assert(x[1] is T2, "x[0] is T1");
                    return data((T1)x[0], (T2)x[1]);
                }, logger);
                Debug.Assert(ConvertableFromString<T1>.Convertor != null, $"ConvertableFromString<{typeof(T1).FullName}>.Convertor != null");
                Debug.Assert(ConvertableFromString<T2>.Convertor != null, $"ConvertableFromString<{typeof(T2).FullName}>.Convertor != null");
                d.ArgumentCaster.Add(ConvertableFromString<T1>.Convertor);
                d.ArgumentCaster.Add(ConvertableFromString<T2>.Convertor);
                return d;
            }

            public static CommandExecutionData Create<T1, T2, T3>(string query, Func<T1, T2, T3, object> data, ILogger<CommandHandlerContext> logger)
            {
                var d = new CommandExecutionData(query, (x) => {
                    Debug.Assert(x[0] is T1, "x[0] is T1");
                    Debug.Assert(x[1] is T2, "x[1] is T2");
                    Debug.Assert(x[2] is T3, "x[2] is T3");
                    return data((T1)x[0], (T2)x[1], (T3)x[2]);
                }, logger);
                Debug.Assert(ConvertableFromString<T1>.Convertor != null, $"ConvertableFromString<{typeof(T1).FullName}>.Convertor != null");
                Debug.Assert(ConvertableFromString<T2>.Convertor != null, $"ConvertableFromString<{typeof(T2).FullName}>.Convertor != null");
                Debug.Assert(ConvertableFromString<T3>.Convertor != null, $"ConvertableFromString<{typeof(T3).FullName}>.Convertor != null");
                d.ArgumentCaster.Add(ConvertableFromString<T1>.Convertor);
                d.ArgumentCaster.Add(ConvertableFromString<T2>.Convertor);
                d.ArgumentCaster.Add(ConvertableFromString<T3>.Convertor);
                return d;
            }



            public UniTask Activate(string[] args)
            {
                var argsObjects = new object[args.Length];

                Debug.Assert(args.Length == this.ArgumentCaster.Count, "args.Length == ArgumentCaster.Count");

                for (var i = 0; i < this.ArgumentCaster.Count; i++)
                {
                    var fromString = this.ArgumentCaster[i];
                    var arg = args[i];

                    try
                    {
                        argsObjects[i] = fromString.ConvertFromString(arg);
                    }
                    catch (Exception e)
                    {
                        this.logger.LogError(e, "failed parse argument");
                        return UniTask.CompletedTask;
                    }
                }

                try
                {
                    var result = this.actor(argsObjects);
                    if (result is not null)
                        this.logger.LogInformation("{result}", result);
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, "failed execute command '{cmdName}'", this.Query);
                }
                return UniTask.CompletedTask;
            }
        }


        public CommandHandlerContext Variable<T1>(string query, Func<T1> getter, Func<T1, object> setter)
        {
            if (typeof(T1).IsEnum && ConvertableFromString<T1>.Convertor is null)
                ConvertableFromString<T1>.Convertor = new EnumConverter<T1>();
            this.Variables.Add(query, VariableExecutionData.Create(query, getter, setter, this.logger));
            return this;
        }


        public CommandHandlerContext Variable<T1>(string query, Func<T1> getter) =>
            this.Variable(query, getter, _ => {
                this.logger.LogWarning($"'{query}' is a readonly property.");
                return _;
            });


        public CommandHandlerContext Command<T1>(string query, Func<T1, object> executor)
        {
            if (typeof(T1).IsEnum && ConvertableFromString<T1>.Convertor is null)
                ConvertableFromString<T1>.Convertor = new EnumConverter<T1>();

            this.Commands.Add(query, CommandExecutionData.Create(query, executor, this.logger));
            return this;
        }

        public CommandHandlerContext Command(string query, Func<object> executor)
        {
            this.Commands.Add(query, CommandExecutionData.Create(query, executor, this.logger));
            return this;
        }

        public CommandHandlerContext Command(string query, Func<UniTask> executor)
        {
            this.Commands.Add(query, CommandExecutionData.Create(query, executor, this.logger));
            return this;
        }

        public CommandHandlerContext Command<T1>(string query, Action<T1> executor) => this.Command<T1>(query, (x) => 
        {
            executor(x);
            return null;
        });

        public CommandHandlerContext Command<T1, T2>(string query, Func<T1, T2, object> executor)
        {
            this.Commands.Add(query, CommandExecutionData.Create(query, executor, this.logger));
            return this;
        }

        public CommandHandlerContext Command<T1, T2, T3>(string query, Func<T1, T2, T3, object> executor)
        {
            this.Commands.Add(query, CommandExecutionData.Create(query, executor, this.logger));
            return this;
        }

        public void Bind<T>(IConvertableFromString<T> converter)
            => ConvertableFromString<T>.Convertor = converter;
    }
}
