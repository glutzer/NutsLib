using System;
using System.Collections.Generic;

namespace NutsLib;

public abstract partial class Widget
{
    public Widget? Parent { get; private set; }
    public Gui Gui { get; private set; }
    public List<Widget> Children { get; } = [];

    /// <summary>
    /// When elements are sorted, this element and it's children will be prioritized by sort priority.
    /// </summary>
    public virtual int SortPriority => 0;

    public Widget(Widget? parent, Gui gui)
    {
        parent?.AddChild(this);
        Gui = gui;
    }

    public Widget AddChild(Widget child)
    {
        Children.Add(child);
        child.Parent?.Children.Remove(child);
        child.Parent = this;
        Gui.MarkForRepartition();
        return this;
    }

    /// <summary>
    /// Remove widget and return it, without disposing it.
    /// </summary>
    public Widget TakeSelf()
    {
        Parent?.Children.Remove(this);
        Parent = null;
        Gui.MarkForRepartition();
        return this;
    }

    /// <summary>
    /// Removes this widget and disposes it.
    /// </summary>
    public void DeleteSelf()
    {
        Parent?.Children.Remove(this);
        Parent = null;
        DisposeAndChildren();
        Gui.MarkForRepartition();
    }

    /// <summary>
    /// Removes all widgets and disposes them.
    /// </summary>
    public void DeleteChildren()
    {
        foreach (Widget child in Children)
        {
            child.DisposeAndChildren();
            child.Parent = null;
        }

        Children.Clear();
        Gui.MarkForRepartition();
    }

    /// <summary>
    /// Removes all widgets of a type and disposes them.
    /// </summary>
    public Widget DeleteChildren<T>() where T : Widget
    {
        foreach (Widget child in Children)
        {
            if (child is not T) continue;
            child.DisposeAndChildren();
            child.Parent = null;
        }

        if (Children.RemoveAll(x => x.Parent == null) > 0)
        {
            Gui.MarkForRepartition();
        }

        return this;
    }

    /// <summary>
    /// Removes all children where a predicate matches and disposes them.
    /// </summary>
    public Widget DeleteChildren(Func<Widget, bool> predicate)
    {
        foreach (Widget child in Children)
        {
            if (!predicate(child)) continue;
            child.DisposeAndChildren();
            child.Parent = null;
        }

        if (Children.RemoveAll(x => x.Parent == null) > 0)
        {
            Gui.MarkForRepartition();
        }

        return this;
    }

    /// <summary>
    /// Dispose this widget and all it's children.
    /// </summary>
    public void DisposeAndChildren()
    {
        Dispose();
        foreach (Widget child in Children)
        {
            child.DisposeAndChildren();
        }
    }

    public Widget SetParent(Widget? parent)
    {
        Parent?.Children.Remove(this);
        Parent = parent;
        Parent?.Children.Add(this);
        Gui.MarkForRepartition();
        return this;
    }

    /// <summary>
    /// Shader assumed to already be in use.
    /// </summary>
    public virtual void OnRender(float dt, NuttyShader shader)
    {

    }

    /// <summary>
    /// Operate on each child. Does not repartition.
    /// </summary>
    public void ForEachChild<T>(Action<T> action) where T : Widget
    {
        foreach (Widget child in Children)
        {
            if (child is T t)
            {
                action(t);
            }
        }
    }

    /// <summary>
    /// Cast to another widget for utility.
    /// </summary>
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