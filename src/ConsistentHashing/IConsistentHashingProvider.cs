using Common;

namespace ConsistentHashing;

public interface IConsistentHashingProvider : IHashingProvider
{
    RouteResult RouteWithPreferenceList(string key);
}