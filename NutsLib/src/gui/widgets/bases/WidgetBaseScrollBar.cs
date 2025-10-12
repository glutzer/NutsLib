using System;
using Vintagestory.API.Client;

namespace NutsLib;

/// <summary>
/// Implementation of a scroll bar missing: rendering, textures.
/// At 0 steps per page it will not step at all.
/// </summary>
public class WidgetBaseScrollBar : Widget
{
    // Progress from top (0) to bottom (1).
    protected float scrollProgress = 0;

    // Active = dragging.
    protected EnumButtonState barState = EnumButtonState.Normal;

    protected bool fullBarHovered;
    protected bool hoveringScrollArea;

    protected float barGrabRatio; // Ratio of the position of the mouse to the bar when started dragging.

    // Bounds that will be scrolled/offset.
    // The fixed position will be set as the offset.
    protected Widget? scrollWidget;
    protected int stepsPerPage;

    public WidgetBaseScrollBar(Widget? parent, Widget? scrollWidget, int stepsPerPage = 10) : base(parent)
    {
        this.scrollWidget = scrollWidget;
        this.stepsPerPage = stepsPerPage;
    }

    public override void OnRender(float dt, NuttyShader shader)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        RenderBackground(X, Y, Width, Height, shader);
        RenderCursor(X, Y + offset, Width, size, shader, barState);
    }

    public void SetScrollArea(Widget scrollWidget)
    {
        this.scrollWidget = scrollWidget;
        Reset();
    }

    protected virtual void RenderBackground(int x, int y, int width, int height, NuttyShader shader)
    {

    }

    protected virtual void RenderCursor(int x, int y, int width, int height, NuttyShader shader, EnumButtonState barState)
    {

    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseMove += GuiEvents_MouseMove;
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
        guiEvents.MouseWheel += GuiEvents_MouseWheel;
    }

    private void GuiEvents_MouseWheel(MouseWheelEventArgs obj)
    {
        if (!obj.IsHandled && (fullBarHovered || hoveringScrollArea))
        {
            obj.SetHandled();

            if (stepsPerPage == 0) return; // Not implemented.

            int steps = GetSteps(GetScrollBarRatio());
            float oneStepProgress = 1f / steps;

            scrollProgress -= obj.delta * oneStepProgress;

            scrollProgress = MathF.Round(scrollProgress * steps) / steps;
            scrollProgress = Math.Clamp(scrollProgress, 0, 1);

            SetOffset();
        }
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (scrollWidget == null) return;

        if (barState == EnumButtonState.Active)
        {
            MoveBar(obj.Y);
        }

        hoveringScrollArea = scrollWidget.IsInAllBounds(obj);

        if (obj.Handled)
        {
            fullBarHovered = false;
            barState = EnumButtonState.Normal;
        }
        else
        {
            fullBarHovered = IsInAllBounds(obj);

            if (barState != EnumButtonState.Active) barState = fullBarHovered && IsMouseOnScrollBar(obj.X, obj.Y) ? EnumButtonState.Hovered : EnumButtonState.Normal;

            if (IsInAllBounds(obj))
            {
                obj.Handled = true;
            }
        }
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && IsInsideAndClip(obj) && IsMouseOnScrollBar(obj.X, obj.Y))
        {
            obj.Handled = true;

            barState = EnumButtonState.Active;
            SetScrollBarGrabRatio(obj.Y);
        }
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        barState = IsInAllBounds(obj) && IsMouseOnScrollBar(obj.X, obj.Y) ? EnumButtonState.Hovered : EnumButtonState.Normal;
    }

    /// <summary>
    /// Get the ratio of the scroll cursor to the bounds.
    /// </summary>
    protected float GetScrollBarRatio()
    {
        return scrollWidget == null ? 1f : Math.Clamp((float)Height / scrollWidget.Height, 0, 1);
    }

    /// <summary>
    /// Get width or height of scroll bar.
    /// </summary>
    protected int GetScrollBarSize(float scrollBarRatio)
    {
        return (int)MathF.Round(Height * scrollBarRatio);
    }

    /// <summary>
    /// Get the offset from the beginning of the bounds to the beginning of the scroll bar.
    /// </summary>
    protected int GetScrollBarOffset(float scrollBarSize)
    {
        return (int)MathF.Round((Height - scrollBarSize) * scrollProgress);
    }

    protected bool IsMouseOnScrollBar(int x, int y)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        return x >= X && x <= X + Width && y >= Y + offset && y <= Y + offset + size;
    }

    /// <summary>
    /// If a scroll bar is clicked, sets where it started grabbing.
    /// </summary>
    protected void SetScrollBarGrabRatio(int mouseY)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        float startY = Y + offset;
        float endY = Y + offset + size;

        barGrabRatio = (mouseY - startY) / (endY - startY);
        barGrabRatio = MathF.Round(Math.Clamp(barGrabRatio, 0, 1), 1);
    }

    /// <summary>
    /// When moving mouse if dragging, move the bar here.
    /// </summary>
    protected void MoveBar(int mouseY)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);

        // Offset mouse position up to simulate grabbing the bar at that position.
        float relativePosition = mouseY - Y - (barGrabRatio * size);

        scrollProgress = relativePosition / (Height - size);

        if (stepsPerPage > 0)
        {
            int steps = GetSteps(ratio);
            scrollProgress = MathF.Round(scrollProgress * steps) / steps;
        }

        scrollProgress = Math.Clamp(scrollProgress, 0, 1);

        SetOffset();
    }

    /// <summary>
    /// Set the new offset of the bounds being scrolled.
    /// </summary>
    protected void SetOffset()
    {
        if (scrollWidget == null) return;

        float offset = (scrollWidget.Height - Height) * scrollProgress;

        // Prevent scroll bar bigger than bounds from going up, probably a bigger issue.
        if (offset < 0) offset = 0;

        scrollWidget.FixedPos(0, -(int)offset);
        scrollWidget.SetBounds();
    }

    /// <summary>
    /// Get how many steps this scroll should be separated into per page.
    /// </summary>
    protected int GetSteps(float scrollBarRatio)
    {
        return (int)(1 / scrollBarRatio * stepsPerPage);
    }

    /// <summary>
    /// Resets the scroll bar to the default position.
    /// </summary>
    public void Reset()
    {
        scrollProgress = 0;
        SetOffset();
    }
}