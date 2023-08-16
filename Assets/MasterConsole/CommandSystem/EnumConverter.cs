namespace UnityEngine.Terminal
{
    using System;

    public class EnumConverter<T> : IConvertableFromString<T>
    {
        public T ConvertFromString(string str)
        {
            if (ulong.TryParse(str, out var index))
            {
                var enumData = EnumCache<T>.Value;
                var safeIndex = Math.Clamp(index, enumData.UnsignedMinValue, enumData.UnsignedMaxValue);
                var safeValue = Convert.ChangeType(safeIndex, enumData.UnderlyingType);
                return (T)Convert.ChangeType(safeValue, enumData.SelfType);
            }

            if (Enum.TryParse(typeof(T), str, out var value))
                return (T)value;

            throw new FormatException($"failed convert '{str}' to {typeof(T).FullName}");
        }

        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }
}