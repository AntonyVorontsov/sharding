using System.Linq;
using BenchmarkDotNet.Attributes;
using Common;
using ConsistentHashing;
using JumpHashing;
using RendezvousHashing;

namespace Sharding.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
// ReSharper disable once ClassCanBeSealed.Global
public class HashingTechniqueBenchmark
{
    private readonly IHashingProvider _consistentHashing;
    private readonly IHashingProvider _jumpHashing;
    private readonly IHashingProvider _rendezvousHashing;

    [Params(10_000, 100_000, 1_000_000)]
    public int NumberOfKeys { get; set; }

    public HashingTechniqueBenchmark()
    {
        var hashingFunction = new XxHashingFunction();
        var shards = new ShardName[]
        {
            "shard-1",
            "shard-2",
            "shard-3",
            "shard-4",
            "shard-5"
        };

        _consistentHashing = new ConsistentHashingProvider(shards.Select(x => new Shard(x, 1000)), hashingFunction);
        _jumpHashing = new JumpHashingProvider(shards, hashingFunction);
        _rendezvousHashing = new RendezvousHashingProvider(shards, hashingFunction);
    }

    [Benchmark]
    public void Ring()
    {
        foreach (var key in Enumerable.Range(0, NumberOfKeys))
        {
            _ = _consistentHashing.Route(key.ToString());
        }
    }

    [Benchmark]
    public void Jump()
    {
        foreach (var key in Enumerable.Range(0, NumberOfKeys))
        {
            _ = _jumpHashing.Route(key.ToString());
        }
    }

    [Benchmark]
    public void Rendezvous()
    {
        foreach (var key in Enumerable.Range(0, NumberOfKeys))
        {
            _ = _rendezvousHashing.Route(key.ToString());
        }
    }
}