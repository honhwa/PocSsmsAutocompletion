using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SsmsAutocompletion {

    internal static class AliasGenerator {

        public static string Generate(string tableName, ISet<string> existingAliases) {
            int dotIndex = tableName.LastIndexOf('.');
            if (dotIndex >= 0) tableName = tableName.Substring(dotIndex + 1);
            tableName = tableName.Trim('[', ']');
            var words       = SplitIntoWords(tableName);
            string baseAlias = BuildBaseAlias(words, tableName);
            if (!existingAliases.Contains(baseAlias)) return baseAlias;
            return FindAvailableAlias(baseAlias, existingAliases);
        }

        private static string BuildBaseAlias(List<string> words, string fallback) {
            if (words.Count > 0)
                return string.Concat(words.Select(word => char.ToLowerInvariant(word[0])));
            return fallback.Substring(0, 1).ToLowerInvariant();
        }

        private static string FindAvailableAlias(string baseAlias, ISet<string> existingAliases) {
            for (int suffix = 2; suffix < 100; suffix++) {
                string candidate = baseAlias + suffix;
                if (!existingAliases.Contains(candidate)) return candidate;
            }
            return baseAlias;
        }

        private static List<string> SplitIntoWords(string name) {
            var words   = new List<string>();
            var current = new StringBuilder();
            for (int i = 0; i < name.Length; i++) {
                char character = name[i];
                if (character == '_') {
                    if (current.Length > 0) { words.Add(current.ToString()); current.Clear(); }
                    continue;
                }
                bool isWordBoundary = char.IsUpper(character) && current.Length > 0
                    && (char.IsLower(current[current.Length - 1])
                        || (i + 1 < name.Length && char.IsLower(name[i + 1])));
                if (isWordBoundary) { words.Add(current.ToString()); current.Clear(); }
                current.Append(character);
            }
            if (current.Length > 0) words.Add(current.ToString());
            return words;
        }
    }
}
