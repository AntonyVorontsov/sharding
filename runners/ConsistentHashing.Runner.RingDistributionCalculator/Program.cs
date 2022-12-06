using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using ConsistentHashing;
using ConsoleTables;

var shards = new[]
{
    new Shard("shard-1", numberOfNodes: 1000),
    new Shard("shard-2", numberOfNodes: 1000),
    new Shard("shard-3", numberOfNodes: 1000),
    new Shard("shard-4", numberOfNodes: 1000),
    new Shard("shard-5", numberOfNodes: 1000)
};
var provider = new ConsistentHashingProvider(shards, new XxHashingFunction());

var ring = provider.Ring.VirtualNodesRing;
var rangeCalculator = shards.ToDictionary(x => x.ShardName, _ => 0L);

var indexOfElement = 0;
var previousHash = 0;
foreach (var (hash, virtualNode) in ring)
{
    if (indexOfElement == 0)
    {
        rangeCalculator[virtualNode.ShardName] += hash - int.MinValue;
        indexOfElement++;
        previousHash = hash;
        continue;
    }

    if (indexOfElement == ring.Count - 1)
    {
        rangeCalculator[virtualNode.ShardName] += int.MaxValue - hash;
        indexOfElement++;
        previousHash = hash;
        continue;
    }

    rangeCalculator[virtualNode.ShardName] += hash - previousHash;
    indexOfElement++;
    previousHash = hash;
}

const long range = (long)int.MaxValue + int.MaxValue;
var distributionCalculator = new SortedDictionary<ShardName, float>(
    rangeCalculator.ToDictionary(
        x => x.Key,
        x => (float)x.Value / range * 100));

var averageDistribution = (float)100 / distributionCalculator.Count;

Console.WriteLine("Distribution of ranges on the ring");

Console.WriteLine();
new ConsoleTable(GetHeaderRow(distributionCalculator.Keys).ToArray())
    .AddRow(GetDistributionRowValues(distributionCalculator).ToArray())
    .AddRow(GetDeltaRowValues(distributionCalculator, averageDistribution).ToArray())
    .Write();
    
static IEnumerable<string> GetHeaderRow(IEnumerable<ShardName> shardNames)
{
    yield return "Shards:";

    foreach (var shardName in shardNames)
    {
        yield return shardName.Value;
    }
}

static IEnumerable<object> GetDistributionRowValues(IReadOnlyDictionary<ShardName, float> map)
{
    yield return "Distribution %";

    foreach (var (_, value) in map)
    {
        yield return value.ToString("n2");
    }
}

static IEnumerable<object> GetDeltaRowValues(IReadOnlyDictionary<ShardName, float> map, float averageDistribution)
{
    yield return "Delta %";

    foreach (var (_, value) in map)
    {
        yield return Math.Abs(value - averageDistribution).ToString("n2");
    }
}