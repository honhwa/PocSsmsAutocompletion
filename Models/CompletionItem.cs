namespace SsmsAutocompletion {

    internal sealed class CompletionItem {
        public string DisplayText { get; }
        public string InsertText  { get; }
        public string Description { get; }

        public CompletionItem(string displayText, string insertText, string description) {
            DisplayText = displayText;
            InsertText  = insertText;
            Description = description;
        }
    }
}
