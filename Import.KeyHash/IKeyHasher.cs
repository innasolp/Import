namespace Import.KeyHash;

public interface IKeyHasher
{
    byte[] Hash<T>(T key);
}