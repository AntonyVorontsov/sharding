using System.Collections.Generic;

namespace ConsistentHashing;

public sealed record RouteResult(ShardName MainShard, IReadOnlyCollection<ShardName> PreferenceList);