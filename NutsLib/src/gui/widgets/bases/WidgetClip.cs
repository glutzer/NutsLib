﻿namespace NutsLib;

/// <summary>
/// Widget for both clipping the rendering and clipping mouse interactions with the ui.
/// Check RenderTools.IsPointInside or a static helper in widget.
/// </summary>
public class WidgetClip : Widget
{
    public bool clip;

    public WidgetClip(bool clip, Widget? parent) : base(parent)
    {
        this.clip = clip;
    }

    public override void OnRender(float dt, MareShader shader)
    {
        if (clip)
        {
            RenderTools.PushScissor(this);
        }
        else
        {
            RenderTools.PopScissor();
        }
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        // Subscribe update to every event that has a position.
        guiEvents.MouseDown += OnUpdate;
        guiEvents.MouseUp += OnUpdate;
        guiEvents.MouseMove += OnUpdate;
        guiEvents.MouseWheel += OnUpdate;
        guiEvents.KeyDown += OnUpdate;
        guiEvents.KeyUp += OnUpdate;
        guiEvents.KeyPress += OnUpdate;
    }

    private void OnUpdate(object obj)
    {
        // Clip should never be called before push here, if elements have been added properly.
        if (clip)
        {
            RenderTools.PopScissor();
        }
        else
        {
            RenderTools.PushScissor(this);
        }
    }
}