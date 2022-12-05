using System;
using FluentAssertions;
using Xunit;

namespace ConsistentHashing.Tests;

public sealed class XxHashingFunctionTests
{
    [Fact]
    public void Calculate_HelloWorldString_Returns826579422()
    {
        var hashFunction = new XxHashingFunction();
        var hash = hashFunction.Calculate("hello world");
        hash.Should().Be(-826579422);
    }

    [Fact]
    public void Calculate_EmptyString_ThrowsException()
    {
        var hashFunction = new XxHashingFunction();
        var function = () => hashFunction.Calculate(string.Empty);
        function.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Calculate_NullString_ThrowsException()
    {
        var hashFunction = new XxHashingFunction();
        var function = () => hashFunction.Calculate(null!);
        function.Should().Throw<ArgumentException>();
    }
}