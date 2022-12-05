namespace ConsistentHashing;

public interface IConsistentHashingProvider
{
    ShardName Route(string key);
    RouteResult RouteWithPreferenceList(string key);
}