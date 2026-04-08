using System.Collections.Generic;

namespace SsmsAutocompletion {

    internal sealed class ForeignKeyInfo {
        public string FkSchema    { get; }
        public string FkTable     { get; }
        public IReadOnlyList<string> FkColumns         { get; }
        public string ReferencedSchema { get; }
        public string ReferencedTable  { get; }
        public IReadOnlyList<string> ReferencedColumns { get; }

        public ForeignKeyInfo(
            string fkSchema, string fkTable, IReadOnlyList<string> fkColumns,
            string referencedSchema, string referencedTable, IReadOnlyList<string> referencedColumns) {
            FkSchema           = fkSchema;
            FkTable            = fkTable;
            FkColumns          = fkColumns;
            ReferencedSchema   = referencedSchema;
            ReferencedTable    = referencedTable;
            ReferencedColumns  = referencedColumns;
        }
    }
}
