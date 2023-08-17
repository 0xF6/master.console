namespace UnityEngine.Terminal
{
    using System;
    using System.Linq;

    internal static class EnumCache<T>
    {
        public class Cache<X>
        {
            public Array Values { get; }
            public object[] UnderlyingValues { get; }
            public int TotalValues { get; }
            public Type UnderlyingType { get; }
            public Type SelfType => typeof(X);

            public long MaxValue =>
                this.UnderlyingValues.Select(x => (long)Convert.ChangeType(x, typeof(long))).Max(x => x);
            public long MinValue =>
                this.UnderlyingValues.Select(x => (long)Convert.ChangeType(x, typeof(long))).Min(x => x);

            public ulong UnsignedMaxValue =>
                this.UnderlyingValues.Select(x => (ulong)Convert.ChangeType(x, typeof(ulong))).Max(x => x);
            public ulong UnsignedMinValue =>
                this.UnderlyingValues.Select(x => (ulong)Convert.ChangeType(x, typeof(ulong))).Min(x => x);

            public Cache()
            {
                this.Values = Enum.GetValues(typeof(X));
                this.TotalValues = Enum.GetValues(typeof(X)).Length;
                this.UnderlyingType = Enum.GetUnderlyingType(typeof(X));

                this.UnderlyingValues = Enumerable.Range(0, this.TotalValues).Select(x => this.Values.GetValue(x))
                    .Select(x => Convert.ChangeType(x, this.UnderlyingType)).ToArray();
            }
        }

        private static Cache<T> _cache;
        public static Cache<T> Value => _cache ??= new Cache<T>();
    }
}
