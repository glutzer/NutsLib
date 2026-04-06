namespace NutsLib;

/// <summary>
/// Stacks child widgets vertically using fixed positioning on the Y.
/// The X will not be affected.
/// </summary>
public class WidgetVerticalStack : Widget
{
    private readonly int spacing;

    public WidgetVerticalStack(Widget? parent, Gui gui, int spacing = 1) : base(parent, gui)
    {
        this.spacing = spacing;
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.BeforeRender += GuiEvents_BeforeRender;
    }

    private void GuiEvents_BeforeRender(float obj)
    {
        RepositionYLevels();
    }

    public void RepositionYLevels()
    {
        int currentAdvance = 0;

        foreach (Widget widget in Children)
        {
            widget.Move(widget.GetFixedPos().X, currentAdvance);
            widget.CalculateBounds();

            currentAdvance += widget.Height;
            currentAdvance += spacing;
        }

        if (Children.Count > 0) CalculateBounds();
    }
}