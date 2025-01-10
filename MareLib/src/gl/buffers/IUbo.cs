namespace MareLib;

public interface IUbo
{
    public int Handle { get; }
    public void Bind(int bindingPoint);
}