using System.Collections.Generic;

namespace MareLib;

public abstract class Widget
{
    public readonly Gui gui;
    public Bounds bounds;

    public List<Widget>? children;

    public bool enabled = true;

    /// <summary>
    /// When elements are sorted, this element and it's children will be prioritized by sort priority.
    /// </summary>
    public virtual int SortPriority => 0;

    public Widget(Gui gui, Bounds bounds)
    {
        this.gui = gui;
        this.bounds = bounds;
    }

    public Widget AddChild(Widget child)
    {
        children ??= new List<Widget>();
        children.Add(child);
        return this;
    }

    /// <summary>
    /// Removes all children of a type.
    /// Also removes the bounds from it's parent if possible. (Most times a bounds is only made once for a child element).
    /// </summary>
    public void RemoveChildAndBoundsFromParent<T>()
    {
        children?.RemoveAll(children =>
        {
            if (children is T)
            {
                children.bounds.parentBounds?.children?.Remove(children.bounds);
                return true;
            }
            return false;
        });
    }

    public void ClearChildren()
    {
        children?.Clear();
    }

    public void AddClip(Bounds bounds)
    {
        AddChild(new ClipWidget(gui, true, bounds));
    }

    public void EndClip(Bounds bounds)
    {
        AddChild(new ClipWidget(gui, false, bounds));
    }

    /// <summary>
    /// Shader assumed to already be in use.
    /// </summary>
    public virtual void OnRender(float dt, MareShader shader)
    {

    }

    /// <summary>
    /// Called after bounds initialized.
    /// Make textures here.
    /// </summary>
    public virtual void Initialize()
    {

    }

    /// <summary>
    /// Register needed events to gui. Called after sorting.
    /// </summary>
    public virtual void RegisterEvents(GuiEvents guiEvents)
    {

    }

    public virtual void Dispose()
    {

    }
}