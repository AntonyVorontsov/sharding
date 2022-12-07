using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using ConsoleTables;
using RendezvousHashing;

var shards = new ShardName[]
{
    "shard-1",
    "shard-2",
    "shard-3",
    "shard-4",
    "shard-5"
};
var provider = new RendezvousHashingProvider(shards, new XxHashingFunction());

var routingCalculator = shards.ToDictionary(x => x, _ => 0);

const int numberOfIdentifiers = 5_000_000;
foreach (var id in GetIdsRangeNaive(numberOfIdentifiers))
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