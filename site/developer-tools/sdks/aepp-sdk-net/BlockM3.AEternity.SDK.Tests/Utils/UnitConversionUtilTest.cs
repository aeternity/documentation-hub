using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockM3.AEternity.SDK.Tests.Utils
{
    [TestClass]
    public class UnitConversionUtilTest
    {
        [TestMethod]
        [TestCategory("Unit Conversion")]
        public void ToAettos()
        {
            Assert.AreEqual(1000000000000000000m, "1".ToAettos(Unit.AE));
        }

        [TestMethod]
        [TestCategory("Unit Conversion")]
        public void FromAettos()
        {
            Assert.AreEqual(1m, "1000000000000000000".FromAettos(Unit.AE));
        }
    }
}