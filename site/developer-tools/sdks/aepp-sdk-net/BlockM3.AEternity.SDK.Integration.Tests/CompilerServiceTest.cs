using System.IO;
using System.Text;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    [TestClass]
    [TestCategory("Integration Tests")]
    public class CompilerServiceTest : BaseTest
    {
        [TestMethod]
        public void TestCompileContract()
        {
            ByteCode byteCode = nativeClient.Compile(TestConstants.TestContractSourceCode, null, null);
            Assert.AreEqual(TestConstants.TestContractByteCode, byteCode.Bytecode);
        }

        [TestMethod]
        public void TestCompileContractCall()
        {
            Calldata calldata = nativeClient.EncodeCallData(TestConstants.TestContractSourceCode, TestConstants.TestContractFunction, TestConstants.TestContractFunctionParams);
            Assert.AreEqual(TestConstants.EncodedServiceCall, calldata.CallData);
        }

        [TestMethod]
        public void TestDecodeCalldata()
        {
            SophiaJsonData callData = nativeClient.DecodeCallData(TestConstants.EncodedServiceCallAnswer, "int");
            Assert.AreEqual(TestConstants.ServiceCallAnswerJSON, ((JObject) callData.Data).ToString(Formatting.None));
        }

        [TestMethod]
        public void TestEncodeCalldata()
        {
            Calldata callData = nativeClient.EncodeCallData(TestConstants.TestContractSourceCode, "init");
            Assert.AreEqual(TestConstants.TestContractCallData, callData.CallData);
        }

        [TestMethod]
        public void TestGenerateACI()
        {
            string paymentSplitterSource = File.ReadAllText(Path.Combine(ResourcePath, "contracts", "PaymentSplitter.aes"), Encoding.UTF8);
            ACI aci = nativeClient.GenerateACI(paymentSplitterSource);
            Assert.AreEqual(TestConstants.PaymentSplitterACI, ((JObject) aci.EncodedAci).ToString(Formatting.None));
        }
    }
}