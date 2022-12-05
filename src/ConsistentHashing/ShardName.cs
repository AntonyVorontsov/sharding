using System;

namespace ConsistentHashing;

public readonly struct ShardName : IEquatable<ShardName>, IComparable<ShardName>
{
    public string Value { get; }

    public ShardName(string value)
    {
        Value = value;
    }

    public static implicit operator string(ShardName name) => name.Value;

    public static implicit operator ShardName(string value) => new(value);

    public override string ToString() => Value;

    public bool Equals(ShardName other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ShardName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(ShardName left, ShardName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShardName left, ShardName right)
    {
        return !left.Equals(right);
    }

    public int CompareTo(ShardName other)
    {
        return string.Compare(Value, other.Value, StringComparison.Ordinal);
    }
}