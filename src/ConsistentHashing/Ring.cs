using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsistentHashing;

public sealed class Ring : IRing
{
    private bool _initializationFinished = false;
    private readonly SortedDictionary<int, VirtualNode> _virtualNodesRing = new();
    private int[] _ringKeys = Array.Empty<int>();

    public IReadOnlyDictionary<int, VirtualNode> VirtualNodesRing => _virtualNodesRing;

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