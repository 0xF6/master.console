namespace UnityEngine.Terminal
{
    public interface ICommandBuilder
    {
        ICommandBuilder Use<T>() where T : CommandSilo;
    }
}