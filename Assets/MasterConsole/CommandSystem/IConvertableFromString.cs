namespace UnityEngine.Terminal
{
    public interface IConvertableFromString<out T> : IConvertableFromString
    {
        new T ConvertFromString(string str);
    }

    public interface IConvertableFromString
    {
        object ConvertFromString(string str);
    }
}
