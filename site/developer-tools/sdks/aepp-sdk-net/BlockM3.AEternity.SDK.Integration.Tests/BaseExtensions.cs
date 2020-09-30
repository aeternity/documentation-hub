using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Transactions.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    public static class BaseExtensions
    {
        public static PostTxResponse PostTx(this FlatClient client, ILogger logger, Tx signedTx)
        {
            PostTxResponse postTxResponse = client.PostTransaction(signedTx);
            logger.LogInformation("Post tx hash :" + postTxResponse.TXHash);
            GenericSignedTx txValue = client.WaitForTxMined(logger, postTxResponse.TXHash);
            logger.LogInformation($"Transaction of type {txValue.TX.GetType().Name} is mined at block {txValue.BlockHash} with height {txValue.BlockHeight}");
            return postTxResponse;
        }

        public static TxInfoObject WaitForTxInfoObject(this FlatClient client, ILogger logger, string txHash)
        {
            return client.GetTransactionInfoByHash(txHash);
        }

        public static GenericSignedTx WaitForTxMined(this FlatClient client, ILogger logger, string txHash)
        {
            long blockHeight = -1;
            GenericSignedTx minedTx = null;
            int doneTrials = 1;

            while (blockHeight == -1 && doneTrials < TestConstants.NUM_TRIALS_DEFAULT)
            {
                minedTx = client.GetTransactionByHash(txHash);
                if (minedTx.BlockHeight > 1)
                {
                    logger.LogDebug("Mined tx: " + minedTx.TX);
                    blockHeight = minedTx.BlockHeight;
                }
                else
                {
                    logger.LogWarning($"Transaction not mined yet, trying again in 1 second ({doneTrials} of {TestConstants.NUM_TRIALS_DEFAULT})...");
                    Thread.Sleep(1000);
                    doneTrials++;
                }
            }

            if (blockHeight == -1)
                throw new OperationCanceledException($"Transaction {txHash} was not mined after {doneTrials} trials, aborting");
            return minedTx;
        }


        public static Calldata EncodeCalldata(this FlatClient client, ILogger logger, string contractSourceCode, string contractFunction, List<string> contractFunctionParams)
        {
            return client.EncodeCallData(contractSourceCode, contractFunction, contractFunctionParams);
        }

        public static JObject DecodeCalldata(this FlatClient client, ILogger logger, string encodedValue, string sophiaReturnType)
        {
            // decode the result to json
            SophiaJsonData sophiaJsonData = client.DecodeCallData(encodedValue, sophiaReturnType);
            return sophiaJsonData.Data as JObject;
        }

        public static DryRunResults PerformDryRunTransactions(this FlatClient client, ILogger logger, List<Dictionary<AccountParameter, object>> accounts, BigInteger? block, List<UnsignedTx> unsignedTxes)
        {
            return client.DryRunTransactions(accounts, block, unsignedTxes);
        }

        public static UnsignedTx CreateUnsignedContractCallTx(this FlatClient client, ILogger logger, string callerId, ulong nonce, string calldata, BigInteger? gasPrice, string contractId, BigInteger amount)
        {
            ushort abiVersion = 1;
            ulong ttl = 0;
            ulong gas = 1579000;
            ContractCallTransaction contractTx = client.CreateContractCallTransaction(abiVersion, calldata, contractId, gas, gasPrice ?? Constants.BaseConstants.MINIMAL_GAS_PRICE, nonce, callerId, ttl);
            contractTx.Model.Amount = amount;
            return contractTx.CreateUnsignedTransaction();
        }
    }
}