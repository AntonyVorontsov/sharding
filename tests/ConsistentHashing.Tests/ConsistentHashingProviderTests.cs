using System;
using FluentAssertions;
using Xunit;

namespace ConsistentHashing.Tests;

public sealed class ConsistentHashingProviderTests
{
    [Fact]
    public void Initialize_NoShards_ThrowsException()
    {
        var function = () => new ConsistentHashingProvider(
            Array.Empty<Shard>(),
            new XxHashingFunction());

        function.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Route_DoesNotThrow()
    {
        var provider = new ConsistentHashingProvider(
            new[]
            {
                new Shard("shard-1", numberOfNodes: 1000),
                new Shard("shard-2", numberOfNodes: 1000),
                new Shard("shard-3", numberOfNodes: 1000),
                new Shard("shard-4", numberOfNodes: 1000),
                new Shard("shard-5", numberOfNodes: 1000),
            },
            new XxHashingFunction());

        var function = () => provider.Route("key");

        function.Should().NotThrow();
    }
}