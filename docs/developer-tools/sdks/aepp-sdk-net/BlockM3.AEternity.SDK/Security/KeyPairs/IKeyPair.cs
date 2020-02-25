namespace BlockM3.AEternity.SDK.Security.KeyPairs
{
/**
 * a representation of private and public key pair
 *
 * @param <T>
 */
    public interface IKeyPair<T>
    {
        T PublicKey { get; }

        T PrivateKey { get; }
    }
}