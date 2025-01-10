using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace MareLib;

public abstract class Gui : GuiDialog
{
    // This is broken in vanilla, input will be passed to this one even if it's drawn last.
    public override double DrawOrder => 0.2;

    public Bounds? MainBounds { get; private set; }

    private bool shouldRepartition;

    /// <summary>
    /// If registered to key events this code will open the gui.
    /// </summary>
    public override string? ToggleKeyCombinationCode => null;
    public override bool UnregisterOnClose => true;

    private readonly List<Widget> widgets = new();
    private Widget[] widgetsBackToFront = Array.Empty<Widget>();

    public readonly GuiEvents guiEvents = new();

    public Gui(ICoreClientAPI capi) : base(capi)
    {

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
    /// Returns the main bounds of the gui.
    /// </summary>
    public abstract void PopulateWidgets(out Bounds mainBounds);

    /// <summary>
    /// Remakes all widgets.
    /// </summary>
    private void SetWidgets()
    {
        Dispose();

        widgets.Clear();
        PopulateWidgets(out Bounds mainBounds);
        MainBounds = mainBounds;

        PartitionWidgets();
        MainBounds.SetBounds();

        foreach (Widget widget in widgetsBackToFront) widget.Initialize();
    }

    /// <summary>
    /// Partitions events and order.
    /// </summary>
    private void PartitionWidgets()
    {
        guiEvents.ClearEvents();

        SortedDictionary<int, List<Widget>> sortedDictionary = new();
        List<Widget> sortedList = new();
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
            if (!sortedDictionary.ContainsKey(sortPriority)) sortedDictionary.Add(sortPriority, new List<Widget>());
            sortedDictionary[sortPriority].Add(widget);

            if (widget.children == null) continue;
            PartitionWidgets(sortedDictionary, widget.children, currentPriority);
        }
    }

    /// <summary>
    /// Marks widgets to be repartitioned on the next frame.
    /// </summary>
    public void MarkForRepartition()
    {
        shouldRepartition = true;
    }

    public void AddWidget(Widget widget)
    {
        widgets.Add(widget);
    }

    public override void OnRenderGUI(float dt)
    {
        if (shouldRepartition) PartitionWidgets();

        // Must re-use the current gui shader when done.
        IShaderProgram currentShader = capi.Render.CurrentActiveShader;

        MareShader guiShader = MareShaderRegistry.Get("gui");
        guiShader.Use();

        RenderTools.DisableDepthTest();

        guiEvents.TriggerBeforeRender(dt);

        foreach (Widget widget in widgetsBackToFront)
        {
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
        guiEvents.TriggerMouseDown(args);
    }

    public override void OnMouseUp(MouseEvent args)
    {
        guiEvents.TriggerMouseUp(args);
    }

    public override void OnMouseMove(MouseEvent args)
    {
        guiEvents.TriggerMouseMove(args);
    }

    public override void OnMouseWheel(MouseWheelEventArgs args)
    {
        guiEvents.TriggerMouseWheel(args);
    }

    public override void OnKeyDown(KeyEvent args)
    {
        guiEvents.TriggerKeyDown(args);
    }

    public override void OnKeyUp(KeyEvent args)
    {
        guiEvents.TriggerKeyUp(args);
    }

    public override void OnKeyPress(KeyEvent args)
    {
        guiEvents.TriggerKeyPress(args);
    }

    public override void Dispose()
    {
        foreach (Widget widget in widgetsBackToFront)
        {
            widget.Dispose();
        }

        // Might be some "composers" in there.
        base.Dispose();

        GC.SuppressFinalize(this);
    }
}