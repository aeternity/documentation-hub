using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.Contracts
{
    public class ContractCreateTransaction : Transaction<CreateContractUnsignedTx, ContractCreateTx>
    {
        internal ContractCreateTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }


        protected override Task<CreateContractUnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugContractCreateAsync(Model, token);
        }

        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_CONTRACT_CREATE_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.OwnerId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Nonce);
            enc.AddByteArray(Encoding.DecodeCheckWithIdentifier(Model.Code));
            enc.AddNumber(CalculateVersion());
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            enc.AddNumber(Model.Deposit);
            enc.AddNumber(Model.Amount);
            enc.AddNumber(Model.Gas);
            enc.AddNumber(Model.GasPrice);
            enc.AddByteArray(Encoding.DecodeCheckWithIdentifier(Model.CallData));
            return enc.Encode();
        }
        public override async Task<CreateContractUnsignedTx> CreateUnsignedTransactionAsync(bool nativeMode, long minimalGasPrice, CancellationToken token = default(CancellationToken))
        {
            CreateContractUnsignedTx tx = await base.CreateUnsignedTransactionAsync(nativeMode, minimalGasPrice, token);
            if (nativeMode)
                tx.ContractId = Encoding.EncodeContractId(Model.OwnerId, Model.Nonce ?? 0);
            return tx;
        }
        private BigInteger CalculateVersion()
        {
            try
            {
                byte[] vmversion;
                byte[] abiversion;

                if (BitConverter.IsLittleEndian)
                {
                    vmversion = BitConverter.GetBytes(Model.VmVersion).Reverse().ToArray();
                    abiversion = BitConverter.GetBytes(Model.AbiVersion).Reverse().ToArray();
                }
                else
                {
                    vmversion = BitConverter.GetBytes(Model.VmVersion);
                    abiversion = BitConverter.GetBytes(Model.AbiVersion);
                }

                string version = BitConverter.ToString(vmversion.Concatenate(abiversion)).Replace("-", "");
                return BigInteger.Parse(version, NumberStyles.HexNumber);
            }
            catch
            {
                _logger.LogError($"Error occured calculating version from parameters vmVersion {Model.VmVersion} and abiVersion {Model.AbiVersion}");
                return 0;
            }
        }
    }
}