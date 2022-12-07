using Common;

namespace RendezvousHashing;

public sealed class RendezvousHashingProvider : IHashingProvider
{
    private readonly ShardName[] _shards;
    private readonly IHashingFunction _hashingFunction;

    public RendezvousHashingProvider(IReadOnlyCollection<ShardName> shards)
        : this(shards, new XxHashingFunction())
    {
    }

    public RendezvousHashingProvider(
        IReadOnlyCollection<ShardName> shards,
        IHashingFunction hashingFunction)
    {
        if (!shards.Any())
        {
            throw new ArgumentException("You have to provide at least one shard", nameof(shards));
        }

        _shards = shards.ToArray();
        _hashingFunction = hashingFunction;
    }

    public ShardName Route(string key)
    {
        var weight = int.MinValue;
        ShardName? destinationShard = null;

        foreach (var shard in _shards)
        {
            var shardWeight = CalculateShardWeight(shard, key);
            if (shardWeight <= weight)
            {
                continue;
            }

            destinationShard = shard;
            weight = shardWeight;
        }

        if (destinationShard is null)
        {
            throw new InvalidOperationException($"Could not define the shard for key {key}");
        }

        return destinationShard.Value;
    }

    private int CalculateShardWeight(ShardName shard, string key)
    {
        var combinedKey = $"{shard.Value}-{key}";
        return _hashingFunction.Calculate(combinedKey);
    }
}