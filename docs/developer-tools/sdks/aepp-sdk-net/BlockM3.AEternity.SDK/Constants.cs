using System.Collections.Generic;
using BlockM3.AEternity.SDK.Generated.Models;

namespace BlockM3.AEternity.SDK
{
    public static class Constants
    {
        public struct ApiIdentifiers
        {
            // API identifiers
            // https://github.com/aeternity/protocol/blob/master/epoch/api/api_encoding.md#encoding-scheme-for-api-identifiers-and-byte-arrays

            // base58
            public const string ACCOUNT_PUBKEY = "ak"; // base58 Account pubkey

            public const string BLOCK_PROOF_OF_FRAUD_HASH = "bf"; // base58 Block Proof of Fraud hash

            public const string BLOCK_STATE_HASH = "bs"; // base58 Block State hash

            public const string BLOCK_TRANSACTION_HASH = "bx"; // base58 Block transaction hash

            public const string CHANNEL = "ch"; // base58 Channel

            public const string COMMITMENT = "cm"; // base58 Commitment

            public const string CONTRACT_PUBKEY = "ct"; // base58 Contract pubkey

            public const string KEY_BLOCK_HASH = "kh"; // base58 Key block hash

            public const string MICRO_BLOCK_HASH = "mh"; // base58 Micro block hash

            public const string NAME = "nm"; // base58 Name

            public const string ORACLE_PUBKEY = "ok"; // base58 Oracle pubkey

            public const string ORACLE_QUERY_ID = "oq"; // base58 Oracle query id

            public const string PEER_PUBKEY = "pp"; // base58 Peer pubkey

            public const string SIGNATURE = "sg"; // base58 Signature

            public const string TRANSACTION_HASH = "th"; // base58 Transaction hash

            // Base 64
            public const string CONTRACT_BYTE_ARRAY = "cb"; // base64 Contract byte array

            public const string ORACLE_RESPONSE = "or"; // base64 Oracle response

            public const string ORACLE_QUERY = "ov"; // base64 Oracle query

            public const string PROOF_OF_INCLUSION = "pi"; // base64 Proof of Inclusion

            public const string STATE_TREES = "ss"; // base64 State trees

            public const string STATE = "st"; // base64 State

            public const string TRANSACTION = "tx"; // base64 Transaction

            // Indentifiers with base58
            public static string[] IDENTIFIERS_B58 = {ACCOUNT_PUBKEY, BLOCK_PROOF_OF_FRAUD_HASH, BLOCK_STATE_HASH, BLOCK_TRANSACTION_HASH, CHANNEL, COMMITMENT, CONTRACT_PUBKEY, KEY_BLOCK_HASH, MICRO_BLOCK_HASH, NAME, ORACLE_PUBKEY, ORACLE_QUERY_ID, PEER_PUBKEY, SIGNATURE, TRANSACTION_HASH};

            public static HashSet<string> IDENTIFIERS_B58_LIST = new HashSet<string>(IDENTIFIERS_B58);

            // Indentifiers with base64
            public static string[] IDENTIFIERS_B64 = {CONTRACT_BYTE_ARRAY, ORACLE_RESPONSE, ORACLE_QUERY, PROOF_OF_INCLUSION, STATE_TREES, STATE, TRANSACTION};

            public static HashSet<string> IDENTIFIERS_B64_LIST = new HashSet<string>(IDENTIFIERS_B64);
        }

        public struct BaseConstants
        {
            public const ulong TX_TTL = 0;

            public const ulong NAME_MAX_TLL = 36000;
            public const ulong NAME_MAX_CLIENT_TTL = 84600;
            public const ulong NAME_CLIENT_TTL = NAME_MAX_CLIENT_TTL;
            public const ulong NAME_TTL = 500;

            public const string PREFIX_ZERO_X = "0x";

            // https://github.com/bitcoin/bips/blob/master/bip-0044.mediawiki
            public const int HD_CHAIN_PURPOSE = 44;

            public const int HD_CHAIN_CODE_AETERNITY = 457;

            // https://github.com/aeternity/protocol/blob/master/consensus/consensus.md#gas
            public const long BASE_GAS = 15000;

            public const long GAS_PER_BYTE = 20;

            // https://github.com/aeternity/protocol/blob/master/consensus/consensus.md#common-fields-for-transactions
            public const long MINIMAL_GAS_PRICE = 1000000000;

            public const long CONTRACT_GAS = 10000;


            public const ushort ORACLE_VM_VERSION = 0;
            public const TTLType ORACLE_TTL_TYPE = TTLType.Delta;
            public const ulong ORACLE_QUERY_FEE = 0;
            public const ulong ORACLE_TTL_VALUE = 500;
            public const ulong ORACLE_QUERY_TTL_VALUE = 10;
            public const ulong ORACLE_RESPONSE_TTL_VALUE = 10;


            public const ulong FEE = 0;

            // average time between key-blocks in minutes
            public const long KEY_BLOCK_INTERVAL = 3;

            public const string AETERNITY_MESSAGE_PREFIX = "æternity Signed Message:\n";

            public const int MAX_MESSAGE_LENGTH = 0xFD;

            // the default testnet url
            public const string DEFAULT_TESTNET_URL = "https://sdk-testnet.aepps.com/v2";

            // the default testnet contract url
            public const string DEFAULT_TESTNET_CONTRACT_URL = "https://compiler.aepps.com";

            public const ushort VM_FORTUNA = 4;
            public const ushort VM_FATE = 5;
            public const ushort VM_SOFIA_LIMA = 6;

            public const ushort VM_VERSION = VM_FATE;

            public const ushort ABI_SOPHIA = 1;
            public const ushort ABI_SOLIDITY = 2;
            public const ushort ABI_FATE = 3;

            public const ushort ABI_VERSION = ABI_FATE;
        }

        public struct Network
        {
            public const string DEVNET = "ae_devnet";
            public const string TESTNET = "ae_uat";
            public const string MAINNET = "ae_mainnet";
        }

        public struct SerializationTags
        {
            // RLP version number
            // https://github.com/aeternity/protocol/blob/master/serializations.md#binary-serialization
            public const int VSN = 1;

            // Tag constant for ids (type uint8)
            // see
            // https://github.com/aeternity/protocol/blob/master/serializations.md#the-id-type
            // <<Tag:1/unsigned-integer-unit:8, Hash:32/binary-unit:8>>
            public const int ID_TAG_ACCOUNT = 1;

            public const int ID_TAG_NAME = 2;

            public const int ID_TAG_COMMITMENT = 3;

            public const int ID_TAG_ORACLE = 4;

            public const int ID_TAG_CONTRACT = 5;

            public const int ID_TAG_CHANNEL = 6;

            // Object tags
            // see
            // https://github.com/aeternity/protocol/blob/master/serializations.md#binary-serialization

            public const int OBJECT_TAG_ACCOUNT = 10;

            public const int OBJECT_TAG_SIGNED_TRANSACTION = 11;

            public const int OBJECT_TAG_SPEND_TRANSACTION = 12;

            public const int OBJECT_TAG_ORACLE = 20;

            public const int OBJECT_TAG_ORACLE_QUERY = 21;

            public const int OBJECT_TAG_ORACLE_REGISTER_TRANSACTION = 22;

            public const int OBJECT_TAG_ORACLE_QUERY_TRANSACTION = 23;

            public const int OBJECT_TAG_ORACLE_RESPONSE_TRANSACTION = 24;

            public const int OBJECT_TAG_ORACLE_EXTEND_TRANSACTION = 25;

            public const int OBJECT_TAG_NAME_SERVICE_NAME = 30;

            public const int OBJECT_TAG_NAME_SERVICE_COMMITMENT = 31;

            public const int OBJECT_TAG_NAME_SERVICE_CLAIM_TRANSACTION = 32;

            public const int OBJECT_TAG_NAME_SERVICE_PRECLAIM_TRANSACTION = 33;

            public const int OBJECT_TAG_NAME_SERVICE_UPDATE_TRANSACTION = 34;

            public const int OBJECT_TAG_NAME_SERVICE_REVOKE_TRANSACTION = 35;

            public const int OBJECT_TAG_NAME_SERVICE_TRANSFER_TRANSACTION = 36;

            public const int OBJECT_TAG_CONTRACT = 40;

            public const int OBJECT_TAG_CONTRACT_CALL = 41;

            public const int OBJECT_TAG_CONTRACT_CREATE_TRANSACTION = 42;

            public const int OBJECT_TAG_CONTRACT_CALL_TRANSACTION = 43;

            public const int OBJECT_TAG_CHANNEL_CREATE_TRANSACTION = 50;

            public const int OBJECT_TAG_CHANNEL_DEPOSIT_TRANSACTION = 51;

            public const int OBJECT_TAG_CHANNEL_WITHDRAW_TRANSACTION = 52;

            public const int OBJECT_TAG_CHANNEL_FORCE_PROGRESS_TRANSACTION = 521;

            public const int OBJECT_TAG_CHANNEL_CLOSE_MUTUAL_TRANSACTION = 53;

            public const int OBJECT_TAG_CHANNEL_CLOSE_SOLO_TRANSACTION = 54;

            public const int OBJECT_TAG_CHANNEL_SLASH_TRANSACTION = 55;

            public const int OBJECT_TAG_CHANNEL_SETTLE_TRANSACTION = 56;

            public const int OBJECT_TAG_CHANNEL_OFF_CHAIN_TRANSACTION = 57;

            public const int OBJECT_TAG_CHANNEL_OFF_CHAIN_UPDATE_TRANSFER = 570;

            public const int OBJECT_TAG_CHANNEL_OFF_CHAIN_UPDATE_DEPOSIT = 571;

            public const int OBJECT_TAG_CHANNEL_OFF_CHAIN_UPDATE_WITHDRAWAL = 572;

            public const int OBJECT_TAG_CHANNEL_OFF_CHAIN_UPDATE_CREATE_CONTRACT = 573;

            public const int OBJECT_TAG_CHANNEL_OFF_CHAIN_UPDATE_CALL_CONTRACT = 574;

            public const int OBJECT_TAG_CHANNEL = 58;

            public const int OBJECT_TAG_CHANNEL_SNAPSHOT_TRANSACTION = 59;

            public const int OBJECT_TAG_POI = 60;

            public const int OBJECT_TAG_MICRO_BODY = 101;

            public const int OBJECT_TAG_LIGHT_MICRO_BLOCK = 102;
        }
    }
}