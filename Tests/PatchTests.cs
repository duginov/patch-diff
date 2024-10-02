using Microsoft.VisualStudio.TestTools.UnitTesting;
using Patch;

namespace Tests
{
    [TestClass]
    public class PatchTests
    {
        [TestMethod]
        public void ShouldAddFields()
        {
            var line1 = new PatchLine { Key = new MasterKey("AAA", "", "") };
            var line2 = new PatchLine { Key = new MasterKey("BBB", "", "") };
            var fut = new Patch.Patch();
            fut.Lines.AddRange([line1, line2]);

            fut.AddFields(["field1","field2"]);
            
            // with NUnit Assert.Multiple() would be more appropriate here, MSTest does not have such method...
            Assert.IsTrue(fut.Lines.All(l => l.Values.ContainsKey("field1")));
            Assert.IsTrue(fut.Lines.All(l => l.Values.ContainsKey("field2")));
            Assert.IsTrue(fut.Lines.All(l => l.Values["field1"]==null));
            Assert.IsTrue(fut.Lines.All(l => l.Values["field2"]==null));
            Assert.IsTrue(fut.ValueFields.Contains("field1"));
            Assert.IsTrue(fut.ValueFields.Contains("field2"));
        }

        [TestMethod]
        public void ShouldRemoveFields()
        {
            // Arrange
            var line1 = new PatchLine
            {
                Key = new MasterKey("AAA", "", ""),
                Values = new Dictionary<string, string?> {{"Sector", "xxx"}, {"Industry", "yyy"}, {"Analyst", "zzz"}}
            };

            var line2 = new PatchLine
            {
                Key = new MasterKey("BBB", "", ""),
                Values = new Dictionary<string, string?> {{"Sector", ""}, {"Industry", "abc"}, {"Analyst", "xyz"}}
            };

            var fut = new Patch.Patch { ValueFields = ["Sector", "Industry", "Analyst"] };
            fut.Lines.AddRange([line1, line2]); 

            // Act
            fut.RemoveFields(["Sector", "Analyst"]);

            // Assert
            Assert.IsFalse(fut.Lines.All(l => l.Values.ContainsKey("Sector")));
            Assert.IsFalse(fut.Lines.All(l => l.Values.ContainsKey("Analyst")));
            Assert.IsFalse(fut.ValueFields.Contains("Sector"));
            Assert.IsFalse(fut.ValueFields.Contains("Analyst"));
        }

        [TestMethod]
        public void ShouldConsolidateSameKeyLines()
        {
            // Arrange
            var line1 = new PatchLine
            {
                Key = new MasterKey("PIPR", "", ""),
                Values = new Dictionary<string, string?> {{"Country","FRA"}, {"Conviction", null}, {"Industry", null}, {"Sector", "Consumer Discretionary"}}
            };

            var line2 = new PatchLine
            {
                Key = new MasterKey("PIPR", "", ""),
                Values = new Dictionary<string, string?> {{"Country","USA"}, {"Conviction", "High"}, {"Industry", null}, {"Sector", null}}
            };

            // add one more unrelated line to be more realistic
            var line3 = new PatchLine
            {
                Key = new MasterKey("FR", "", ""),
                Values = new Dictionary<string, string?> {{"Country","USA"}, {"Conviction", null}, {"Industry", null}, {"Sector", null}}
            };

            var fut = new Patch.Patch { ValueFields = ["Country", "Conviction", "Industry", "Sector"] };
            fut.Lines.AddRange([line1, line2, line3]);

            // Act
            fut.ConsolidateSameKeyLines();

            // Assert
            var consolidatedLine = fut.Lines.Single(l => l.Key!.Key == "PIPR");
            Assert.AreEqual(2, fut.Lines.Count);
            Assert.AreEqual("USA", consolidatedLine.Values["Country"]);
            Assert.AreEqual("High", consolidatedLine.Values["Conviction"]);
            Assert.AreEqual(null, consolidatedLine.Values["Industry"]);
            Assert.AreEqual("Consumer Discretionary", consolidatedLine.Values["Sector"]);

        }
    }
}
