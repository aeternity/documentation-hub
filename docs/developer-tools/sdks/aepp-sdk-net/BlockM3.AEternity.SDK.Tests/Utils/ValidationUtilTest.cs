using System;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockM3.AEternity.SDK.Tests.Utils
{
    [TestClass]
    public class ValidationUtilTest : BaseTest
    {
        private readonly string domainMaxAllowedLength = "kryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokr.test";
        private readonly string domainTooLong = "kryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokrautskryptokra.test";

        [TestCategory("AENS Naming System")]
        [TestMethod]
        [Description("kryptokrauts.ae is not valid")]
        public void Domain1()
        {
            string domain = "kryptokrauts.ae";
            Assert.ThrowsException<ArgumentException>(() => Validation.CheckNamespace(domain));
        }

        [TestCategory("AENS Naming System")]
        [TestMethod]
        [Description("kryptokrauts.test is valid")]
        public void Domain2()
        {
            string domain = "kryptokrauts.test";
            Validation.CheckNamespace(domain);
        }

        [TestCategory("AENS Naming System")]
        [TestMethod]
        [Description("kryptokrauts is not valid")]
        public void Domain3()
        {
            string domain = "kryptokrauts";
            Assert.ThrowsException<ArgumentException>(() => Validation.CheckNamespace(domain));
        }

        [TestCategory("AENS Naming System")]
        [TestMethod]
        [Description("domain is too long")]
        public void Domain4()
        {
            Assert.ThrowsException<ArgumentException>(() => Validation.CheckNamespace(domainTooLong));
        }

        [TestCategory("AENS Naming System")]
        [TestMethod]
        [Description("domain length is valid")]
        public void Domain5()
        {
            Validation.CheckNamespace(domainMaxAllowedLength);
        }
    }
}