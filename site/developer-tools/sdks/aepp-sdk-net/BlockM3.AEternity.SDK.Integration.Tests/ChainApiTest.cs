using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    [TestClass]
    [TestCategory("Integration Tests")]
    public class ChainApiTest : BaseTest
    {
        [TestMethod]
        public void GetCurrentKeyBlockTest()
        {
            KeyBlock k = nativeClient.GetCurrentKeyBlock();
            Assert.IsTrue(k.Height > 0);
        }
    }
}