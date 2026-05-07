using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SsmsAutocompletion.Tests {

    [TestClass]
    public class AliasGeneratorTests {

        private static string Gen(string table, params string[] taken) =>
            AliasGenerator.Generate(table, new HashSet<string>(taken));

        // ── Base alias generation ──────────────────────────────────────────────

        [TestMethod] public void CamelCase_MultiWord() =>
            Assert.AreEqual("co",  Gen("CustomerOrder"));

        [TestMethod] public void PascalCase_ThreeWords() =>
            Assert.AreEqual("soh", Gen("SalesOrderHeader"));

        [TestMethod] public void Underscore_Separator() =>
            Assert.AreEqual("od",  Gen("order_details"));

        [TestMethod] public void Mixed_CamelUnderscore() =>
            Assert.AreEqual("co",  Gen("Customer_Order"));

        [TestMethod] public void SingleWord_LowerCase() =>
            Assert.AreEqual("o",   Gen("orders"));

        [TestMethod] public void SingleWord_AllUpperCase_AllLettersIncluded() =>
            // Every letter is uppercase → all are included (not treated as acronym)
            Assert.AreEqual("orders", Gen("ORDERS"));

        [TestMethod] public void SingleChar() =>
            Assert.AreEqual("t",   Gen("T"));

        // ── Schema / bracket stripping ─────────────────────────────────────────

        [TestMethod] public void Schema_Stripped() =>
            Assert.AreEqual("o",   Gen("dbo.Orders"));

        [TestMethod] public void Brackets_Stripped() =>
            Assert.AreEqual("o",   Gen("[Orders]"));

        [TestMethod] public void Schema_And_Brackets_Stripped() =>
            Assert.AreEqual("o",   Gen("dbo.[Orders]"));

        [TestMethod] public void Schema_WithBracketedTable() =>
            Assert.AreEqual("soh", Gen("[dbo].[SalesOrderHeader]"));

        // ── Disambiguation ─────────────────────────────────────────────────────

        [TestMethod] public void Conflict_AppendsTwo() =>
            Assert.AreEqual("co2", Gen("CustomerOrder", "co"));

        [TestMethod] public void Conflict_AppendsThree() =>
            Assert.AreEqual("co3", Gen("CustomerOrder", "co", "co2"));

        [TestMethod] public void NoConflict_ReturnsBase() =>
            Assert.AreEqual("co",  Gen("CustomerOrder", "soh", "od"));

        // ── Edge cases ─────────────────────────────────────────────────────────

        [TestMethod] public void LeadingUnderscore() =>
            Assert.AreEqual("o",   Gen("_Orders"));

        [TestMethod] public void AllUnderscores_FallbackFirstChar() {
            // "_" strips to empty → fallback to 't'
            string alias = Gen("_");
            Assert.IsTrue(alias.Length > 0);
        }
    }
}
