namespace UnityEngine.Terminal
{
    using Microsoft.Extensions.Logging;
    using System;

    #if VCONTINAER_DEFINED

    public static class TerminalInstaller
    {
        public static IContainerBuilder UseTerminal(this IContainerBuilder builder, IWithTerminalFeatureSettings settings)
        {
            builder.RegisterInstance(settings);
            builder.RegisterInstance(settings.TerminalSettings);
            builder.RegisterInstance(new LogBuffer(settings.TerminalSettings.BufferSize));
            builder.Register<CommandShell>(Lifetime.Singleton);
            builder.Register<CommandHistory>(Lifetime.Singleton);
            builder.Register<CommandAutocomplete>(Lifetime.Singleton);
            builder.Register<DynamicCommand>(Lifetime.Singleton).As<IDynamicCommand>();
            builder.UseLogger<IDynamicCommand>();
            builder.Register(x => {
                var go = Object.FindFirstObjectByType<Camera>().gameObject;
                
                var term = go.AddComponent<Terminal>();
                x.Inject(term);
                return term;
            }, Lifetime.Singleton);
            builder.UseLogger<Terminal>();
            builder.WarmUp<Terminal>();
            return builder;
        }
    }
    #endif
    public interface IDynamicCommand
    {
        void Create(Action<CommandHandlerContext> setup);
    }

    public class DynamicCommand : IDynamicCommand
    {
        private readonly ILogger<CommandHandlerContext> loggerForCtx;
        private readonly ILogger<IDynamicCommand> logger;
        private readonly CommandShell shell;

        public DynamicCommand(ILogger<CommandHandlerContext> loggerForCtx, ILogger<IDynamicCommand> logger, CommandShell shell)
        {
            this.loggerForCtx = loggerForCtx;
            this.logger = logger;
            this.shell = shell;
        }

        public void Create(Action<CommandHandlerContext> setup)
        {
            var ctx = new CommandHandlerContext(this.loggerForCtx);
            try
            {
                setup(ctx);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed dynamic registration command");
            }

            foreach (var data in ctx.GetCommands())
            {
                shell.AddDynamicCommand(data.Key, data.Value);
                logger.LogInformation("Success registered '{cmd}' dynamic command!", data.Key);
            }
        }
    }

    public interface IWithTerminalFeatureSettings
    {
        TerminalSettings TerminalSettings { get; }
    }
}
