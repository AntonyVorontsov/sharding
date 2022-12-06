using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace ConsistentHashing;

public sealed class Ring : IRing
{
    private bool _initializationFinished = false;
    private readonly SortedDictionary<int, VirtualNode> _virtualNodesRing = new();
    private int[] _ringKeys = Array.Empty<int>();
    private readonly int _replicationFactor;
    private readonly HashSet<ShardName> _usedShards;

    public IReadOnlyDictionary<int, VirtualNode> VirtualNodesRing => _virtualNodesRing;

    public Ring(int replicationFactor)
    {
        if (replicationFactor < 0)
        {
            throw new ArgumentException("Replication factor should be a positive number", nameof(replicationFactor));
        }

        _replicationFactor = replicationFactor;
        _usedShards = new HashSet<ShardName>();
    }

    public void AddVirtualNode(int hash, VirtualNode virtualNode)
    {
        if (_initializationFinished)
        {
            throw new InvalidOperationException("Initialization of the ring has been finished already");
        }

        if (_virtualNodesRing.ContainsKey(hash))
        {
            throw new InvalidOperationException("The same hash for different virtual nodes. Check your hashing function");
        }

        _virtualNodesRing[hash] = virtualNode;
        _usedShards.Add(virtualNode.ShardName);
    }

    public void FinishInitialization()
    {
        if (_initializationFinished)
        {
            return;
        }

        if (!_virtualNodesRing.Any())
        {
            throw new InvalidOperationException("The ring has not been initialized");
        }

        if (_replicationFactor > _usedShards.Count - 1)
        {
            throw new InvalidOperationException("Replication factor cannot be more than a number of used shards");
        }

        _ringKeys = _virtualNodesRing.Keys.ToArray();
        _initializationFinished = true;
    }

    public ShardName Route(int hash)
    {
        if (!_initializationFinished)
        {
            throw new InvalidOperationException("Initialization of the ring has not been finished yet");
        }

        if (_virtualNodesRing.ContainsKey(hash))
        {
            return _virtualNodesRing[hash].ShardName;
        }

        var index = FindIndexUsingBinarySearch(hash);
        return _virtualNodesRing[_ringKeys[index]].ShardName;
    }

    public RouteResult RouteWithPreferenceList(int hash)
    {
        if (!_initializationFinished)
        {
            throw new InvalidOperationException("Initialization of the ring has not been finished yet");
        }

        var index = FindIndexUsingBinarySearch(hash);
        var mainShard = _virtualNodesRing[_ringKeys[index]].ShardName;
        var preferenceList = new HashSet<ShardName>();
        while (preferenceList.Count < _replicationFactor)
        {
            if (index == _ringKeys.Length - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }

            var nextShard = _virtualNodesRing[_ringKeys[index]].ShardName;
            if (nextShard != mainShard)
            {
                preferenceList.Add(nextShard);
            }
        }

        return new RouteResult(mainShard, preferenceList);
    }

    private int FindIndexUsingBinarySearch(int hash)
    {
        var left = 0;
        var right = _ringKeys.Length - 1;
        var middle = _ringKeys.Length / 2;

        while (left <= right)
        {
            // If we hit the right virtual node.
            if (_ringKeys[middle] == hash)
            {
                return middle;
            }

            if (_ringKeys[middle] < hash)
            {
                // If we hit the end of the ring then we return the first virtual node.
                if (middle == _ringKeys.Length - 1)
                {
                    return 0;
                }

                left = middle;
                middle = (right + left + 1) / 2;
            }
            else if (_ringKeys[middle] > hash)
            {
                // In case the hash lower than the first virtual node, then return the first node.
                if (middle == 0)
                {
                    return 0;
                }

                // Just in case.
                if (_ringKeys[middle - 1] == hash)
                {
                    return middle - 1;
                }

                // Our sweet spot.
                if (_ringKeys[middle - 1] < hash)
                {
                    return middle;
                }

                // Keep searching
                right = middle;
                middle = (right + left - 1) / 2;
            }
        }

        throw new InvalidOperationException($"Could not find the index for the hash {hash}");
    }
}