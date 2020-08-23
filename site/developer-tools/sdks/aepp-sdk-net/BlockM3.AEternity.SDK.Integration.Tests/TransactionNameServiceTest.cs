using System;
using System.Collections.Generic;
using System.Numerics;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Transactions.NameService;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    [TestClass]
    [TestCategory("Integration Tests")]
    public class TransactionNameServiceTest : BaseTest
    {
        private readonly Random random = new Random();

        private string InvalidDomain;
        private string validDomain;


        [TestMethod]
        public void TransactionNameServiceTests()
        {
            InvalidDomain = TestConstants.DOMAIN + random.Next();
            validDomain = InvalidDomain + TestConstants.NAMESPACE;
            BuildNativeNamePreclaimTransaction();
            PostNameClaimTx();
            PostRevokeTx();
            PostUpdateTx();
        }

        /**
         * create an unsigned native namepreclaim transaction
         *
         * 
         */
        public void BuildNativeNamePreclaimTransaction()
        {
            string sender = BaseKeyPair.Generate().PublicKey;
            BigInteger salt = Crypto.GenerateNamespaceSalt();
            ulong nonce = 1;
            ulong ttl = 100;
            NamePreclaimTransaction namePreclaimTx = nativeClient.CreateNamePreClaimTransaction(sender, validDomain, salt, nonce, ttl);
            NamePreclaimTransaction namePreclaimTxDebug = debugClient.CreateNamePreClaimTransaction(sender, validDomain, salt, nonce, ttl);
            UnsignedTx unsignedTxNative = namePreclaimTx.CreateUnsignedTransaction();
            UnsignedTx unsignedTx = namePreclaimTxDebug.CreateUnsignedTransaction();
            Assert.AreEqual(unsignedTxNative.TX, unsignedTx.TX);
        }

        public void PostNameClaimTx()
        {
            BaseKeyPair keyPair = new BaseKeyPair(TestConstants.BENEFICIARY_PRIVATE_KEY);
            Account account = nativeClient.GetAccount(keyPair.PublicKey);
            BigInteger salt = Crypto.GenerateNamespaceSalt();
            ulong nonce = account.Nonce + 1;
            ulong ttl = 0;
            NamePreclaimTransaction namePreclaimTx = nativeClient.CreateNamePreClaimTransaction(keyPair.PublicKey, validDomain, salt, nonce, ttl);
            UnsignedTx unsignedTx = namePreclaimTx.CreateUnsignedTransaction();


            NamePreclaimTransaction namePreclaimTxDebug = debugClient.CreateNamePreClaimTransaction(keyPair.PublicKey, validDomain, salt, nonce, ttl);
            UnsignedTx unsignedTxDebug = namePreclaimTxDebug.CreateUnsignedTransaction();
            Assert.AreEqual(unsignedTxDebug.TX, unsignedTx.TX);


            Tx signedTx = nativeClient.SignTransaction(unsignedTx, keyPair.PrivateKey);


            logger.LogInformation("Signed NamePreclaimTx: " + signedTx.TX);
            PostTxResponse postTxResponse = nativeClient.PostTx(logger, signedTx);
            logger.LogInformation("NamePreclaimTx hash: " + postTxResponse.TXHash);
            Assert.AreEqual(postTxResponse.TXHash, Encoding.ComputeTxHash(signedTx.TX));

            NameClaimTransaction nameClaimTx = nativeClient.CreateNameClaimTransaction(keyPair.PublicKey, validDomain, salt, nonce + 1, ttl);
            UnsignedTx unsignedClaimTx = nameClaimTx.CreateUnsignedTransaction();

            NameClaimTransaction nameClaimTxDebug = debugClient.CreateNameClaimTransaction(keyPair.PublicKey, validDomain, salt, nonce + 1, ttl);
            UnsignedTx unsignedClaimTxDebug = nameClaimTxDebug.CreateUnsignedTransaction();


            Assert.AreEqual(unsignedClaimTxDebug.TX, unsignedClaimTx.TX);


            Tx signedClaimTx = nativeClient.SignTransaction(unsignedClaimTx, keyPair.PrivateKey);
            logger.LogInformation("Signed NameClaimTx: " + signedClaimTx.TX);
            postTxResponse = nativeClient.PostTx(logger, signedClaimTx);
            logger.LogInformation($"Using namespace {validDomain} and salt {salt} for committmentId {Encoding.GenerateCommitmentHash(validDomain, salt)}");
            logger.LogInformation("NameClaimTx hash: " + postTxResponse.TXHash);
        }


        public void PostUpdateTx()
        {
            BaseKeyPair keyPair = new BaseKeyPair(TestConstants.BENEFICIARY_PRIVATE_KEY);
            Account account = nativeClient.GetAccount(keyPair.PublicKey);
            ulong nonce = account.Nonce + 1;
            BigInteger salt = Crypto.GenerateNamespaceSalt();
            ulong ttl = 0;
            string domain = TestConstants.DOMAIN + random.Next() + TestConstants.NAMESPACE;

            /** create a new namespace to update later */
            NamePreclaimTransaction namePreclaimTx = nativeClient.CreateNamePreClaimTransaction(keyPair.PublicKey, domain, salt, nonce, ttl);
            UnsignedTx unsignedTx = namePreclaimTx.CreateUnsignedTransaction();
            Tx signedTx = nativeClient.SignTransaction(unsignedTx, keyPair.PrivateKey);
            PostTxResponse postTxResponse = nativeClient.PostTx(logger, signedTx);
            Assert.AreEqual(postTxResponse.TXHash, Encoding.ComputeTxHash(signedTx.TX));
            NameClaimTransaction nameClaimTx = nativeClient.CreateNameClaimTransaction(keyPair.PublicKey, domain, salt, nonce + 1, ttl);
            UnsignedTx unsignedClaimTx = nameClaimTx.CreateUnsignedTransaction();
            Tx signedClaimTx = nativeClient.SignTransaction(unsignedClaimTx, keyPair.PrivateKey);
            PostTxResponse postClaimTxResponse = nativeClient.PostTx(logger, signedClaimTx);
            NameEntry nameEntry = nativeClient.GetNameId(domain);
            ulong initialTTL = nameEntry.Ttl;
            logger.LogInformation($"Created namespace {domain} with salt {salt} and nameEntry {nameEntry} in tx {postClaimTxResponse.TXHash} for update test");
            /** finished creating namespace */
            ulong nameTtl = 10000;
            ulong clientTtl = 50;
            account = nativeClient.GetAccount(keyPair.PublicKey);
            nonce = account.Nonce + 1;
            NameUpdateTransaction nameUpdateTx = nativeClient.CreateNameUpdateTransaction(keyPair.PublicKey, nameEntry.Id, nonce, ttl, clientTtl, nameTtl, new List<NamePointer>());
            UnsignedTx unsignedUpdateTx = nameUpdateTx.CreateUnsignedTransaction();
            Tx signedUpdateTx = nativeClient.SignTransaction(unsignedUpdateTx, keyPair.PrivateKey);
            PostTxResponse postUpdateTxResponse = nativeClient.PostTx(logger, signedUpdateTx);
            Assert.AreEqual(postUpdateTxResponse.TXHash, Encoding.ComputeTxHash(signedUpdateTx.TX));
            nameEntry = nativeClient.GetNameId(domain);
            logger.LogInformation($"Updated namespace {domain} with salt {salt} and nameEntry {nameEntry} in tx {postClaimTxResponse.TXHash} for update test");

            ulong updatedTTL = nameEntry.Ttl;
            // subtract 40000 because initial default ttl is 50000 and updated ttl was 10000
            ulong diffTtl = initialTTL - updatedTTL;
            Assert.IsTrue(diffTtl <= 40000);
            if (diffTtl < 40000)
                logger.LogInformation($"Diff of Ttl is {diffTtl}, this happens when meanwhile new blocks are mined");
        }

        /** @param context */

        public void PostRevokeTx()
        {
            BaseKeyPair keyPair = new BaseKeyPair(TestConstants.BENEFICIARY_PRIVATE_KEY);
            string nameId = nativeClient.GetNameId(validDomain).Id;
            Account account = nativeClient.GetAccount(keyPair.PublicKey);
            ulong nonce = account.Nonce + 1;
            ulong ttl = 0;
            NameRevokeTransaction nameRevokeTx = nativeClient.CreateNameRevokeTransaction(keyPair.PublicKey, nameId, nonce, ttl);
            UnsignedTx unsignedTx = nameRevokeTx.CreateUnsignedTransaction();
            Tx signedTx = nativeClient.SignTransaction(unsignedTx, keyPair.PrivateKey);
            logger.LogInformation("Signed NameRevokeTx: " + signedTx.TX);
            PostTxResponse postTxResponse = nativeClient.PostTx(logger, signedTx);
            logger.LogInformation("NameRevokeTx hash: " + postTxResponse.TXHash);
            Assert.AreEqual(postTxResponse.TXHash, Encoding.ComputeTxHash(signedTx.TX));
            Assert.ThrowsException<ApiException<Error>>(() => nativeClient.GetNameIdAsync(validDomain).TimeoutAsync(TestConstants.NUM_TRIALS_DEFAULT).RunAndUnwrap(), "Not Found");
            logger.LogInformation("Validated, that namespace {validDomain} is revoked");
        }
    }
}