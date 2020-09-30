using System.Collections.Generic;
using NBitcoin;

namespace BlockM3.AEternity.SDK.Security.KeyPairs
{
    public class DeterministicKeyPair
    {
        private readonly ExtKey _key;
        private readonly Dictionary<string, int> hierarchy = new Dictionary<string, int>();

        public DeterministicKeyPair(byte[] seed)
        {
            _key = new ExtKey(seed);
        }

        private DeterministicKeyPair(ExtKey key, string basepath, int pos)
        {
            _key = key;
            hierarchy[basepath] = pos;
        }

        public int Depth => _key.Depth;
        public Key PrivateKey => _key.PrivateKey;

        public DeterministicKeyPair DeriveNextChild(string path, bool hardened)
        {
            int num = 0;
            if (hierarchy.ContainsKey(path))
                num = hierarchy[path];
            hierarchy[path] = num + 1;
            string npath = path + "/" + num;
            if (hardened)
                npath += "'";
            ExtKey k = _key.Derive(new KeyPath(npath));
            return new DeterministicKeyPair(k, npath, 0);
        }
    }
}