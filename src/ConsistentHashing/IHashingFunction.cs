namespace ConsistentHashing;

public interface IHashingFunction
{
    int Calculate(string key);
}