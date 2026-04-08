namespace SsmsAutocompletion {

    internal sealed class ColumnInfo {
        public string ColumnName { get; }
        public string DataType   { get; }

        public ColumnInfo(string columnName, string dataType) {
            ColumnName = columnName;
            DataType   = dataType;
        }
    }
}
