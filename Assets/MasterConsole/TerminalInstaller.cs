namespace UnityEngine.Terminal
{
    using VContainer;
    using Microsoft.Extensions.Logging;
    using System;


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
}
