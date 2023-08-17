using Microsoft.Extensions.Logging;
using System;
using UnityEngine;
using UnityEngine.Terminal;
using VContainer;
using VContainer.Unity;
using ILogger = Microsoft.Extensions.Logging.ILogger;

public class EntryPoint : LifetimeScope
{
    public GameSettings Settings;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.UseTerminal(Settings, x => {
            x.UseGameObjectCommands();
        });
        builder.Register<ILoggerFactory>(x => {
            return new FooLoggerFactory();
        }, Lifetime.Singleton);
        base.Configure(builder);
    }
}


public class FooLoggerFactory : ILoggerFactory
{
    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new UnityLogger();
    }

    public void AddProvider(ILoggerProvider provider)
    {
        throw new System.NotImplementedException();
    }
}

public class UnityLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        switch (logLevel)
        {
            case LogLevel.Error or LogLevel.Critical:
                Debug.LogError(formatter(state, exception));
                return;
            case LogLevel.Warning:
                Debug.LogWarning(formatter(state, exception));
                return;
            default:
                Debug.Log(formatter(state, exception));
                break;
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => throw new Exception();
}
