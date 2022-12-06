using System.Collections.Generic;
using Common;

namespace ConsistentHashing;

public sealed record RouteResult(ShardName MainShard, IReadOnlyCollection<ShardName> PreferenceList);