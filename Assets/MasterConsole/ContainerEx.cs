namespace UnityEngine.Terminal
{
    using VContainer;
    using VContainer.Unity;
    using Microsoft.Extensions.Logging;
    using System.Runtime.CompilerServices;

    internal static class ContainerEx
    {
        public static IContainerBuilder UseLogger<T>(this IContainerBuilder builder)
        {
            builder.Register(x => x.Resolve<ILoggerFactory>().CreateLogger<T>(), Lifetime.Singleton);
            return builder;
        }

        public static bool TryResolve<T>(this IObjectResolver resolver, out T result)
        {
            try
            {
                result = resolver.Resolve<T>();
                return true;
            }
            catch (VContainerException)
            {
                result = default(T);
                return false;
            }
        }

        public static IContainerBuilder WarmUp<T>(this IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ForceCreateAction<T>>();
            return builder;
        }
        internal class ForceCreateAction<X> : IStartable
        {
            private readonly X _;

            [MethodImpl(MethodImplOptions.NoOptimization)]
            public ForceCreateAction(X unused) => _ = unused;

            public void Start() { }
        }
    }
}