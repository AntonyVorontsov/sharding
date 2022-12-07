# Sharding

Consistent hashing and its alternatives required for database sharding (implementation written in C#).

## Consistent hashing

In order to use [consistent hashing](https://en.wikipedia.org/wiki/Consistent_hashing) - create an instance of `ConsistentHashingProvider` and pass a collection of shards:

```csharp
var shards = new[]
{
    new Shard("shard-1", numberOfNodes: 1000),
    new Shard("shard-2", numberOfNodes: 1000),
    new Shard("shard-3", numberOfNodes: 1000),
    new Shard("shard-4", numberOfNodes: 1000),
    new Shard("shard-5", numberOfNodes: 1000)
};
var provider = new ConsistentHashingProvider(shards);
```

The `numberOfNodes` parameter describes the number of virtual nodes that will be distributed across the hashing ring.
The default implementation uses `xxHash` hashing algorithm from `System.Data.HashFunction.xxHash` package, but if you want to provide your own hashing algorithm there is a way to do it - you can implement the interface `IHashingFunction` and pass an instance of it while creating consistent hashing provider.

```csharp
public sealed class YourOwnHashingAlgorithm : IHashingFunction
{
    public int Calculate(string key)
    {
        // calculate hash
    }
}

var provider = new ConsistentHashingProvider(shards, new YourOwnHashingAlgorithm());
```

And them you only have to use `Route` method to get the shard number the passed key belongs to.

```csharp
var provider = new ConsistentHashingProvider(shards);

var shardName = provider.Route("your string key");
```

In case you want to get a "route" with so-called preference list (reference to Amazon Dynamo) you can use `RouteWithPreferenceList` method. To make this thing work you have to specify the replication factor for `ConsistentHashingProvider`.

```csharp
var provider = new ConsistentHashingProvider(shards, new YourOwnHashingAlgorithm(), replicationFactor: 3);
var routeResult = provider.RouteWithPreferenceList("your key");
```

And the route result itself will look like this:

```csharp
public sealed record RouteResult(ShardName MainShard, IReadOnlyCollection<ShardName> PreferenceList);
```

Where `MainShard` is the shard the given key belongs to and the collection `PreferenceList` is the list of shards that can be used for replication (of the given key).

## Jump hashing

To use jump hashing algorithm (described in [this paper](https://arxiv.org/abs/1406.2294)) there is the class called `JumpHashingProvider`. The process of initialization is absolutely the same as in previous examples, you have to pass a list of shards and a hashing function which will handle strings.

```csharp
var shards = new ShardName[]
{
    "shard-1",
    "shard-2",
    "shard-3",
    "shard-4",
    "shard-5"
};
var provider = new JumpHashingProvider(shards, new XxHashingFunction());
```

And then you can route keys to shards.

```csharp
ShardName result = provider.Route("your key");
```

## RendezvousHashing hashing

The [rendezvous hashing](https://en.wikipedia.org/wiki/Rendezvous_hashing) technique is also available.

```csharp
var shards = new ShardName[]
{
    "shard-1",
    "shard-2",
    "shard-3",
    "shard-4",
    "shard-5"
};
var provider = new RendezvousHashingProvider(shards, new XxHashingFunction());
```

And then

```csharp
ShardName result = provider.Route("your key");
```