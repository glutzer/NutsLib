using Vintagestory.API.Client;

namespace MareLib;

public class TestGui : Gui
{
    public TestGui() : base()
    {
    }

    public override void PopulateWidgets(out Bounds mainBounds)
    {
        mainBounds = Bounds.Create().Fixed(32, 32, 32, 32);

        //AddWidget();
    }
}