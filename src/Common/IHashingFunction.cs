namespace Common;

public interface IHashingFunction
{
    int Calculate(string key);
}