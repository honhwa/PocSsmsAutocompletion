using System.Collections.Generic;

namespace SsmsAutocompletion {

    internal interface ICompletionProvider {
        IReadOnlyList<CompletionItem> GetCompletions(CompletionRequest request);
    }
}
