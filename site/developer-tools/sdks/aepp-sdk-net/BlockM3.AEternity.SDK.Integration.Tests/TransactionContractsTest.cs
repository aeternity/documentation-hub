using System;
using System.Collections.Generic;
using System.Numerics;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Transactions.Contracts;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.RLP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    [TestClass]
    [TestCategory("Integration Tests")]

    public class TransactionContractsTest : BaseTest
    {
        private static string localDeployedContractId;

        [TestInitialize]
        public void initBeforeTest()
        {
            DeployContractNativeOnLocalNode();
        }

        [TestMethod]
        public void DecodeRLPArrayTest()
        {
            try
            {
                byte[] value = Hex.Decode(TestConstants.BinaryTxDevnet);
                RLPCollection el = RLP.Decode(value) as RLPCollection;
                if (el == null || el.Count <= 12)
                    Assert.Fail("Collection at least of 12 items required");

                Assert.AreEqual(Constants.SerializationTags.OBJECT_TAG_CONTRACT_CREATE_TRANSACTION, el[0].RLPData.ToIntFromRLPDecoded());
                Assert.AreEqual(Constants.SerializationTags.VSN, el[1].RLPData.ToIntFromRLPDecoded());
                CollectionAssert.AreEqual(el[2].RLPData, Encoding.DecodeCheckAndTag(baseKeyPair.PublicKey, Constants.SerializationTags.ID_TAG_ACCOUNT));
                Assert.AreEqual(1, el[3].RLPData.ToBigIntegerFromRLPDecoded());
                CollectionAssert.AreEqual(el[4].RLPData, Encoding.DecodeCheckWithIdentifier(TestConstants.TestContractByteCode));
                Assert.AreEqual(262145, el[5].RLPData.ToBigIntegerFromRLPDecoded());
                Assert.AreEqual(1098660000000000, el[6].RLPData.ToBigIntegerFromRLPDecoded());
                Assert.AreEqual(20000, el[7].RLPData.ToBigIntegerFromRLPDecoded());
                Assert.AreEqual(0, el[8].RLPData.ToBigIntegerFromRLPDecoded());
                Assert.AreEqual(0, el[9].RLPData.ToBigIntegerFromRLPDecoded());
                Assert.AreEqual(1000, el[10].RLPData.ToBigIntegerFromRLPDecoded());
                Assert.AreEqual(1100000000, el[11].RLPData.ToBigIntegerFromRLPDecoded());
                CollectionAssert.AreEqual(Encoding.DecodeCheckWithIdentifier(TestConstants.TestContractCallData), el[12].RLPData);
            }
            catch (Exception afe)
            {
                logger.LogError("Error decoding RLP array");
                Assert.Fail(afe.Message);
            }
        }

        /**
         * create an unsigned native CreateContract transaction
         *
         * @param context
         */
        [TestMethod]
        public void BuildCreateContractTransactionTest()
        {
            string ownerId = baseKeyPair.PublicKey;
            ushort abiVersion = 1;
            ushort vmVersion = 4;
            BigInteger amount = 100;
            BigInteger deposit = 100;
            ulong ttl = 20000;
            ulong gas = 1000;
            BigInteger gasPrice = 1100000000;

            ulong nonce = 1;
            ContractCreateTransaction contractTx = nativeClient.CreateContractCreateTransaction(abiVersion, amount, TestConstants.TestContractCallData, TestConstants.TestContractByteCode, deposit, gas, gasPrice, nonce, ownerId, ttl, vmVersion);
            ContractCreateTransaction contractTxDebug = debugClient.CreateContractCreateTransaction(abiVersion, amount, TestConstants.TestContractCallData, TestConstants.TestContractByteCode, deposit, gas, gasPrice, nonce, ownerId, ttl, vmVersion);
            contractTx.Fee = contractTxDebug.Fee = 1098660000000000;

            UnsignedTx unsignedTxNative = contractTx.CreateUnsignedTransaction();
            UnsignedTx unsignedTxDebug = contractTxDebug.CreateUnsignedTransaction();
            Assert.AreEqual(unsignedTxNative.TX, unsignedTxDebug.TX);
        }


        [TestMethod]
        public void BuildCallContractTransactionTest()
        {
            Account account = nativeClient.GetAccount(baseKeyPair.PublicKey);
            string callerId = baseKeyPair.PublicKey;
            ushort abiVersion = 1;
            ulong ttl = 20000;
            ulong gas = 1000;
            BigInteger gasPrice = 1000000000;
            ulong nonce = account.Nonce + 1;
            string callContractCalldata = TestConstants.EncodedServiceCall;
            ContractCallTransaction contractTx = nativeClient.CreateContractCallTransaction(abiVersion, callContractCalldata, localDeployedContractId, gas, gasPrice, nonce, callerId, ttl);
            ContractCallTransaction contractTxDebug = debugClient.CreateContractCallTransaction(abiVersion, callContractCalldata, localDeployedContractId, gas, gasPrice, nonce, callerId, ttl);
            contractTx.Fee = contractTxDebug.Fee = 1454500000000000;

            UnsignedTx unsignedTxNative = contractTx.CreateUnsignedTransaction();
            logger.LogInformation("CreateContractTx hash (native unsigned): " + unsignedTxNative.TX);

            UnsignedTx unsignedTxDebug = contractTxDebug.CreateUnsignedTransaction();
            logger.LogInformation("CreateContractTx hash (debug unsigned): " + unsignedTxDebug.TX);
            Assert.AreEqual(unsignedTxNative.TX, unsignedTxDebug.TX);
        }

        [TestMethod]
        public void StaticCallContractOnLocalNode()
        {
            Account account = nativeClient.GetAccount(baseKeyPair.PublicKey);
            ulong nonce = account.Nonce + 1;

            // Compile the call contract
            Calldata calldata = nativeClient.EncodeCalldata(logger, TestConstants.TestContractSourceCode, TestConstants.TestContractFunction, TestConstants.TestContractFunctionParams);
            List<Dictionary<AccountParameter, object>> dict = new List<Dictionary<AccountParameter, object>>();
            dict.Add(new Dictionary<AccountParameter, object> {{AccountParameter.PUBLIC_KEY, baseKeyPair.PublicKey}});
            dict.Add(new Dictionary<AccountParameter, object> {{AccountParameter.PUBLIC_KEY, baseKeyPair.PublicKey}});

            DryRunResults results = nativeClient.DryRunTransactionsAsync(dict, null, new List<UnsignedTx> {CreateUnsignedContractCallTx(nonce, calldata.CallData, null), CreateUnsignedContractCallTx(nonce + 1, calldata.CallData, null)}).TimeoutAsync(TestConstants.NUM_TRIALS_DEFAULT).RunAndUnwrap();
            logger.LogInformation(JsonConvert.SerializeObject(results));
            foreach (DryRunResult result in results.Results)
            {
                Assert.AreEqual("ok", result.Result);
            }
        }

        [TestMethod]
        public void StaticCallContractFailOnLocalNode()
        {
            Calldata calldata = nativeClient.EncodeCalldata(logger, TestConstants.TestContractSourceCode, TestConstants.TestContractFunction, TestConstants.TestContractFunctionParams);
            List<Dictionary<AccountParameter, object>> dict = new List<Dictionary<AccountParameter, object>>();
            dict.Add(new Dictionary<AccountParameter, object> {{AccountParameter.PUBLIC_KEY, baseKeyPair.PublicKey}});

            DryRunResults results = nativeClient.DryRunTransactionsAsync(dict, null, new List<UnsignedTx> {CreateUnsignedContractCallTx(1, calldata.CallData, null)}).TimeoutAsync(TestConstants.NUM_TRIALS_DEFAULT).RunAndUnwrap();
            logger.LogInformation(JsonConvert.SerializeObject(results));
            foreach (DryRunResult result in results.Results)
            {
                Assert.AreEqual("error", result.Result);
            }
        }


        /**
         * min gas is not sufficient, validate this!
         *
         * @param context
         */
        [TestMethod]
        public void CallContractAfterDryRunOnLocalNode()
        {
            Account account = nativeClient.GetAccount(baseKeyPair.PublicKey);
            ulong nonce = account.Nonce + 1;

            // Compile the call contract
            Calldata calldata = nativeClient.EncodeCalldata(logger, TestConstants.TestContractSourceCode, TestConstants.TestContractFunction, TestConstants.TestContractFunctionParams);
            List<Dictionary<AccountParameter, object>> dict = new List<Dictionary<AccountParameter, object>>();
            dict.Add(new Dictionary<AccountParameter, object> {{AccountParameter.PUBLIC_KEY, baseKeyPair.PublicKey}});
            DryRunResults results = nativeClient.PerformDryRunTransactions(logger, dict, null, new List<UnsignedTx> {CreateUnsignedContractCallTx(nonce, calldata.CallData, null)});
            logger.LogInformation("callContractAfterDryRunOnLocalNode: " + JsonConvert.SerializeObject(results));
            foreach (DryRunResult result in results.Results)
            {
                Assert.AreEqual("ok", result.Result);
                var contractAfterDryRunTx = nativeClient.CreateContractCallTransaction(1, calldata.CallData, localDeployedContractId, result.CallObj.GasUsed, result.CallObj.GasPrice, nonce, baseKeyPair.PublicKey, 0);

                UnsignedTx unsignedTxNative = contractAfterDryRunTx.CreateUnsignedTransaction();
                Tx signedTxNative = nativeClient.SignTransaction(unsignedTxNative, baseKeyPair.PrivateKey);

                // post the signed contract call tx
                PostTxResponse postTxResponse = nativeClient.PostTx(logger, signedTxNative);
                Assert.AreEqual(postTxResponse.TXHash, Encoding.ComputeTxHash(signedTxNative.TX));
                logger.LogInformation("CreateContractTx hash: " + postTxResponse.TXHash);

                // get the tx info object to resolve the result
                TxInfoObject txInfoObject = nativeClient.WaitForTxInfoObject(logger, postTxResponse.TXHash);

                // decode the result to json
                JObject json = nativeClient.DecodeCalldata(logger, txInfoObject.CallInfo.ReturnValue, TestConstants.TestContractFunctionSophiaType);
                Assert.AreEqual(TestConstants.TestContractFunctionParam, json.Value<string>("value"));
            }
        }

        private UnsignedTx CreateUnsignedContractCallTx(ulong nonce, string calldata, BigInteger? gasPrice)
        {
            string callerId = baseKeyPair.PublicKey;
            ushort abiVersion = 1;
            ulong ttl = 0;
            ulong gas = 1579000;

            ContractCallTransaction contractTx = nativeClient.CreateContractCallTransaction(abiVersion, calldata, localDeployedContractId, gas, gasPrice ?? Constants.BaseConstants.MINIMAL_GAS_PRICE, nonce, callerId, ttl);

            return contractTx.CreateUnsignedTransaction();
        }

        private void DeployContractNativeOnLocalNode()
        {
            Account account = nativeClient.GetAccount(baseKeyPair.PublicKey);
            string ownerId = baseKeyPair.PublicKey;
            ushort abiVersion = 1;
            ushort vmVersion = 4;
            BigInteger amount = 0;
            BigInteger deposit = 0;
            ulong ttl = 0;
            ulong gas = 1000000;
            BigInteger gasPrice = 2000000000;
            ulong nonce = account.Nonce + 1;
            ContractCreateTransaction contractTx = nativeClient.CreateContractCreateTransaction(abiVersion, amount, TestConstants.TestContractCallData, TestConstants.TestContractByteCode, deposit, gas, gasPrice, nonce, ownerId, ttl, vmVersion);
            CreateContractUnsignedTx unsignedTxNative = contractTx.CreateUnsignedTransaction();
            Tx signedTxNative = nativeClient.SignTransaction(unsignedTxNative, baseKeyPair.PrivateKey);


            logger.LogInformation("CreateContractTx hash (native signed): " + signedTxNative);
            PostTxResponse postTxResponse = nativeClient.PostTx(logger, signedTxNative);
            TxInfoObject txInfoObject = nativeClient.WaitForTxInfoObject(logger, postTxResponse.TXHash);
            localDeployedContractId = txInfoObject.CallInfo.ContractId;
            logger.LogInformation("Deployed contract - hash " + postTxResponse.TXHash + " - " + txInfoObject.TXInfo);
        }

        [TestMethod]
        public void CallContractOnLocalNodeTest()
        {
            Account account = nativeClient.GetAccount(baseKeyPair.PublicKey);
            ulong nonce = account.Nonce + 1;

            Calldata callData = nativeClient.EncodeCalldata(logger, TestConstants.TestContractSourceCode, TestConstants.TestContractFunction, TestConstants.TestContractFunctionParams);

            UnsignedTx unsignedTxNative = CreateUnsignedContractCallTx(nonce, callData.CallData, null);
            Tx signedTxNative = nativeClient.SignTransaction(unsignedTxNative, baseKeyPair.PrivateKey);

            // post the signed contract call tx
            PostTxResponse postTxResponse = nativeClient.PostTx(logger, signedTxNative);
            Assert.AreEqual(postTxResponse.TXHash, Encoding.ComputeTxHash(signedTxNative.TX));
            logger.LogInformation("CreateContractTx hash: " + postTxResponse.TXHash);

            // get the tx info object to resolve the result
            TxInfoObject txInfoObject = nativeClient.WaitForTxInfoObject(logger, postTxResponse.TXHash);


            // decode the result to json
            JObject json = nativeClient.DecodeCalldata(logger, txInfoObject.CallInfo.ReturnValue, TestConstants.TestContractFunctionSophiaType);
            Assert.AreEqual(TestConstants.TestContractFunctionParam, json.Value<string>("value"));
        }
    }
}