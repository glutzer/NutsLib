namespace NutsLib;

public interface IUbo
{
    int Handle { get; }
    void Bind(int bindingPoint);
}