using Microsoft.VisualStudio.TestTools.UnitTesting;
using Patch;

#pragma warning disable CA1859

namespace Tests
{
    using Values = Dictionary<string, string?>;

    [TestClass]
    public class DiffDetectorTests
    {
        [TestMethod]
        public void ShouldDetectRemovedFields()
        {

            var oldPatch = new Patch.Patch { ValueFields = ["BeginDate", "EndDate", "Issuer", "Sector", "Industry", "Country", "Conviction", "Analyst"] };
            var newPatch = new Patch.Patch { ValueFields = ["BeginDate", "EndDate", "Issuer", "Sector", "Industry", "Analyst"] };

            IDiffDetector fut = new DiffDetector();

            var result = fut.DetectRemovedFields(oldPatch, newPatch);
            Assert.AreEqual(2,result.Count);
            Assert.IsTrue(result.Contains("Country"));
            Assert.IsTrue(result.Contains("Conviction"));
        }

        [TestMethod]
        public void ShouldDetectAddedFields()
        {
            var oldPatch = new Patch.Patch { ValueFields = ["BeginDate", "EndDate", "Issuer", "Sector", "Industry", "Analyst"] };
            var newPatch = new Patch.Patch { ValueFields = ["BeginDate", "EndDate", "Issuer", "Sector", "Industry", "Country", "Conviction", "Analyst"] };

            IDiffDetector fut = new DiffDetector();

            var result = fut.DetectAddedColumns(oldPatch, newPatch);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("Country"));
            Assert.IsTrue(result.Contains("Conviction"));
        }

        [TestMethod]
        public void ShouldDetectLineChanges()
        {
            // Arrange
            var key1 = new MasterKey("PIPR", "", "");
            var oldValues1 = new Values {{"Country", "USA"}, {"Industry", null}, {"Sector", "ORIGINAL"}};
            // two fields changed, ONE effective change, another is null
            var newValues1 = new Values(oldValues1) { ["Industry"] = "CHANGED", ["Sector"] = null };
            var oldLine1 = CreatePatchLine(key1, oldValues1);
            var newLine1 = CreatePatchLine(key1, newValues1);

            var key2 = new MasterKey("JBL", "", "02/02/2024");
            var oldValues2 = new Values {{"Country", "USA"}, {"Industry", null}, {"Sector", null}};
            // no value changes
            var newValues2 = new Values(oldValues2);
            var oldLine2 = CreatePatchLine(key2, oldValues2);
            var newLine2 = CreatePatchLine(key2, newValues2);

            // one line removed for the patch, another line added - should not affect DetectChangesInLines() result 
            var oldData3 = new Values {{ "Country", null }, {"Industry", "Energy"}};
            var oldLine3 = CreatePatchLine("FR", "", "", oldData3);
            var newData3 = new Values {{ "Country", null }, {"Industry", "Computers"}};
            var newLine3 = CreatePatchLine("XYZ", "", "", newData3);

            IDiffDetector fut = new DiffDetector();
            
            // Act
            var result = fut.DetectChangesInLines([oldLine1, oldLine2, oldLine3], [newLine1, newLine2, newLine3]);

            // Assert
            Assert.AreEqual(1, result.Count, "Wrong number of changed lines detected");
            var lineDiffs = result.First().ValueDiffs;
            Assert.AreEqual(1, lineDiffs.Count, "Wrong number of detected value changes");
            Assert.AreEqual(null, lineDiffs["Industry"].OldValue);
            Assert.AreEqual("CHANGED", lineDiffs["Industry"].NewValue);
        }

        [TestMethod]
        // this also tests detection of added lines, with reversed inputs
        public void ShouldDetectRemovedLines()
        {
            // Arrange
            var oldLine1 = CreatePatchLine("PIPR", "","", new Values {{"Country", "USA"}, {"Industry", null}, {"Sector", null}});
            var oldLine2 = CreatePatchLine("JBL", "","02/02/2024", new Values {{"Country", "CAN"}, {"Industry", "Computers"}, {"Sector", null}});
            var oldLine3 = CreatePatchLine("FR", "","", new Values {{"Country", null}, {"Industry", "Computers"}, {"Sector", null}});

            var newLine = CreatePatchLine("FR", "", "", new Values { { "Country", null }, { "Industry", "Computers" }, { "Sector", null } });

            IDiffDetector fut = new DiffDetector();

            // Act
            var result = fut.DetectUniqueLines([oldLine1,oldLine2,oldLine3], [newLine]);

            // Assert
            Assert.AreEqual(2, result.Count, "Wrong number of removed lines detected");
            
            var line1 = result.Single(r => r.Key!.Key == "PIPR");
            // check if only non-null values are left for reporting
            Assert.AreEqual(1,line1.Values.Count, "Wrong number of non-null value fields");
            Assert.AreEqual("USA",line1.Values["Country"]);

            var line2 = result.Single(r => r.Key!.Key == "JBL");
            // check if only non-null values are left for reporting
            Assert.AreEqual(2, line2.Values.Count, "Wrong number of non-null value fields");
            Assert.AreEqual("Computers", line2.Values["Industry"]);
            Assert.AreEqual("CAN", line2.Values["Country"]);

        }

        [TestMethod]
        public void ShouldTreatChangedDateAsRemovalAndAddition()
        {
            // Arrange
            var oldLine = CreatePatchLine("PIPR", "02/02/2024", "02/05/2024", new Values { { "Country", "USA" }, { "Industry", null }});
            var newLine = CreatePatchLine("PIPR", "02/02/2024", "12/12/2024", new Values { { "Country", "USA" }, { "Industry", null }});

            IDiffDetector fut = new DiffDetector();

            // Act
            var removedLines = fut.DetectUniqueLines([oldLine], [newLine]);
            var addedLines = fut.DetectUniqueLines([newLine], [oldLine]);
            var changedLines = fut.DetectChangesInLines([oldLine], [newLine]);

            // Assert
            Assert.AreEqual(1, removedLines.Count);
            Assert.AreEqual(1, addedLines.Count);
            Assert.AreEqual(0, changedLines.Count);
        }

        private static PatchLine CreatePatchLine(string key, string from, string to, Values values)
        {
            return CreatePatchLine(new MasterKey(key, from, to), values);
        }

        private static PatchLine CreatePatchLine(MasterKey key, Values values)
        {
            return new PatchLine
            {
                Key = key,
                Values = values
            };
        }
    }
}
