using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SsmsAutocompletion {

    internal sealed class DatabaseMetadataCache : IDatabaseMetadata {

        public static readonly IDatabaseMetadata Instance = new DatabaseMetadataCache();

        private static readonly object Lock    = new object();
        private static readonly TimeSpan Ttl   = TimeSpan.FromMinutes(10);
        private static readonly Dictionary<string, CacheEntry> Entries =
            new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

        public void WarmAsync(ConnectionKey connectionKey, ServerConnection serverConnection) {
            if (connectionKey == null || connectionKey.IsEmpty || serverConnection == null) return;
            Task.Run(() => EnsureLoaded(connectionKey, serverConnection));
        }

        public IMetadataProvider GetMetadataProvider(ConnectionKey connectionKey) {
            if (connectionKey == null || connectionKey.IsEmpty) return null;
            lock (Lock) {
                if (Entries.TryGetValue(connectionKey.ToString(), out var entry) && !entry.IsExpired)
                    return entry.MetadataProvider;
            }
            return null;
        }

        public IReadOnlyList<TableInfo> GetTables(ConnectionKey connectionKey) {
            if (connectionKey == null || connectionKey.IsEmpty) return Array.Empty<TableInfo>();
            lock (Lock) {
                if (Entries.TryGetValue(connectionKey.ToString(), out var entry) && !entry.IsExpired)
                    return entry.Tables;
            }
            return Array.Empty<TableInfo>();
        }

        public IReadOnlyList<ColumnInfo> GetColumns(ConnectionKey connectionKey, string schema, string tableName) {
            if (connectionKey == null || connectionKey.IsEmpty) return Array.Empty<ColumnInfo>();
            lock (Lock) {
                if (Entries.TryGetValue(connectionKey.ToString(), out var entry) && !entry.IsExpired) {
                    if (entry.Columns.TryGetValue(MakeTableKey(schema, tableName), out var columns))
                        return columns;
                }
            }
            return Array.Empty<ColumnInfo>();
        }

        public IReadOnlyList<ForeignKeyInfo> GetForeignKeys(ConnectionKey connectionKey, string schema, string tableName) {
            if (connectionKey == null || connectionKey.IsEmpty) return Array.Empty<ForeignKeyInfo>();
            lock (Lock) {
                if (Entries.TryGetValue(connectionKey.ToString(), out var entry) && !entry.IsExpired) {
                    if (entry.ForeignKeys.TryGetValue(MakeTableKey(schema, tableName), out var foreignKeys))
                        return foreignKeys;
                }
            }
            return Array.Empty<ForeignKeyInfo>();
        }

        public void Invalidate(ConnectionKey connectionKey) {
            if (connectionKey == null || connectionKey.IsEmpty) return;
            lock (Lock) { Entries.Remove(connectionKey.ToString()); }
        }

        private static void EnsureLoaded(ConnectionKey connectionKey, ServerConnection serverConnection) {
            lock (Lock) {
                if (Entries.TryGetValue(connectionKey.ToString(), out var existing) && !existing.IsExpired)
                    return;
            }
            var newEntry = LoadFromSmo(serverConnection);
            lock (Lock) {
                if (!Entries.TryGetValue(connectionKey.ToString(), out var existing2) || existing2.IsExpired)
                    Entries[connectionKey.ToString()] = newEntry;
            }
        }

        private static CacheEntry LoadFromSmo(ServerConnection serverConnection) {
            var tables      = new List<TableInfo>();
            var columnMap   = new Dictionary<string, List<ColumnInfo>>(StringComparer.OrdinalIgnoreCase);
            var foreignKeyMap = new Dictionary<string, List<ForeignKeyInfo>>(StringComparer.OrdinalIgnoreCase);
            IMetadataProvider metadataProvider = null;
            try {
                metadataProvider = SmoMetadataProvider.CreateConnectedProvider(serverConnection);
                var server   = new Server(serverConnection);
                var database = server.Databases[serverConnection.DatabaseName];
                if (database == null) return CacheEntry.Empty();
                foreach (Table table in database.Tables) {
                    if (table.IsSystemObject) continue;
                    tables.Add(new TableInfo(table.Schema, table.Name));
                    PopulateColumnsAndForeignKeys(table, columnMap, foreignKeyMap);
                }
            }
            catch { }
            return BuildCacheEntry(metadataProvider, tables, columnMap, foreignKeyMap);
        }

        private static void PopulateColumnsAndForeignKeys(
            Table table,
            Dictionary<string, List<ColumnInfo>> columnMap,
            Dictionary<string, List<ForeignKeyInfo>> foreignKeyMap) {
            string tableKey = MakeTableKey(table.Schema, table.Name);
            var columnList  = new List<ColumnInfo>();
            foreach (Column column in table.Columns)
                columnList.Add(new ColumnInfo(column.Name, column.DataType.Name));
            columnMap[tableKey] = columnList;
            foreach (ForeignKey foreignKey in table.ForeignKeys)
                AddForeignKeyToMap(foreignKey, table.Schema, table.Name, foreignKeyMap);
        }

        private static void AddForeignKeyToMap(
            ForeignKey foreignKey, string ownerSchema, string ownerTable,
            Dictionary<string, List<ForeignKeyInfo>> foreignKeyMap) {
            var fkColumns  = new List<string>();
            var refColumns = new List<string>();
            foreach (ForeignKeyColumn fkColumn in foreignKey.Columns) {
                fkColumns.Add(fkColumn.Name);
                refColumns.Add(fkColumn.ReferencedColumn);
            }
            var foreignKeyInfo = new ForeignKeyInfo(
                ownerSchema, ownerTable, fkColumns.AsReadOnly(),
                foreignKey.ReferencedTableSchema, foreignKey.ReferencedTable, refColumns.AsReadOnly());
            string ownerKey = MakeTableKey(ownerSchema, ownerTable);
            AddToListMap(foreignKeyMap, ownerKey, foreignKeyInfo);
            string referencedKey = MakeTableKey(foreignKey.ReferencedTableSchema, foreignKey.ReferencedTable);
            AddToListMap(foreignKeyMap, referencedKey, foreignKeyInfo);
        }

        private static void AddToListMap<T>(Dictionary<string, List<T>> map, string key, T value) {
            if (!map.TryGetValue(key, out var list)) {
                list     = new List<T>();
                map[key] = list;
            }
            list.Add(value);
        }

        private static CacheEntry BuildCacheEntry(
            IMetadataProvider metadataProvider, List<TableInfo> tables,
            Dictionary<string, List<ColumnInfo>> columnMap,
            Dictionary<string, List<ForeignKeyInfo>> foreignKeyMap) {
            var frozenColumns = new Dictionary<string, IReadOnlyList<ColumnInfo>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in columnMap) frozenColumns[kv.Key] = kv.Value.AsReadOnly();
            var frozenForeignKeys = new Dictionary<string, IReadOnlyList<ForeignKeyInfo>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in foreignKeyMap) frozenForeignKeys[kv.Key] = kv.Value.AsReadOnly();
            return new CacheEntry(metadataProvider, tables.AsReadOnly(), frozenColumns, frozenForeignKeys);
        }

        private static string MakeTableKey(string schema, string tableName) =>
            $"{schema ?? "dbo"}.{tableName}";

        private sealed class CacheEntry {
            public readonly IMetadataProvider MetadataProvider;
            public readonly IReadOnlyList<TableInfo> Tables;
            public readonly IReadOnlyDictionary<string, IReadOnlyList<ColumnInfo>> Columns;
            public readonly IReadOnlyDictionary<string, IReadOnlyList<ForeignKeyInfo>> ForeignKeys;
            private readonly DateTime _loadedAt;

            public bool IsExpired => DateTime.UtcNow - _loadedAt > Ttl;

            public CacheEntry(
                IMetadataProvider metadataProvider,
                IReadOnlyList<TableInfo> tables,
                IReadOnlyDictionary<string, IReadOnlyList<ColumnInfo>> columns,
                IReadOnlyDictionary<string, IReadOnlyList<ForeignKeyInfo>> foreignKeys) {
                MetadataProvider = metadataProvider;
                Tables           = tables;
                Columns          = columns;
                ForeignKeys      = foreignKeys;
                _loadedAt        = DateTime.UtcNow;
            }

            public static CacheEntry Empty() => new CacheEntry(
                null,
                Array.Empty<TableInfo>(),
                new Dictionary<string, IReadOnlyList<ColumnInfo>>(),
                new Dictionary<string, IReadOnlyList<ForeignKeyInfo>>());
        }
    }
}
