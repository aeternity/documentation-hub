using System.Numerics;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Transactions;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    [TestClass]
    [TestCategory("Integration Tests")]
    public class TransactionSpendApiTest : BaseTest
    {
        /**
         * create an unsigned native spend transaction
         *
         */

        [TestMethod]
        public void BuildNativeSpendTransactionTest()
        {
            string sender = BaseKeyPair.Generate().PublicKey;
            string recipient = BaseKeyPair.Generate().PublicKey;
            BigInteger amount = 1000;
            string payload = "";
            ulong ttl = 100;
            ulong nonce = 5;

            SpendTransaction spendTx = nativeClient.CreateSpendTransaction(sender, recipient, amount, payload, ttl, nonce);
            SpendTransaction spendTxDebug = debugClient.CreateSpendTransaction(sender, recipient, amount, payload, ttl, nonce);
            UnsignedTx unsignedTxNative = spendTx.CreateUnsignedTransaction();
            UnsignedTx unsignedTx = spendTxDebug.CreateUnsignedTransaction();
            Assert.AreEqual(unsignedTx.TX, unsignedTxNative.TX);
        }

        [TestMethod]
        public void PostSpendTxTest()
        {
            // get the currents accounts nonce in case a transaction is already
            // created and increase it by one
            Account account = nativeClient.GetAccount(baseKeyPair.PublicKey);

            BaseKeyPair kp = BaseKeyPair.Generate();

            string recipient = kp.PublicKey;
            BigInteger amount = 1000000000000000000;
            string payload = "";
            ulong ttl = 0;
            ulong nonce = account.Nonce + 1;
            SpendTransaction spendTx = nativeClient.CreateSpendTransaction(baseKeyPair.PublicKey, recipient, amount, payload, ttl, nonce);
            UnsignedTx unsignedTxNative = spendTx.CreateUnsignedTransaction();
            Tx signedTx = nativeClient.SignTransaction(unsignedTxNative, baseKeyPair.PrivateKey);
            PostTxResponse txResponse = nativeClient.PostTransaction(signedTx);
            logger.LogInformation("SpendTx hash: " + txResponse.TXHash);
            Assert.AreEqual(txResponse.TXHash, Encoding.ComputeTxHash(signedTx.TX));
        }
    }
}