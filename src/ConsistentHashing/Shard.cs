using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace ConsistentHashing;

public sealed class Shard : IEquatable<Shard>
{
    public ShardName ShardName { get; }
    public int NumberOfNodes { get; }

    public Shard(ShardName shardShardName, int numberOfNodes)
    {
        if (numberOfNodes < 1)
        {
            throw new ArgumentException("Number of virtual nodes should be a positive number");
        }

        ShardName = shardShardName;
        NumberOfNodes = numberOfNodes;
    }

    public IEnumerable<VirtualNode> GenerateVirtualNodes()
    {
        return Enumerable.Range(1, NumberOfNodes).Select(x => new VirtualNode(ShardName, x));
    }

    public bool Equals(Shard? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ShardName.Equals(other.ShardName) && NumberOfNodes == other.NumberOfNodes;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Shard other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ShardName, NumberOfNodes);
    }

    public static bool operator ==(Shard? left, Shard? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Shard? left, Shard? right)
    {
        return !Equals(left, right);
    }
}