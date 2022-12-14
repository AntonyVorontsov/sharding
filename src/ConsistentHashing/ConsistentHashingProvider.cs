using System.Collections.Generic;
using System.Linq;
using Common;

namespace ConsistentHashing;

public sealed class ConsistentHashingProvider : IConsistentHashingProvider
{
    private readonly IHashingFunction _hashingFunction;
    private readonly Ring _ring;

    public IRing Ring => _ring;

    public ConsistentHashingProvider(IEnumerable<Shard> shards)
        : this(shards, new XxHashingFunction(), replicationFactor: 0)
    {
    }

    public ConsistentHashingProvider(
        IEnumerable<Shard> shards,
        IHashingFunction hashingFunction,
        int replicationFactor = 0)
    {
        _hashingFunction = hashingFunction;
        _ring = new Ring(replicationFactor);

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

    public RouteResult RouteWithPreferenceList(string key)
    {
        var hash = _hashingFunction.Calculate(key);
        return _ring.RouteWithPreferenceList(hash);
    }
}