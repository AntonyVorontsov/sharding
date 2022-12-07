using System;
using Common;
using FluentAssertions;
using Xunit;

namespace RendezvousHashing.Tests;

public sealed class RendezvousHashingProviderTests
{
    [Fact]
    public void Ctor_NoShards_ThrowsException()
    {
        var function = () => new RendezvousHashingProvider(Array.Empty<ShardName>());

        function.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_OneShard_DoesNotThrowException()
    {
        var function = () => new RendezvousHashingProvider(new[] { new ShardName("shard") });

        function.Should().NotThrow();
    }

    [Fact]
    public void Route_OneShard_ReturnsThatShard()
    {
        var shardName = new ShardName("shard");
        var provider = new RendezvousHashingProvider(new[] { shardName });

        var result = provider.Route("a key");

        result.Should().Be(shardName);
    }

    [Fact]
    public void Route_TwoShards_ReturnsSecondShard()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var provider = new RendezvousHashingProvider(new[] { firstShardName, secondShardName });

        var result = provider.Route("a key");

        // That is here just to show that in the case of resharding that key has to be relocated to another shard.
        result.Should().Be(secondShardName);
    }
}