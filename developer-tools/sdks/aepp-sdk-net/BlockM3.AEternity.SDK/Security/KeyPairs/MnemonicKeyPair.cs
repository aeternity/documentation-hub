using System;
using System.Collections.Generic;
using System.Linq;
using BlockM3.AEternity.SDK.Exceptions;
using BlockM3.AEternity.SDK.Utils;
using NBitcoin;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Security.KeyPairs
{
    /**
     * this class wrapps a mnemonic additionally to the rawKeyPair it contains the list of mnemonic seed
     * words, the generated {@link RawKeyPair} and the {@link DeterministicHierarchy} which build the
     * base for generating a hierarchical deterministic wallet.
     *
     * <p>The deterministicHierarchy object can either be created from the root (master) or a derived
     * key according to the tree structure stated in <a
     * href=https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki#Master_key_generation>BIP32</a>
     */
    public class MnemonicKeyPair : RawKeyPair
    {
        public MnemonicKeyPair(RawKeyPair rawKeyPair, List<string> mnemonicSeedWords, DeterministicKeyPair extKeyPair) : base(rawKeyPair.PublicKey, rawKeyPair.PrivateKey)
        {
            MnemonicSeedWords = mnemonicSeedWords;
            DeterministicKeyPair = extKeyPair;
        }


        public List<string> MnemonicSeedWords { get; set; }

        public DeterministicKeyPair DeterministicKeyPair { get; set; }

        public RawKeyPair ToRawKeyPair() => new RawKeyPair(PublicKey, PrivateKey);


        public static MnemonicKeyPair Generate(string mnemonicSeedPassword, int entropysize)
        {
            try
            {
                if (mnemonicSeedPassword == null)
                    mnemonicSeedPassword = "";
                // generate a random byte array
                byte[] entropy = Crypto.GenerateSalt(entropysize);
                // generate the list of mnemonic seed words based on the random byte array
                List<string> mnemonicSeedWords = new Mnemonic(Wordlist.English, entropy).Words.ToList();
                return RecoverMnemonicKeyPair(mnemonicSeedWords, mnemonicSeedPassword);
            }
            catch (Exception e)
            {
                throw new AException($"An error occured generating keyPair {e.Message}", e);
            }
        }

        public MnemonicKeyPair DerivedKey(bool hardened, string path = null)
        {
            DeterministicKeyPair master = DeterministicKeyPair;
            // check if we really have the masterKey at hand
            //mpiva limitation no go back in history
            if (master.Depth != 0)
            {
                throw new AException("Given mnemonicKeyPair object does not contain the master key");
            }
            /**
             * following the BIP32 specification create the following tree purpose -> coin -> account ->
             * external chain -> child address
             */

            /**
             * always set path for purpose {@link BaseConstants.HD_CHAIN_PURPOSE}, coin {@link
             * BaseConstants.HD_CHAIN_CODE_AETERNITY}
             */
            string pathToDerivedKey = "m/" + Constants.BaseConstants.HD_CHAIN_PURPOSE + "'/" + Constants.BaseConstants.HD_CHAIN_CODE_AETERNITY + "'";


            /** if no arguments are given, set default account and external chain (0h, 0h) */
            if (string.IsNullOrEmpty(path))
            {
                pathToDerivedKey += "/0'/0'";
                /** in case arguments are given - add warning */
            }
            else
            {
                pathToDerivedKey += "/" + path;
                pathToDerivedKey = pathToDerivedKey.Replace("//", "/");
            }

            DeterministicKeyPair nextChildDeterministicKeyPair = master.DeriveNextChild(pathToDerivedKey, hardened);

            // derive a new child
            RawKeyPair childRawKeyPair = new RawKeyPair(Hex.ToHexString(nextChildDeterministicKeyPair.PrivateKey.ToBytes()));

            return new MnemonicKeyPair(childRawKeyPair, MnemonicSeedWords, nextChildDeterministicKeyPair);
        }


        public static MnemonicKeyPair RecoverMnemonicKeyPair(List<string> mnemonicSeedWords, string mnemonicSeedPassword)
        {
            if (mnemonicSeedPassword == null)
                mnemonicSeedPassword = "";

            Mnemonic mn = new Mnemonic(string.Join(" ", mnemonicSeedWords), Wordlist.English);
            // generate the seed from words and password
            byte[] seed = mn.DeriveSeed(mnemonicSeedPassword);
            // generate the master key from the seed using bitcoinj implementation of hd
            // wallets
            DeterministicKeyPair master = new DeterministicKeyPair(seed);
            RawKeyPair generatedKeyPair = new RawKeyPair(Hex.ToHexString(master.PrivateKey.ToBytes()));
            return new MnemonicKeyPair(generatedKeyPair, mnemonicSeedWords, master);
        }

        public string CreateHDKeystore()
        {
            Dictionary<string, string> keystore = new Dictionary<string, string>();
            keystore.Add("publicKey", GetPublicKey());
            keystore.Add("mnemonicSeedWords", string.Join(" ", MnemonicSeedWords));
            try
            {
                return JsonConvert.SerializeObject(keystore, Formatting.Indented);
            }
            catch (Exception e)
            {
                throw new AException("Error creating wallet-json", e);
            }
        }
    }
}