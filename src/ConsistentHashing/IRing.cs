using System.Collections.Generic;

namespace ConsistentHashing;

public interface IRing
{
    IReadOnlyDictionary<int, VirtualNode> VirtualNodesRing { get; }
}