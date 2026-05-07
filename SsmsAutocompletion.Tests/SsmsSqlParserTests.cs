using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SsmsAutocompletion.Tests {

    [TestClass]
    public class SsmsSqlParserTests {

        private readonly SsmsSqlParser _parser = new SsmsSqlParser();

        // ── Parse ──────────────────────────────────────────────────────────────

        [TestMethod] public void Parse_ValidSql_ReturnsNonNull() =>
            Assert.IsNotNull(_parser.Parse("SELECT 1"));

        [TestMethod] public void Parse_EmptyString_ReturnsNonNull() =>
            Assert.IsNotNull(_parser.Parse(""));

        [TestMethod] public void Parse_Null_ReturnsNonNull() =>
            Assert.IsNotNull(_parser.Parse(null));

        [TestMethod] public void Parse_WithCte_ReturnsScript() {
            var result = _parser.Parse("WITH cte AS (SELECT 1 AS id) SELECT * FROM cte");
            Assert.IsNotNull(result?.Script);
        }

        // ── GetLineColumn ──────────────────────────────────────────────────────

        [TestMethod] public void GetLineColumn_Position0_Returns1_1() {
            var (line, col) = _parser.GetLineColumn("SELECT", 0);
            Assert.AreEqual(1, line);
            Assert.AreEqual(1, col);
        }

        [TestMethod] public void GetLineColumn_MidFirstLine() {
            var (line, col) = _parser.GetLineColumn("SELECT 1", 5);
            Assert.AreEqual(1, line);
            Assert.AreEqual(6, col);
        }

        [TestMethod] public void GetLineColumn_StartOfSecondLine() {
            // "SELECT\n" → position 7 is start of line 2
            var (line, col) = _parser.GetLineColumn("SELECT\n1", 7);
            Assert.AreEqual(2, line);
            Assert.AreEqual(1, col);
        }

        [TestMethod] public void GetLineColumn_MidSecondLine() {
            var (line, col) = _parser.GetLineColumn("SELECT\nFROM t", 10);
            Assert.AreEqual(2, line);
            Assert.AreEqual(4, col);
        }

        [TestMethod] public void GetLineColumn_EmptyString() {
            var (line, col) = _parser.GetLineColumn("", 0);
            Assert.AreEqual(1, line);
            Assert.AreEqual(1, col);
        }

        [TestMethod] public void GetLineColumn_NegativePosition_Returns1_1() {
            var (line, col) = _parser.GetLineColumn("SELECT", -1);
            Assert.AreEqual(1, line);
            Assert.AreEqual(1, col);
        }

        [TestMethod] public void GetLineColumn_PositionBeyondLength_Clamped() {
            // Should not throw
            var (line, col) = _parser.GetLineColumn("SELECT", 100);
            Assert.AreEqual(1, line);
            Assert.IsTrue(col >= 1);
        }
    }
}
