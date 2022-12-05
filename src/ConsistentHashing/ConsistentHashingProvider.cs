using System.Collections.Generic;
using System.Linq;

namespace ConsistentHashing;

public sealed class ConsistentHashingProvider : IConsistentHashingProvider
{
    private readonly IHashingFunction _hashingFunction;
    private readonly Ring _ring;

    public IRing Ring => _ring;

    public ConsistentHashingProvider(IEnumerable<Shard> shards) : this(shards, new XxHashingFunction())
    {
    }

    public ConsistentHashingProvider(IEnumerable<Shard> shards, IHashingFunction hashingFunction)
    {
        _hashingFunction = hashingFunction;
        _ring = new Ring();

        InitializeRing(shards);
    }

    private void InitializeRing(IEnumerable<Shard> shards)
    {
        var virtualNodes = shards.SelectMany(x => x.GenerateVirtualNodes()).ToArray();
        foreach (var virtualNode in virtualNodes)
        {
            var hash = _hashingFunction.Calculate(virtualNode.NodeName);
            _ring.AddVirtualNode(hash, virtualNode);
        }

        _ring.FinishInitialization();
    }

    public ShardName Route(string key)
    {
        var hash = _hashingFunction.Calculate(key);
        return _ring.Route(hash);
    }
}