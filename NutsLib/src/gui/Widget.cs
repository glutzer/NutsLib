using System;
using System.Collections.Generic;

namespace NutsLib;

public abstract partial class Widget
{
    public Widget? Parent { get; private set; }
    public readonly List<Widget> children = [];

    /// <summary>
    /// When elements are sorted, this element and it's children will be prioritized by sort priority.
    /// </summary>
    public virtual int SortPriority => 0;

    public Widget(Widget? parent)
    {
        parent?.AddChild(this);
    }

    public Widget AddChild(Widget child)
    {
        children.Add(child);
        child.Parent?.children.Remove(child);
        child.Parent = this;
        return this;
    }

    public Widget RemoveSelf(bool dispose = true)
    {
        Parent?.children.Remove(this);
        Parent = null;
        if (dispose) DisposeAndChildren();
        return this;
    }

    public Widget ClearChildren(bool dispose = true)
    {
        foreach (Widget child in children)
        {
            child.Parent = null;

            if (dispose)
            {
                child.DisposeAndChildren();
            }
        }
        children.Clear();
        return this;
    }

    public void ClearChildren<T>(bool dispose = true) where T : Widget
    {
        for (int i = children.Count - 1; i >= 0; i--)
        {
            if (children[i] is T t)
            {
                t.Parent = null;
                if (dispose) t.DisposeAndChildren();
                children.RemoveAt(i);
            }
        }
    }

    public Widget RemoveChild(Widget widget, bool dispose = true)
    {
        widget.Parent = null;
        if (dispose) widget.DisposeAndChildren();
        children.Remove(widget);
        return this;
    }

    /// <summary>
    /// Dispose this widget and all it's children.
    /// </summary>
    public void DisposeAndChildren()
    {
        Dispose();
        foreach (Widget child in children)
        {
            child.DisposeAndChildren();
        }
    }

    public Widget SetParent(Widget? parent)
    {
        Parent?.children.Remove(this);
        Parent = parent;
        Parent?.children.Add(this);
        return this;
    }

    /// <summary>
    /// Shader assumed to already be in use.
    /// </summary>
    public virtual void OnRender(float dt, NuttyShader shader)
    {

    }

    public void ForEachChild<T>(Action<T> action) where T : Widget
    {
        foreach (Widget child in children)
        {
            if (child is T t)
            {
                action(t);
            }
        }
    }

    public void As<T>(out T widget) where T : Widget
    {
        widget = (T)this;
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