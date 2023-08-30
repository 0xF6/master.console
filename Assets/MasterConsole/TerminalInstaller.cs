namespace UnityEngine.Terminal
{
    using VContainer;
    using System;


    public static class TerminalInstaller
    {
        public static IContainerBuilder UseTerminal(this IContainerBuilder builder, IWithTerminalFeatureSettings settings, Action<ICommandBuilder> cmdBuilder = null, Func<ITerminalBuffer> customBuffer = null)
        {
            builder.UseCommands(x => {
                x.Use<DefaultTerminalCommands>();
                cmdBuilder?.Invoke(x);
            });

            builder.RegisterInstance(settings);
            builder.RegisterInstance(settings.TerminalSettings);
            if (customBuffer is null)
                builder.RegisterInstance(new LogBuffer(settings.TerminalSettings.BufferSize)).As<ITerminalBuffer>();
            else
            {
                var buffer = customBuffer();

                if (buffer is null)
                    builder.RegisterInstance(new LogBuffer(settings.TerminalSettings.BufferSize)).As<ITerminalBuffer>();
                else
                    builder.RegisterInstance(buffer).As<ITerminalBuffer>();
            }
           
            builder.Register<CommandShell>(Lifetime.Singleton);
            builder.Register<CommandHistory>(Lifetime.Singleton);
            builder.Register<CommandAutocomplete>(Lifetime.Singleton);
            builder.Register<DynamicCommand>(Lifetime.Singleton).As<IDynamicCommand>();
            builder.Register<TerminalContext>(Lifetime.Singleton);
            builder.UseLogger<IDynamicCommand>();
            builder.Register(x => {
                var go = CameraResolver.Resolver.GetMainCamera(x).gameObject;

                var term = go.AddComponent<Terminal>();
                x.Inject(term);
                return term;
            }, Lifetime.Singleton);
            builder.UseLogger<Terminal>();
            builder.WarmUp<Terminal>();
            return builder;
        }
    }
    
    public interface IWithTerminalFeatureSettings
    {
        TerminalSettings TerminalSettings { get; }
    }

    public class DefaultTerminalCommands : CommandSilo
    {
        private readonly LogBuffer buffer;

        public DefaultTerminalCommands(LogBuffer buffer) 
            => this.buffer = buffer;

        public override void Create(CommandHandlerContext context)
        {
#if !DISABLE_DEFAULT_COMMANDS
            context.Command("clear", this.buffer.Clear);
            context.Command("exit", Application.Quit);
#endif
        }
    }
}
