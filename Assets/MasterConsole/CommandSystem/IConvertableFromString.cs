namespace UnityEngine.Terminal
{
    public interface IConvertableFromString
    {
        object ConvertFromString(string str);
    }
}