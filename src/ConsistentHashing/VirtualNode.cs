namespace ConsistentHashing;

public sealed record VirtualNode(ShardName ShardName, int NodeNumber)
{
    public string NodeName => $"{ShardName}-virtual-node-{NodeNumber}";
}