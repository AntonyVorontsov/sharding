using System;
using System.Collections.Generic;
using System.Linq;
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

var routingCalculator = shards.ToDictionary(x => x.ShardName, _ => 0);

const int numberOfIdentifiers = 5_000_000;
foreach (var id in GetIdsRange(numberOfIdentifiers))
{
    var shard = provider.Route(id.ToString());
    routingCalculator[shard]++;
}

var distributionCalculator = new SortedDictionary<ShardName, float>(
    routingCalculator.ToDictionary(
        x => x.Key,
        x => (float)x.Value / numberOfIdentifiers * 100));
var averageDistribution = (float)100 / distributionCalculator.Count;

Console.WriteLine("Data distribution per shard");
Console.WriteLine();

new ConsoleTable(GetHeaderRow(distributionCalculator.Keys).ToArray())
    .AddRow(GetDistributionRowValues(distributionCalculator).ToArray())
    .AddRow(GetDeltaRowValues(distributionCalculator, averageDistribution).ToArray())
    .Write();

static IEnumerable<long> GetIdsRangeNaive(int numberOfIdentifiers)
{
    return Enumerable.Range(1, numberOfIdentifiers).Select(x => (long)x);
}

static IEnumerable<long> GetIdsRange(int numberOfIdentifiers)
{
    var random = new Random(Seed: 42);

    var previousValue = 0L;
    const int constantOffset = 2_500;
    foreach (var _ in Enumerable.Range(1, numberOfIdentifiers))
    {
        var id = previousValue + constantOffset + random.Next(0, constantOffset);
        yield return id;
        previousValue = id;
    }
}

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