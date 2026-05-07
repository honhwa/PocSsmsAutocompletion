using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SsmsAutocompletion.Tests {

    [TestClass]
    public class CteExtractorTests {

        private static readonly SsmsSqlParser  Parser    = new SsmsSqlParser();
        private static readonly CteExtractor   Extractor = new CteExtractor();

        private static System.Collections.Generic.IReadOnlyList<string> Extract(string sql) =>
            Extractor.Extract(Parser.Parse(sql));

        // ── Happy path ─────────────────────────────────────────────────────────

        [TestMethod]
        public void SingleCte_ReturnsName() {
            var names = Extract("WITH cte AS (SELECT 1 AS id) SELECT * FROM cte");
            CollectionAssert.Contains(names.ToList(), "cte");
            Assert.AreEqual(1, names.Count);
        }

        [TestMethod]
        public void MultipleCtes_ReturnsBothNames() {
            var names = Extract(
                "WITH a AS (SELECT 1), b AS (SELECT 2) SELECT * FROM a JOIN b ON 1=1");
            Assert.AreEqual(2, names.Count);
            CollectionAssert.Contains(names.ToList(), "a");
            CollectionAssert.Contains(names.ToList(), "b");
        }

        [TestMethod]
        public void CteWithExplicitColumns_ReturnsName() {
            var names = Extract(
                "WITH cte(col1, col2) AS (SELECT 1, 2) SELECT * FROM cte");
            Assert.AreEqual(1, names.Count);
            Assert.AreEqual("cte", names[0]);
        }

        [TestMethod]
        public void ThreeCtes_AllReturned() {
            var names = Extract(
                "WITH x AS (SELECT 1), y AS (SELECT 2), z AS (SELECT 3) SELECT * FROM x");
            Assert.AreEqual(3, names.Count);
            CollectionAssert.Contains(names.ToList(), "x");
            CollectionAssert.Contains(names.ToList(), "y");
            CollectionAssert.Contains(names.ToList(), "z");
        }

        // ── No CTEs ────────────────────────────────────────────────────────────

        [TestMethod]
        public void NoCte_ReturnsEmpty() {
            var names = Extract("SELECT * FROM Orders");
            Assert.AreEqual(0, names.Count);
        }

        [TestMethod]
        public void EmptySql_ReturnsEmpty() {
            var names = Extract("");
            Assert.AreEqual(0, names.Count);
        }

        // ── Null safety ────────────────────────────────────────────────────────

        [TestMethod]
        public void NullParseResult_ReturnsEmpty() {
            var names = Extractor.Extract(null);
            Assert.AreEqual(0, names.Count);
        }

        // ── Deduplication ──────────────────────────────────────────────────────

        [TestMethod]
        public void PreservesOrder_FirstCteFirst() {
            var names = Extract(
                "WITH alpha AS (SELECT 1), beta AS (SELECT 2) SELECT * FROM alpha");
            Assert.AreEqual("alpha", names[0]);
            Assert.AreEqual("beta",  names[1]);
        }
    }
}
