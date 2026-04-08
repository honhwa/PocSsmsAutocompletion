using Microsoft.SqlServer.Management.SqlParser.Parser;

namespace SsmsAutocompletion {

    internal interface ISqlParser {
        ParseResult Parse(string sql);
        (int line, int column) GetLineColumn(string sql, int position);
    }
}
