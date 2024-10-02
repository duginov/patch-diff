using Microsoft.VisualStudio.TestTools.UnitTesting;
using Patch;

#pragma warning disable CA1859

namespace Tests
{
    [TestClass]
    public class CsvPatchLoaderTests
    {
        [TestMethod]
        public void ShouldLoadPatch()
        {
            IPatchLoader fut = new CsvPatchLoader();
            var result = fut.Load("Patch0.csv");
            Assert.IsNotNull(result);
        }

        [DataTestMethod]
        [DataRow("Patch0.csv", "Issuer")]
        [DataRow("Patch5.csv", "Field1")]
        public void ShouldDetermineKey(string fileName, string expectedKey)
        {
            IPatchLoader fut = new CsvPatchLoader();
            var result = fut.Load(fileName);
            Assert.IsNotNull(result.KeyName);
            Assert.AreEqual(expectedKey, result.KeyName);
        }

        [TestMethod]
        public void ShouldNotLoadFilesWithLessThan4Columns()
        {
            IPatchLoader fut = new CsvPatchLoader();
            Assert.ThrowsException<Exception>(() =>
            {
                fut.Load("Patch6.csv");
            });
        }

        [TestMethod]
        public void ShouldThrowIfNoData()
        {
            IPatchLoader fut = new CsvPatchLoader();
            Assert.ThrowsException<Exception>(() =>
            {
                fut.Load("Patch7.csv");
            });
        }

        [DataTestMethod]
        [DataRow("Patch0.csv", "Country,Conviction,Industry,Sector")]
        [DataRow("Patch5.csv", "Issuer,Sector,Industry,Country,Conviction,Analyst")]
        public void ShouldDetermineNonKeys(string fileName, string expectedNonKeys)
        {
            IPatchLoader fut = new CsvPatchLoader();
            var result = fut.Load(fileName);
            Assert.IsNotNull(result.ValueFields);
            var nonKeys = string.Join(",", result.ValueFields);
            Assert.AreEqual(expectedNonKeys, nonKeys);
        }

        [DataTestMethod]
        [DataRow("Patch0.csv", 3, 4)]
        [DataRow("Patch5.csv", 5, 6)]
        public void ShouldReadCorrectNumberOfLinesAndColumns(string fileName, int expectedNumberOfLines, int expectedNumberOfColumns)
        {
            IPatchLoader fut = new CsvPatchLoader();
            var result = fut.Load(fileName);
            Assert.IsNotNull(result.KeyName);
            Assert.AreEqual(expectedNumberOfLines, result.Lines.Count);
            Assert.IsTrue(result.Lines.Any(l=>l.Values.Count==expectedNumberOfColumns));
        }

        [TestMethod]
        public void ShouldDetectMissingEndDate()
        {
            IPatchLoader fut = new CsvPatchLoader();
            var ex = Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                fut.Load("Patch8.csv");
            });
            StringAssert.Contains(ex.Message, "EndDate");
        }

        [TestMethod]
        public void ShouldDetectMissingStartDate()
        {
            IPatchLoader fut = new CsvPatchLoader();
            var ex = Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                fut.Load("Patch9.csv");
            });
            StringAssert.Contains(ex.Message, "BeginDate");
        }

    }
}
