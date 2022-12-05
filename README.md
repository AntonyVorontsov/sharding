# Sharding

Consistent hashing and alternatives required for database sharding (implementation written in C#).

## Consistent hashing

In order to use consistent hashing - create an instance of `ConsistentHashingProvider` and pass a collection of shards:

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
The default implementation uses xxHash hashing algorithm from `System.Data.HashFunction.xxHash` package, but if you want to provide your own hashing algorithm there is a way to do it - you can implement the interface `IHashingFunction` and pass an instance of it while creating consistent hashing provider.

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