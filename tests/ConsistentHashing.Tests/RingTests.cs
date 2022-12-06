using System;
using Common;
using FluentAssertions;
using Xunit;

namespace ConsistentHashing.Tests;

public sealed class RingTests
{
    [Fact]
    public void AddVirtualNode_InitializationFinished_ThrowsException()
    {
        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(new ShardName("shard-1"), 1));
        ring.FinishInitialization();

        var action = () => ring.AddVirtualNode(100, new VirtualNode(new ShardName("shard-2"), 1));
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddVirtualNode_NodeAlreadyExists_ThrowsException()
    {
        var ring = new Ring(replicationFactor: 0);

        const int hash = 100;
        var virtualNode = new VirtualNode(new ShardName("shard"), 1);
        ring.AddVirtualNode(hash, virtualNode);

        var action = () => ring.AddVirtualNode(hash, virtualNode);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddVirtualNode_NoNodes_AddsOneNodeToRing()
    {
        var ring = new Ring(replicationFactor: 0);

        const int hash = 100;
        var virtualNode = new VirtualNode(new ShardName("shard"), 1);

        ring.AddVirtualNode(hash, virtualNode);

        ring.VirtualNodesRing.Count.Should().Be(1);
        ring.VirtualNodesRing.Should().ContainKey(hash);
        ring.VirtualNodesRing.Should().ContainValue(virtualNode);
    }

    [Fact]
    public void AddVirtualNode_OneNodeInRing_AddsAnotherNodeToRing()
    {
        var ring = new Ring(replicationFactor: 0);

        var existingNode = new VirtualNode(new ShardName("shard-1"), 1);
        ring.AddVirtualNode(50, existingNode);

        var virtualNode = new VirtualNode(new ShardName("shard-2"), 1);
        ring.AddVirtualNode(100, virtualNode);

        ring.VirtualNodesRing.Count.Should().Be(2);
        ring.VirtualNodesRing.Should().ContainKeys(50, 100);
        ring.VirtualNodesRing.Should().ContainValues(existingNode, virtualNode);
    }

    [Fact]
    public void FinishInitialization_EmptyRing_ThrowsException()
    {
        var ring = new Ring(replicationFactor: 0);

        var action = () => ring.FinishInitialization();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FinishInitialization_ReplicationFactorMoreThanNumberOfShards_ThrowsException()
    {
        var ring = new Ring(replicationFactor: 2);
        ring.AddVirtualNode(50, new VirtualNode(new ShardName("shard-1"), 1));

        var action = () => ring.FinishInitialization();

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Route_InitializationIsNotFinished_ThrowsException()
    {
        var ring = new Ring(replicationFactor: 0);

        var action = () => ring.Route(100);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Route_HashLowerThanFirstVirtualNode_ReturnsShardFromFirstVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(50);
        routedShardName.Should().Be(firstShardName);
    }

    [Fact]
    public void Route_HashEqualsToFirstVirtualNode_ReturnsShardFromFirstVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(100);
        routedShardName.Should().Be(firstShardName);
    }

    [Fact]
    public void Route_HashLowerThanSecondVirtualNodeButHigherThanFirstVirtualNode_ReturnsShardFromSecondVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(150);
        routedShardName.Should().Be(secondShardName);
    }

    [Fact]
    public void Route_HashEqualsToSecondVirtualNode_ReturnsShardFromSecondVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(200);
        routedShardName.Should().Be(secondShardName);
    }

    [Fact]
    public void Route_HashLowerThanThirdVirtualNodeButHigherThanSecondVirtualNode_ReturnsShardFromThirdVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(250);
        routedShardName.Should().Be(thirdShardName);
    }

    [Fact]
    public void Route_HashEqualsToThirdVirtualNode_ReturnsShardFromThirdVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(300);
        routedShardName.Should().Be(thirdShardName);
    }

    [Fact]
    public void Route_HashLowerThanFourthVirtualNodeButHigherThanThirdVirtualNode_ReturnsShardFromFourthVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(350);
        routedShardName.Should().Be(fourthShardName);
    }

    [Fact]
    public void Route_HashEqualsToFourthVirtualNode_ReturnsShardFromFourthVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(400);
        routedShardName.Should().Be(fourthShardName);
    }

    [Fact]
    public void Route_HashLowerThanFifthVirtualNodeButHigherThanFourthVirtualNode_ReturnsShardFromFifthVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(450);
        routedShardName.Should().Be(fifthShardName);
    }

    [Fact]
    public void Route_HashEqualsToFifthVirtualNode_ReturnsShardFromFifthVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(500);
        routedShardName.Should().Be(fifthShardName);
    }

    [Fact]
    public void Route_HashMoreThanFifthVirtualNode_ReturnsShardFromFirstVirtualNode()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routedShardName = ring.Route(550);
        routedShardName.Should().Be(firstShardName);
    }

    [Fact]
    public void RouteWithPreferenceList_HashMoreThanFifthVirtualNode_ReturnsShardFromFirstVirtualNode_WithPreferenceListOfNextThreeShards()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 3);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routeResult = ring.RouteWithPreferenceList(550);
        routeResult.MainShard.Should().Be(firstShardName);
        routeResult.PreferenceList.Count.Should().Be(3);
        routeResult.PreferenceList.Should().Contain(new[] { secondShardName, thirdShardName, fourthShardName });
    }

    [Fact]
    public void RouteWithPreferenceList_HashMoreThanFourthVirtualNode_ReturnsShardFromFifthVirtualNode_WithPreferenceListOfNextThreeShards()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");
        var fourthShardName = new ShardName("shard-4");
        var fifthShardName = new ShardName("shard-5");

        var ring = new Ring(replicationFactor: 3);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(thirdShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(fourthShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(fifthShardName, 1));
        ring.FinishInitialization();

        var routeResult = ring.RouteWithPreferenceList(450);
        routeResult.MainShard.Should().Be(fifthShardName);
        routeResult.PreferenceList.Count.Should().Be(3);
        routeResult.PreferenceList.Should().Contain(new[] { firstShardName, secondShardName, thirdShardName });
    }

    [Fact]
    public void RouteWithPreferenceList_HashLessThanFirstVirtualNode_ReturnsShardFromFirst_WithPreferenceListOfNextTwoShards()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");

        var ring = new Ring(replicationFactor: 2);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(thirdShardName, 1));
        ring.FinishInitialization();

        var routeResult = ring.RouteWithPreferenceList(50);
        routeResult.MainShard.Should().Be(firstShardName);
        routeResult.PreferenceList.Count.Should().Be(2);
        routeResult.PreferenceList.Should().Contain(new[] { secondShardName, thirdShardName });
    }

    [Fact]
    public void RouteWithPreferenceList_ZeroReplicationFactor_HashLessThanFirstVirtualNode_ReturnsShardFromFirst_EmptyPreferenceList()
    {
        var firstShardName = new ShardName("shard-1");
        var secondShardName = new ShardName("shard-2");
        var thirdShardName = new ShardName("shard-3");

        var ring = new Ring(replicationFactor: 0);

        ring.AddVirtualNode(100, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(200, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(300, new VirtualNode(firstShardName, 1));
        ring.AddVirtualNode(400, new VirtualNode(secondShardName, 1));
        ring.AddVirtualNode(500, new VirtualNode(thirdShardName, 1));
        ring.FinishInitialization();

        var routeResult = ring.RouteWithPreferenceList(50);
        routeResult.MainShard.Should().Be(firstShardName);
        routeResult.PreferenceList.Count.Should().Be(0);
    }
}