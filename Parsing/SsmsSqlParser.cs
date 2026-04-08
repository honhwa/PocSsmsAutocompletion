using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio.Text;
using System;

namespace SsmsAutocompletion {

    internal sealed class SsmsSqlParser : ISqlParser {

        public ParseResult Parse(string sql) {
            try { return Parser.Parse(sql ?? ""); }
            catch { return null; }
        }

        public (int line, int column) GetLineColumn(string sql, int position) {
            if (string.IsNullOrEmpty(sql) || position < 0) return (1, 1);
            int safePosition = Math.Min(position, sql.Length);
            int lineNumber   = 1;
            int lineStart    = 0;
            for (int i = 0; i < safePosition; i++) {
                if (sql[i] != '\n') continue;
                lineNumber++;
                lineStart = i + 1;
            }
            return (lineNumber, safePosition - lineStart + 1);
        }

        public static (int line, int column) GetLineColumnFromSnapshot(ITextSnapshot snapshot, int position) {
            var textLine = snapshot.GetLineFromPosition(Math.Min(position, snapshot.Length));
            return (textLine.LineNumber + 1, position - textLine.Start.Position + 1);
        }
    }
}
