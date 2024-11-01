using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

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

    private readonly List<Widget> elements = new();
    private Widget[] elementsRenderOrder = Array.Empty<Widget>();
    private Widget[] elementsInteractOrder = Array.Empty<Widget>();

    public readonly GuiEvents guiEvents = new();

    public Gui(ICoreClientAPI capi) : base(capi)
    {
        MainHook.OnWindowResize += OnWindowResize;
        MainHook.OnGuiRescale += OnGuiRescale;
    }

    public virtual void RegisterEvents(GuiEvents guiEvents)
    {

    }

    public override void OnGuiOpened()
    {
        SetElements();
    }

    public override void OnGuiClosed()
    {
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
    /// Add every element to the gui here.
    /// Returns the main bounds of the gui.
    /// </summary>
    public abstract Bounds PopulateElements();

    /// <summary>
    /// Remakes all elements.
    /// </summary>
    private void SetElements()
    {
        Dispose();

        elements.Clear();
        MainBounds = PopulateElements();

        PartitionElements();
        MainBounds.SetBounds();

        foreach (Widget element in elementsRenderOrder) element.Initialize();
    }

    /// <summary>
    /// Partitions events and order.
    /// </summary>
    private void PartitionElements()
    {
        guiEvents.ClearEvents();

        SortedDictionary<int, List<Widget>> sortedDictionary = new();
        List<Widget> sortedList = new();
        PartitionElements(sortedDictionary, elements, 0);
        foreach (List<Widget> pair in sortedDictionary.Values)
        {
            sortedList.AddRange(pair);
        }
        elementsRenderOrder = sortedList.ToArray();
        sortedList.Reverse();
        elementsInteractOrder = sortedList.ToArray();

        // Re-register events.
        RegisterEvents(guiEvents);
        foreach (Widget element in elementsInteractOrder)
        {
            element.RegisterEvents(guiEvents);
        }
    }

    private static void PartitionElements(SortedDictionary<int, List<Widget>> sortedDictionary, List<Widget> elements, int currentPriority)
    {
        foreach (Widget element in elements)
        {
            int sortPriority = element.SortPriority + currentPriority;
            if (!sortedDictionary.ContainsKey(sortPriority)) sortedDictionary.Add(sortPriority, new List<Widget>());
            sortedDictionary[sortPriority].Add(element);

            if (element.children == null) continue;
            PartitionElements(sortedDictionary, element.children, currentPriority);
        }
    }

    /// <summary>
    /// Marks elements to be repartitioned on the next frame.
    /// </summary>
    public void MarkForRepartition()
    {
        shouldRepartition = true;
    }

    public void AddElement(Widget element)
    {
        elements.Add(element);
    }

    public override void OnRenderGUI(float dt)
    {
        if (shouldRepartition) PartitionElements();

        IShaderProgram currentShader = capi.Render.CurrentActiveShader;
        currentShader.Stop();

        ShaderProgram guiShader = MareShaderRegistry.Shaders["gui"];
        guiShader.Use();

        // Set ortho.
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, MainHook.RenderWidth, MainHook.RenderHeight, 0, -1, 1);
        guiShader.Uniform("projectionMatrix", projection);

        capi.Render.GLDisableDepthTest();

        guiEvents.TriggerBeforeRender(dt);

        foreach (Widget element in elementsRenderOrder)
        {
            element.OnRender(dt, guiShader);
        }

        guiEvents.TriggerAfterRender(dt);

        // Depth testing is enabled for normal guis but these are sorted.
        // Must be re-enabled for rendering items.
        capi.Render.GLEnableDepthTest();

        guiShader.Stop();
        currentShader.Use();
    }

    /// <summary>
    /// On window resize, recreate the gui.
    /// </summary>
    public void OnWindowResize(int width, int height)
    {
        SetElements();
    }

    /// <summary>
    /// On gui rescale, recreate the gui.
    /// </summary>
    public void OnGuiRescale(int scale)
    {
        SetElements();
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
        foreach (Widget element in elementsRenderOrder)
        {
            element.Dispose();
        }

        // Might be some "composers" in there.
        base.Dispose();

        GC.SuppressFinalize(this);
    }
}