using System;
using System.Collections.Generic;

namespace SsmsAutocompletion {

    internal sealed class CompletionEngine {
        private readonly IReadOnlyList<ICompletionProvider> _providers;
        private readonly CompletionRequestBuilder           _requestBuilder;
        private readonly IContextDetector                   _contextDetector;

        public CompletionEngine(
            IReadOnlyList<ICompletionProvider> providers,
            CompletionRequestBuilder requestBuilder,
            IContextDetector contextDetector) {
            _providers       = providers;
            _requestBuilder  = requestBuilder;
            _contextDetector = contextDetector;
        }

        public IReadOnlyList<CompletionItem> GetCompletions(
            Microsoft.VisualStudio.Text.ITextSnapshot snapshot,
            string sql, int caretPosition, ConnectionKey connectionKey) {
            var request  = _requestBuilder.Build(snapshot, sql, caretPosition, connectionKey);
            var allItems = CollectAllItems(request);
            var deduplicated = Deduplicate(allItems);
            return FilterByPrefix(deduplicated, _contextDetector.GetCurrentWord(snapshot, caretPosition));
        }

        private List<CompletionItem> CollectAllItems(CompletionRequest request) {
            var allItems = new List<CompletionItem>();
            foreach (var provider in _providers)
                allItems.AddRange(provider.GetCompletions(request));
            return allItems;
        }

        private static IReadOnlyList<CompletionItem> Deduplicate(List<CompletionItem> items) {
            var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<CompletionItem>(items.Count);
            foreach (var item in items) {
                if (!seen.Add(item.DisplayText)) continue;
                result.Add(item);
            }
            return result.AsReadOnly();
        }

        private static IReadOnlyList<CompletionItem> FilterByPrefix(
            IReadOnlyList<CompletionItem> items, string prefix) {
            if (string.IsNullOrEmpty(prefix)) return items;
            var filtered = new List<CompletionItem>();
            foreach (var item in items) {
                if (item.DisplayText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    filtered.Add(item);
            }
            return filtered.AsReadOnly();
        }
    }
}
