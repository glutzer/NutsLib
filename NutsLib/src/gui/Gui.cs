using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace NutsLib;

public abstract class Gui : GuiDialog
{
    // This is broken in vanilla, input will be passed to this one even if it's drawn last.
    public override double DrawOrder => 0.2;

    private bool shouldRepartition;

    /// <summary>
    /// If registered to key events this code will open the gui.
    /// </summary>
    public override string? ToggleKeyCombinationCode => null;
    public override bool UnregisterOnClose => true;

    private readonly List<Widget> widgets = [];
    private Widget[] widgetsBackToFront = [];

    public readonly GuiEvents guiEvents;

    // Set after events.
    public static int MouseX { get; set; }
    public static int MouseY { get; set; }

    public Gui() : base(MainAPI.Capi)
    {
        guiEvents = new(this);
    }

    public static int Scaled(int value)
    {
        return value * MainAPI.GuiScale;
    }

    public static float Scaled(float value)
    {
        return value * MainAPI.GuiScale;
    }

    /// <summary>
    /// Register events for the whole gui.
    /// </summary>
    public virtual void RegisterEvents(GuiEvents guiEvents)
    {

    }

    public override void OnGuiOpened()
    {
        MainAPI.OnWindowResize += OnWindowResize;
        MainAPI.OnGuiRescale += OnGuiRescale;
        SetWidgets();
    }

    public override void OnGuiClosed()
    {
        MainAPI.OnWindowResize -= OnWindowResize;
        MainAPI.OnGuiRescale -= OnGuiRescale;
        Dispose();
    }

    public override void Toggle()
    {
        if (IsOpened())
        {
            TryClose();
        }
        else
        {
            TryOpen();
        }
    }

    /// <summary>
    /// Add every widget to the gui here.
    /// </summary>
    public abstract void PopulateWidgets();

    /// <summary>
    /// Remakes all widgets using PopulateWidgets.
    /// </summary>
    public void SetWidgets()
    {
        Dispose();

        widgets.Clear();
        PopulateWidgets();

        PartitionWidgets();
    }

    /// <summary>
    /// Partitions events and order.
    /// Also sets bounds of all elements.
    /// </summary>
    private void PartitionWidgets()
    {
        shouldRepartition = false;

        guiEvents.ClearEvents();

        SortedDictionary<int, List<Widget>> sortedDictionary = [];
        List<Widget> sortedList = [];
        PartitionWidgets(sortedDictionary, widgets, 0);
        foreach (List<Widget> pair in sortedDictionary.Values)
        {
            sortedList.AddRange(pair);
        }
        widgetsBackToFront = sortedList.ToArray();

        // Re-register events.
        RegisterEvents(guiEvents);

        for (int i = widgetsBackToFront.Length; i-- > 0;)
        {
            widgetsBackToFront[i].RegisterEvents(guiEvents);
        }
    }

    private static void PartitionWidgets(SortedDictionary<int, List<Widget>> sortedDictionary, List<Widget> widgets, int currentPriority)
    {
        foreach (Widget widget in widgets)
        {
            int sortPriority = widget.SortPriority + currentPriority;
            if (!sortedDictionary.ContainsKey(sortPriority)) sortedDictionary.Add(sortPriority, []);
            sortedDictionary[sortPriority].Add(widget);

            if (widget.Children == null) continue;
            PartitionWidgets(sortedDictionary, widget.Children, sortPriority);
        }
    }

    /// <summary>
    /// Widgets should be partitioned on next event.
    /// Called when adding/removing children from a widget.
    /// </summary>
    public void MarkForRepartition()
    {
        shouldRepartition = true;
    }

    private void CheckRepartition()
    {
        if (shouldRepartition) PartitionWidgets();
    }

    public void AddWidget(Widget widget)
    {
        widgets.Add(widget);
    }

    public override void OnRenderGUI(float dt)
    {
        CheckRepartition();

        // Set bounds.
        foreach (Widget widget in widgets) widget.CalculateBounds();

        // Must re-use the current gui shader when done.
        IShaderProgram currentShader = capi.Render.CurrentActiveShader;

        ShaderGui guiShader = NuttyShaderRegistry.Get<ShaderGui>("gui");
        guiShader.Use();

        RenderTools.DisableDepthTest();

        guiEvents.TriggerBeforeRender(dt);

        float currentFade = 0f;
        guiShader.Uniform("fade", currentFade);
        foreach (Widget widget in widgetsBackToFront)
        {
            if (widget.Fade != currentFade)
            {
                currentFade = widget.Fade;
                guiShader.Uniform("fade", currentFade);
            }

            widget.OnRender(dt, guiShader);
        }

        guiEvents.TriggerAfterRender(dt);

        // Depth testing is enabled for normal guis but these are sorted.
        // Must be re-enabled for rendering items.
        RenderTools.EnableDepthTest();

        currentShader.Use();
    }

    /// <summary>
    /// On window resize, recreate the gui.
    /// </summary>
    public void OnWindowResize(int width, int height)
    {
        SetWidgets();
    }

    /// <summary>
    /// On gui rescale, recreate the gui.
    /// </summary>
    public void OnGuiRescale(int scale)
    {
        SetWidgets();
    }

    public override void OnMouseDown(MouseEvent args)
    {
        CheckRepartition();
        MouseX = args.X;
        MouseY = args.Y;
        guiEvents.TriggerMouseDown(args);
    }

    public override void OnMouseUp(MouseEvent args)
    {
        CheckRepartition();
        MouseX = args.X;
        MouseY = args.Y;
        guiEvents.TriggerMouseUp(args);
    }

    public override void OnMouseMove(MouseEvent args)
    {
        CheckRepartition();
        MouseX = args.X;
        MouseY = args.Y;
        MouseOverCursor = null;
        guiEvents.TriggerMouseMove(args);
    }

    public override void OnMouseWheel(MouseWheelEventArgs args)
    {
        CheckRepartition();
        guiEvents.TriggerMouseWheel(args);
    }

    public override void OnKeyDown(KeyEvent args)
    {
        CheckRepartition();
        guiEvents.TriggerKeyDown(args);
    }

    public override void OnKeyUp(KeyEvent args)
    {
        CheckRepartition();
        guiEvents.TriggerKeyUp(args);
    }

    public override void OnKeyPress(KeyEvent args)
    {
        CheckRepartition();
        guiEvents.TriggerKeyPress(args);
    }

    public override void Dispose()
    {
        CheckRepartition();

        foreach (Widget widget in widgetsBackToFront)
        {
            widget.Dispose();
        }

        widgetsBackToFront = [];

        // Might be some "composers" in there.
        base.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Enumerates over a type of widget in input order.
    /// </summary>
    public IEnumerable<T> ForWidgets<T>()
    {
        for (int i = widgetsBackToFront.Length; i-- > 0;)
        {
            if (widgetsBackToFront[i] is T widget) yield return widget;
        }
    }

    /// <summary>
    /// Enumerates over a type of widget in render order.
    /// </summary>
    public IEnumerable<T> ForWidgetsReverse<T>()
    {
        for (int i = 0; i < widgetsBackToFront.Length; i++)
        {
            if (widgetsBackToFront[i] is T widget) yield return widget;
        }
    }
}