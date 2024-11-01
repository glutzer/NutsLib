using Vintagestory.API.Client;

namespace MareLib;

public class TestGui : Gui
{
    public TestGui(ICoreClientAPI capi) : base(capi)
    {
    }

    public override Bounds PopulateElements()
    {
        Bounds bounds = Bounds.Create().Fixed(32, 32, 32, 32);

        AddElement(new TestElement(this, bounds));

        return bounds;
    }
}