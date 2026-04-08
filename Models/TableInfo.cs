namespace SsmsAutocompletion {

    internal sealed class TableInfo {
        public string Schema    { get; }
        public string TableName { get; }

        public TableInfo(string schema, string tableName) {
            Schema    = schema;
            TableName = tableName;
        }

        public override string ToString() =>
            Schema == "dbo" ? TableName : $"{Schema}.{TableName}";
    }
}
