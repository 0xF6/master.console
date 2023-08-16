namespace UnityEngine.Terminal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Buffers;

    public interface IShellAutocomplete
    {
        void Register(string word);
        string[] Complete(ref string text, ref int formatWidth);
    }

    public class CommandAutocomplete : IShellAutocomplete
    {
        private readonly HashSet<string> knownWords = new();
        public void Register(string word) => knownWords.Add(word.ToLower());

        public string[] Complete(ref string text, ref int formatWidth) 
        {
            var partial_word = EatLastWord(ref text).ToLower();
            var buffer = ArrayPool<string>.Shared.Rent(knownWords.Count);
            
            var bufferIndex = 0;
            foreach (var known in knownWords.Where(known => known.StartsWith(partial_word)))
            {
                buffer[bufferIndex++] = known;
                
                if (known.Length > formatWidth) 
                    formatWidth = known.Length;
            }
            
            var completions = buffer.ToArray();
            ArrayPool<string>.Shared.Return(buffer, true);
            text += PartialWord(completions);
            return completions;
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
                if (partialLength > word.Length) partialLength = word.Length;

                for (var i = 0; i < partialLength; i++) if (word[i] != firstMatch[i])
                    partialLength = i;
            }
            return firstMatch[..partialLength];
        }
    }
}
