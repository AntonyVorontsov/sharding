namespace ConsistentHashing;

public interface IConsistentHashingProvider
{
    ShardName Route(string key);
}