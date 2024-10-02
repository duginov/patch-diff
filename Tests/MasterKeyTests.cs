using Microsoft.VisualStudio.TestTools.UnitTesting;
using Patch;

namespace Tests
{
    [TestClass]
    public class MasterKeyTests
    {
        [TestMethod]
        public void ShouldConstructFromDataStrings()
        {
            _ = new MasterKey("AAA", "1/1/2020", "02/02/2022");
            _ = new MasterKey("AAA", "2020-1-1", "");
            _ = new MasterKey("AAA", "", "05/5/2024");
            _ = new MasterKey("AAA", "", "20240505");
            _ = new MasterKey("AAA", "", "");
        }

        [DataTestMethod]
        [DataRow("2024-01-01","200004-01-01")]
        [DataRow("2024-02-30","1/1/2025")]
        public void ShouldThrowForInvalidDates(string fromDate, string toDate)
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                _ = new MasterKey("SomeKey", fromDate, toDate);
            });
        }
    }
}