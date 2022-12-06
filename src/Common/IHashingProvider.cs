namespace Common;

public interface IHashingProvider
{
    ShardName Route(string key);
}