using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SsmsAutocompletion.Tests {

    [TestClass]
    public class AliasExtractorTests {

        private static readonly SsmsSqlParser  Parser    = new SsmsSqlParser();
        private static readonly AliasExtractor Extractor = new AliasExtractor();

        private IReadOnlyDictionary<string, TableInfo> Extract(string sql) =>
            Extractor.Extract(Parser.Parse(sql));

        // ── Explicit aliases ───────────────────────────────────────────────────

        [TestMethod]
        public void From_WithAlias_NoAs() {
            var map = Extract("SELECT * FROM Orders o");
            Assert.IsTrue(map.ContainsKey("o"));
            Assert.AreEqual("Orders", map["o"].TableName);
        }

        [TestMethod]
        public void From_WithAlias_WithAs() {
            var map = Extract("SELECT * FROM Orders AS o");
            Assert.IsTrue(map.ContainsKey("o"));
            Assert.AreEqual("Orders", map["o"].TableName);
        }

        [TestMethod]
        public void Join_WithAlias() {
            var map = Extract(
                "SELECT * FROM Customers c INNER JOIN Orders o ON c.Id = o.CustomerId");
            Assert.IsTrue(map.ContainsKey("c"));
            Assert.IsTrue(map.ContainsKey("o"));
            Assert.AreEqual("Customers", map["c"].TableName);
            Assert.AreEqual("Orders",    map["o"].TableName);
        }

        // ── No alias → table name used as key ─────────────────────────────────

        [TestMethod]
        public void From_NoAlias_TableNameIsKey() {
            var map = Extract("SELECT * FROM Orders");
            Assert.IsTrue(map.ContainsKey("orders")); // key lowercased
            Assert.AreEqual("Orders", map["orders"].TableName);
        }

        // ── Schema-qualified ───────────────────────────────────────────────────

        [TestMethod]
        public void Schema_Qualified_ExtractsTableAndSchema() {
            var map = Extract("SELECT * FROM dbo.Orders o");
            Assert.IsTrue(map.ContainsKey("o"));
            Assert.AreEqual("Orders", map["o"].TableName);
            Assert.AreEqual("dbo",    map["o"].Schema);
        }

        [TestMethod]
        public void Schema_WithBrackets() {
            var map = Extract("SELECT * FROM [dbo].[Orders] o");
            Assert.IsTrue(map.ContainsKey("o"));
            Assert.AreEqual("Orders", map["o"].TableName);
        }

        // ── Multiple joins ─────────────────────────────────────────────────────

        [TestMethod]
        public void MultipleJoins_AllAliasesExtracted() {
            var map = Extract(
                "SELECT * FROM A a LEFT JOIN B b ON a.Id = b.AId RIGHT JOIN C c ON b.Id = c.BId");
            Assert.AreEqual(3, map.Count);
            Assert.IsTrue(map.ContainsKey("a"));
            Assert.IsTrue(map.ContainsKey("b"));
            Assert.IsTrue(map.ContainsKey("c"));
        }

        // ── Null safety ────────────────────────────────────────────────────────

        [TestMethod]
        public void NullParseResult_ReturnsEmpty() {
            var map = Extractor.Extract(null);
            Assert.AreEqual(0, map.Count);
        }

        // ── Alias lookup is case-insensitive ──────────────────────────────────

        [TestMethod]
        public void AliasKey_CaseInsensitive() {
            var map = Extract("SELECT * FROM Orders O");
            // key is stored lowercased
            Assert.IsTrue(map.ContainsKey("o"));
        }
    }
}
