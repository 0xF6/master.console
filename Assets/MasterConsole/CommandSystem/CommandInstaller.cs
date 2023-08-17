namespace UnityEngine.Terminal
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using VContainer;

    public static class CommandInstaller
    {
        public static IContainerBuilder UseCommands(this IContainerBuilder builder, Action<ICommandBuilder> cmdBuilder)
        {
            builder.UseLogger<CommandHandlerContext>();

            var s = new CommandSilosContext();

            cmdBuilder(s);

            foreach (var action in s.DelayedAdditionalRegistration) action(builder);
            foreach (var type in s.Types) builder.Register(type, Lifetime.Singleton);


            builder.Register(x => {
                var a = new CommandHandlerContext(x.Resolve<ILogger<CommandHandlerContext>>());
                foreach (var type in s.Types)
                {
                    if (x.Resolve(type) is CommandSilo silo)
                        silo.Create(a);
                    else
                        Debug.LogError($"Failed resolve silo");
                }
                return a;
            }, Lifetime.Singleton);
            return null;
        }
        
        private class CommandSilosContext : ICommandBuilder
        {
            public readonly List<Type> Types = new();
            public readonly List<Action<IContainerBuilder>> DelayedAdditionalRegistration = new();

            public ICommandBuilder Use<T>() where T : CommandSilo
            {
                this.Types.Add(typeof(T));
                this.DelayedAdditionalRegistration.Add(x => x.UseLogger<T>());
                return this;
            }
        }
    }
}
