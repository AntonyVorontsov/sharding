using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace JumpHashing;

public sealed class JumpHashingProvider : IHashingProvider
{
    private readonly ShardName[] _shards;
    private readonly IHashingFunction _hashingFunction;

    public JumpHashingProvider(IReadOnlyCollection<ShardName> shards) : this(shards, new XxHashingFunction())
    {
    }

    public JumpHashingProvider(
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
        var hash = _hashingFunction.Calculate(key);
        var shardIndex = GetShardIndexByJumpAlgorithm(hash);
        if (shardIndex < 0)
        {
            throw new InvalidOperationException($"Could not calculate the bucket index for hash {hash}");
        }

        return _shards[shardIndex];
    }

    private int GetShardIndexByJumpAlgorithm(int hash)
    {
        var sporadic = new Random(Seed: hash);

        var bucketBeforePreviousJump = -1;
        var jump = 0;

        while (jump < _shards.Length)
        {
            bucketBeforePreviousJump = jump;
            var pseudoRandomValue = sporadic.NextDouble();
            jump = (int)Math.Floor((bucketBeforePreviousJump + 1) / pseudoRandomValue);
        }

        return bucketBeforePreviousJump;
    }
}