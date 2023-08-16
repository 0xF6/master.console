namespace UnityEngine.Terminal
{
    using VContainer;

    public interface ICameraResolver
    {
        Camera GetMainCamera(IObjectResolver resolver);
    }

    public static class CameraResolver
    {
        public static ICameraResolver Resolver = new CameraResolverFromContainer();
        
        public class MainCameraResolver : ICameraResolver
        {
            public Camera GetMainCamera(IObjectResolver resolver) => Camera.main;
        }

        public class CameraResolverFromContainer : ICameraResolver
        {
            public static ICameraResolver FallbackResolver = new MainCameraResolver();

            public Camera GetMainCamera(IObjectResolver resolver)
            {
                return resolver.TryResolve<Camera>(out var result) ?
                    result : FallbackResolver.GetMainCamera(resolver);
            }
        }
    }
}