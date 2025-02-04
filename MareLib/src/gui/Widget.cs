using System.Collections.Generic;

namespace MareLib;

public abstract partial class Widget
{
    public Widget? Parent { get; private set; }
    public readonly List<Widget> children = new();

    /// <summary>
    /// When elements are sorted, this element and it's children will be prioritized by sort priority.
    /// </summary>
    public virtual int SortPriority => 0;

    public Widget(Widget? parent = null)
    {
        parent?.AddChild(this);
    }

    public Widget AddChild(Widget child)
    {
        children.Add(child);
        child.SetParent(this);
        return this;
    }

    public Widget SetParent(Widget? parent)
    {
        Parent = parent;
        return this;
    }

    public Widget NoScaling(bool noScale = true)
    {
        NoScale = noScale;
        return this;
    }

    /// <summary>
    /// Shader assumed to already be in use.
    /// </summary>
    public virtual void OnRender(float dt, MareShader shader)
    {

    }

    /// <summary>
    /// Register needed events to gui. Called after sorting.
    /// </summary>
    public virtual void RegisterEvents(GuiEvents guiEvents)
    {

    }

    /// <summary>
    /// Called when the gui is closed or when the widgets are set with PopulateWidgets.
    /// </summary>
    public virtual void Dispose()
    {

    }
}