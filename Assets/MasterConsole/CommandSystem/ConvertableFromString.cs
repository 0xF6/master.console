// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace UnityEngine.Terminal
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public static class ConvertableFromString<T>
    {
        public static IConvertableFromString<T> Convertor;
    }
    
    public static class ConvertableFromStringRuntimeInit
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            ConvertableFromString<byte>.Convertor = new Convertable_Byte();
            ConvertableFromString<sbyte>.Convertor = new Convertable_SByte();
            ConvertableFromString<short>.Convertor = new Convertable_Short();
            ConvertableFromString<string>.Convertor = new Convertable_String();
            ConvertableFromString<Vector3>.Convertor = new Convertable_Vector3();
            ConvertableFromString<float>.Convertor = new Convertable_Float();
            ConvertableFromString<long>.Convertor = new Convertable_Long();
            ConvertableFromString<int>.Convertor = new Convertable_Int();
            ConvertableFromString<bool>.Convertor = new Convertable_Bool();
            ConvertableFromString<DateTimeOffset>.Convertor = new Convertable_DateTime();
            ConvertableFromString<decimal>.Convertor = new Convertable_Decimal();
            ConvertableFromString<Color>.Convertor = new Convertable_Color();
            //ConvertableFromString<AppId>.Convertor = new Convertable_AppId();
            //ConvertableFromString<SteamId>.Convertor = new Convertable_SteamId();
        }
    }


    //public class Convertable_AppId : IConvertableFromString<AppId>
    //{
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public AppId ConvertFromString(string str)
    //        => uint.Parse(str);

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    object IConvertableFromString.ConvertFromString(string str)
    //        => this.ConvertFromString(str);
    //}

    public class Convertable_Color : IConvertableFromString<Color>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color ConvertFromString(string str)
        {
            if (ColorUtility.TryParseHtmlString(str, out var c))
                return c;
            if ((str.StartsWith('[') || str.StartsWith('(')) && (str.EndsWith(']') || str.EndsWith(')')))
            {
                var (r, g, b) = str.Trim('[', ']', '(', ')').Split(',').Unpack3(float.Parse);
                return new Color(r, g, b);
            }
            throw new FormatException($"'{str}' is not valid Color");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }


    public class Convertable_Decimal : IConvertableFromString<decimal>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ConvertFromString(string str)
            => decimal.Parse(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_DateTime : IConvertableFromString<DateTimeOffset>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset ConvertFromString(string str)
            => long.TryParse(str, out var d) ? DateTimeOffset.FromUnixTimeSeconds(d).ToLocalTime() : DateTimeOffset.Parse(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_Bool : IConvertableFromString<bool>
    {
        public bool ConvertFromString(string str)
        {
            if (byte.TryParse(str, out var d) && d == 1 || d == 0)
                return d == 1;
            if (str.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                return true;
            if (str.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return false;
            if (str.Equals("enabled", StringComparison.InvariantCultureIgnoreCase))
                return true;
            if (str.Equals("disabled", StringComparison.InvariantCultureIgnoreCase))
                return false;
            if(str.Equals("up", StringComparison.InvariantCultureIgnoreCase))
                return true;
            if (str.Equals("down", StringComparison.InvariantCultureIgnoreCase))
                return false;
            throw new FormatException($"'{str}' is not valid boolean");
        }
        
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_Byte : IConvertableFromString<byte>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ConvertFromString(string str)
            => byte.Parse(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_SByte : IConvertableFromString<sbyte>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ConvertFromString(string str)
            => sbyte.Parse(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_Short : IConvertableFromString<short>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ConvertFromString(string str)
            => short.Parse(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    //public class Convertable_SteamId : IConvertableFromString<SteamId>
    //{
    //    public SteamId ConvertFromString(string str)
    //        => ulong.Parse(str);

    //    object IConvertableFromString.ConvertFromString(string str)
    //        => this.ConvertFromString(str);
    //}

    public class Convertable_String : IConvertableFromString<string>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ConvertFromString(string str)
            => str;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_Int : IConvertableFromString<int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ConvertFromString(string str)
            => int.Parse(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }
    public class Convertable_Long : IConvertableFromString<long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ConvertFromString(string str)
            => long.Parse(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_Float : IConvertableFromString<float>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ConvertFromString(string str)
            => float.Parse(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }

    public class Convertable_Vector3 : IConvertableFromString<Vector3>
    {
        public Vector3 ConvertFromString(string str)
        {
            if (str.Equals("up", StringComparison.InvariantCultureIgnoreCase))
                return Vector3.up;
            if (str.Equals("down", StringComparison.InvariantCultureIgnoreCase))
                return Vector3.down;

            var (x, y, z) = str.Trim('(', ')', '[', ']', '{', '}').Split(',').Unpack3();

            return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        }

        object IConvertableFromString.ConvertFromString(string str)
            => this.ConvertFromString(str);
    }


    public static class TupleEx
    {
        public static (T, T, T) Unpack3<T>(this T[] arr) => (arr[0], arr[1], arr[2]);
        public static (X, X, X) Unpack3<T, X>(this T[] arr, Func<T, X> convert) => (convert(arr[0]), convert(arr[1]), convert(arr[2]));
    }
}
