namespace UnityEngine.Terminal
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    public interface IShellHistory
    {
        void Push(string command);
        string Next();
        string Previous();
        void Clear();
    }

    public class CommandHistory : IShellHistory
    {
        private readonly List<string> history = new();
        private int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            history.Add(command);
            position = history.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Next()
        {
            position++;

            if (position < history.Count) 
                return history[position];
            position = history.Count;
            return string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Previous() 
        {
            if (history.Count == 0)
                return string.Empty;

            position--;

            if (position < 0) 
                position = 0;

            return history[position];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            history.Clear();
            position = 0;
        }
    }
}
