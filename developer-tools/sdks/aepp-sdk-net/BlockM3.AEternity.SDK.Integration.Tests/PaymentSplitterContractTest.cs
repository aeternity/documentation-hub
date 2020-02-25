using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Transactions.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    [TestClass]
    [TestCategory("Integration Tests")]
    public class PaymentSplitterContractTest : BaseTest
    {
        private static string paymentSplitterSource;
        private static string localDeployedContractId;

        private static BaseKeyPair initialReceiver1;
        private static BaseKeyPair initialReceiver2;
        private static BaseKeyPair initialReceiver3;

        private static readonly Dictionary<string, int> initialWeights = new Dictionary<string, int>();

        [TestMethod]
        public void PaymentSplitterTest()
        {
            InitPS();
            DeployPaymentSplitter();
            CallPayAndSplitMethod();
        }

        public void InitPS()
        {
            paymentSplitterSource = File.ReadAllText(Path.Combine(ResourcePath, "contracts", "PaymentSplitter.aes"), Encoding.UTF8);
            initialReceiver1 = BaseKeyPair.Generate();
            initialReceiver2 = BaseKeyPair.Generate();
            initialReceiver3 = BaseKeyPair.Generate();
            logger.LogInformation("Initial receiver 1: " + initialReceiver1.PublicKey);
            logger.LogInformation("Initial receiver 2: " + initialReceiver2.PublicKey);
            logger.LogInformation("Initial receiver 3: " + initialReceiver3.PublicKey);
            initialWeights.Add(initialReceiver1.PublicKey, 40);
            initialWeights.Add(initialReceiver2.PublicKey, 40);
            initialWeights.Add(initialReceiver3.PublicKey, 20);
            Assert.AreEqual(3, initialWeights.Count);
        }

        private string GenerateMapParam(Dictionary<string, int> recipientConditions)
        {
            return "{" + string.Join(", ", recipientConditions.Select(a => "[" + a.Key + "] = " + a.Value)) + "}";
        }

        public void DeployPaymentSplitter()
        {
            ByteCode byteCode = nativeClient.Compile(paymentSplitterSource, null, null);
            Calldata calldata = nativeClient.EncodeCalldata(logger, paymentSplitterSource, "init", new List<string> {GenerateMapParam(initialWeights)});
            logger.LogInformation("contract bytecode: " + byteCode.Bytecode);
            logger.LogInformation("contract calldata: " + calldata.CallData);
            Account account = nativeClient.GetAccount(baseKeyPair.PublicKey);
            string ownerId = baseKeyPair.PublicKey;

            ushort abiVersion = 1;
            ushort vmVersion = 4;
            BigInteger amount = 0;
            BigInteger deposit = 0;
            ulong ttl = 0;
            ulong gas = 4800000;
            BigInteger gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE;
            ulong nonce = account.Nonce + 1;

            ContractCreateTransaction contractTx = nativeClient.CreateContractCreateTransaction(abiVersion, amount, calldata.CallData, byteCode.Bytecode, deposit, gas, gasPrice, nonce, ownerId, ttl, vmVersion);

            UnsignedTx unsignedTx = contractTx.CreateUnsignedTransaction();
            logger.LogInformation("Unsigned Tx - hash - dryRun: " + unsignedTx.TX);
            Tx signedTxNative = nativeClient.SignTransaction(unsignedTx, baseKeyPair.PrivateKey);
            logger.LogInformation("CreateContractTx hash (native signed): " + signedTxNative);

            PostTxResponse postTxResponse = nativeClient.PostTx(logger, signedTxNative);
            TxInfoObject txInfoObject = nativeClient.WaitForTxInfoObject(logger, postTxResponse.TXHash);
            localDeployedContractId = txInfoObject.CallInfo.ContractId;
            logger.LogInformation("Deployed contract - hash " + postTxResponse.TXHash + " - " + txInfoObject);
            if ("revert".Equals(txInfoObject.CallInfo.ReturnType))
            {
                Assert.Fail("transaction reverted: " + nativeClient.DecodeCalldata(logger, txInfoObject.CallInfo.ReturnValue, "string"));
            }
        }

        public void CallPayAndSplitMethod()
        {
            Account account = nativeClient.GetAccount(baseKeyPair.PublicKey);
            BigInteger balanceRecipient1;
            BigInteger balanceRecipient2;
            BigInteger balanceRecipient3;
            try
            {
                balanceRecipient1 = nativeClient.GetAccount(initialReceiver1.PublicKey).Balance;
                balanceRecipient2 = nativeClient.GetAccount(initialReceiver2.PublicKey).Balance;
                balanceRecipient3 = nativeClient.GetAccount(initialReceiver3.PublicKey).Balance;
                // if one of the accounts wasn't active we get an error and know that the
                // accounts
                // don't have any balance
            }
            catch (Exception)
            {
                balanceRecipient1 = 0;
                balanceRecipient2 = 0;
                balanceRecipient3 = 0;
            }

            ulong nonce = account.Nonce + 1;
            decimal paymentValue = "1".ToAettos(Unit.AE);
            Calldata calldata = nativeClient.EncodeCalldata(logger, paymentSplitterSource, "payAndSplit", null);
            logger.LogInformation("Contract ID: " + localDeployedContractId);
            List<Dictionary<AccountParameter, object>> accounts = new List<Dictionary<AccountParameter, object>>();
            Dictionary<AccountParameter, object> ac = new Dictionary<AccountParameter, object>() {{AccountParameter.PUBLIC_KEY, baseKeyPair.PublicKey}};
            accounts.Add(ac);
            DryRunResults dryRunResults = nativeClient.PerformDryRunTransactions(logger, accounts, null, new List<UnsignedTx>() {nativeClient.CreateUnsignedContractCallTx(logger, baseKeyPair.PublicKey, nonce, calldata.CallData, null, localDeployedContractId, new BigInteger(paymentValue))});
            logger.LogInformation("callContractAfterDryRunOnLocalNode: " + JsonConvert.SerializeObject(dryRunResults));
            Assert.AreEqual(1, dryRunResults.Results.Count);
            DryRunResult dryRunResult = dryRunResults.Results.First();
            Assert.AreEqual("ok", dryRunResult.Result);
            ContractCallTransaction contractAfterDryRunTx = nativeClient.CreateContractCallTransaction(1, calldata.CallData, localDeployedContractId, dryRunResult.CallObj.GasUsed, dryRunResult.CallObj.GasPrice, nonce, baseKeyPair.PublicKey, 0);
            contractAfterDryRunTx.Model.Amount = new BigInteger(paymentValue);

            UnsignedTx unsignedTxNative = contractAfterDryRunTx.CreateUnsignedTransaction();
            Tx signedTxNative = nativeClient.SignTransaction(unsignedTxNative, baseKeyPair.PrivateKey);

            // post the signed contract call tx
            PostTxResponse postTxResponse = nativeClient.PostTx(logger, signedTxNative);
            Assert.AreEqual(postTxResponse.TXHash, Utils.Encoding.ComputeTxHash(signedTxNative.TX));
            logger.LogInformation("CreateContractTx hash: " + postTxResponse.TXHash);

            // we wait until the tx is available and the payment should have been splitted
            TxInfoObject txInfoObject = nativeClient.WaitForTxInfoObject(logger, postTxResponse.TXHash);
            logger.LogInformation("PayAndSplit transaction - hash " + postTxResponse.TXHash + " - " + txInfoObject);
            if ("revert".Equals(txInfoObject.CallInfo.ReturnType))
            {
                Assert.Fail("transaction reverted: " + nativeClient.DecodeCalldata(logger, txInfoObject.CallInfo.ReturnValue, "string"));
            }

            Assert.AreEqual(balanceRecipient1 + new BigInteger(paymentValue * 0.4m), nativeClient.GetAccount(initialReceiver1.PublicKey).Balance);
            Assert.AreEqual(balanceRecipient2 + new BigInteger(paymentValue * 0.4m), nativeClient.GetAccount(initialReceiver2.PublicKey).Balance);
            Assert.AreEqual(balanceRecipient3 + new BigInteger(paymentValue * 0.2m), nativeClient.GetAccount(initialReceiver3.PublicKey).Balance);
        }
    }
}