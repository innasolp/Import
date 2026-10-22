using System.Text;

namespace Import.KeyHash.XxHash64;

public class XxHash64KeyHasher : IKeyHasher
{
    public byte[] Hash<T>(T key)
    {
        var sourceBytes = JsonCanonicalizer.GetCanonicalJson(key);

        byte[] hashBytes = System.IO.Hashing.XxHash64.Hash(sourceBytes);

        byte[] prefix = Encoding.UTF8.GetBytes("idx:");
        byte[] finalKey = new byte[prefix.Length + hashBytes.Length];

        Buffer.BlockCopy(prefix, 0, finalKey, 0, prefix.Length);
        Buffer.BlockCopy(hashBytes, 0, finalKey, prefix.Length, hashBytes.Length);

        return finalKey;
    }
}