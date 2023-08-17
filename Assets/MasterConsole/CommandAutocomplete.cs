namespace UnityEngine.Terminal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Buffers;

    public interface IShellAutocomplete
    {
        void Register(string word);
        string[] Complete(ref string text, ref int formatWidth, out IDisposable disposer);
    }

    public class CommandAutocomplete : IShellAutocomplete
    {
        private readonly List<string> knownWords = new();
        public void Register(string word) => knownWords.Add(word.ToLower());

        public string[] Complete(ref string text, ref int formatWidth, out IDisposable disposer) 
        {
            var partialWord = EatLastWord(ref text).ToLower();
            var partials = knownWords.Where(known => known.StartsWith(partialWord)).ToArray();
            var buffer = ArrayPool<string>.Shared.Rent(partials.Length);
            
            var bufferIndex = 0;
            foreach (var known in partials)
            {
                buffer[bufferIndex++] = known;
                
                if (known.Length > formatWidth) 
                    formatWidth = known.Length;
            }
            
            text += PartialWord(buffer);
            disposer = new ArrayPoolFree(buffer);
            return buffer;
        }


        class ArrayPoolFree : IDisposable
        {
            private readonly string[] buffer;

            public ArrayPoolFree(string[] buffer)
            {
                this.buffer = buffer;
            }
            public void Dispose()
            {
                ArrayPool<string>.Shared.Return(buffer, true);
            }
        }

        private static string EatLastWord(ref string text) 
        {
            var lastSpace = text.LastIndexOf(' ');
            var result = text[(lastSpace + 1)..];
            text = text[..(lastSpace + 1)];
            return result;
        }

        private static string PartialWord(IReadOnlyList<string> words) 
        {
            if (words.Count == 0)
                return string.Empty;

            var firstMatch = words[0];
            var partialLength = firstMatch.Length;

            if (words.Count == 1)
                return firstMatch;

            foreach (var word in words) 
            {
                if (string.IsNullOrEmpty(word)) continue;
                if (partialLength > word.Length) partialLength = word.Length;

                for (var i = 0; i < partialLength; i++) if (word[i] != firstMatch[i])
                    partialLength = i;
            }
            return firstMatch[..partialLength];
        }
    }
}
