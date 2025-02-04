using System;
using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Implementation of a scroll bar missing: rendering, textures.
/// At 0 steps per page it will not step at all.
/// </summary>
public class BaseScrollBarWidget : Widget
{
    // Progress from top (0) to bottom (1).
    protected float scrollProgress = 0;

    // Active = dragging.
    protected ButtonState barState = ButtonState.Normal;

    protected bool fullBarHovered;
    protected bool hoveringScrollArea;

    protected float barGrabRatio; // Ratio of the position of the mouse to the bar when started dragging.

    // Bounds that will be scrolled/offset.
    // The fixed position will be set as the offset.
    protected Bounds scrollBounds;
    protected int stepsPerPage;

    public BaseScrollBarWidget(Gui gui, Bounds bounds, Bounds scrollBounds, int stepsPerPage = 10) : base(gui, bounds)
    {
        this.scrollBounds = scrollBounds;
        this.stepsPerPage = stepsPerPage;
    }

    public override void OnRender(float dt, MareShader shader)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        RenderBackground(bounds.X, bounds.Y, bounds.Width, bounds.Height, shader);
        RenderCursor(bounds.X, bounds.Y + offset, bounds.Width, size, shader, barState);
    }

    protected virtual void RenderBackground(int x, int y, int width, int height, MareShader shader)
    {

    }

    protected virtual void RenderCursor(int x, int y, int width, int height, MareShader shader, ButtonState barState)
    {

    }

    public void SetNewScrollBounds(Bounds scrollBounds)
    {
        this.scrollBounds = scrollBounds;
        Reset();
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
        if (barState == ButtonState.Active)
        {
            MoveBar(obj.Y);
            return;
        }

        if (obj.Handled) return;

        hoveringScrollArea = scrollBounds.IsInAllBounds(obj);
        fullBarHovered = bounds.IsInsideAndClip(obj);

        if (bounds.IsInsideAndClip(obj) && IsMouseOnScrollBar(obj.X, obj.Y))
        {
            barState = ButtonState.Hovered;
        }
        else
        {
            barState = ButtonState.Normal;
        }
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && bounds.IsInsideAndClip(obj) && IsMouseOnScrollBar(obj.X, obj.Y))
        {
            obj.Handled = true;

            barState = ButtonState.Active;
            SetScrollBarGrabRatio(obj.Y);
        }
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        if (barState != ButtonState.Active) return;

        if (bounds.IsInsideAndClip(obj) && IsMouseOnScrollBar(obj.X, obj.Y))
        {
            barState = ButtonState.Hovered;
        }
        else
        {
            barState = ButtonState.Normal;
        }
    }

    /// <summary>
    /// Get the ratio of the scroll cursor to the bounds.
    /// </summary>
    protected float GetScrollBarRatio()
    {
        return Math.Clamp((float)bounds.Height / scrollBounds.Height, 0, 1);
    }

    /// <summary>
    /// Get width or height of scroll bar.
    /// </summary>
    protected int GetScrollBarSize(float scrollBarRatio)
    {
        return (int)MathF.Round(bounds.Height * scrollBarRatio);
    }

    /// <summary>
    /// Get the offset from the beginning of the bounds to the beginning of the scroll bar.
    /// </summary>
    protected int GetScrollBarOffset(float scrollBarSize)
    {
        return (int)MathF.Round((bounds.Height - scrollBarSize) * scrollProgress);
    }

    protected bool IsMouseOnScrollBar(int x, int y)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        return x >= bounds.X && x <= bounds.X + bounds.Width && y >= bounds.Y + offset && y <= bounds.Y + offset + size;
    }

    /// <summary>
    /// If a scroll bar is clicked, sets where it started grabbing.
    /// </summary>
    protected void SetScrollBarGrabRatio(int mouseY)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        float startY = bounds.Y + offset;
        float endY = bounds.Y + offset + size;

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
        float relativePosition = mouseY - bounds.Y - (barGrabRatio * size);

        scrollProgress = relativePosition / (bounds.Height - size);

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
        float offset = (scrollBounds.Height - bounds.Height) * scrollProgress;

        // Prevent scroll bar bigger than bounds from going up, probably a bigger issue.
        if (offset < 0) offset = 0;

        scrollBounds.FixedPos(0, -(int)offset / scrollBounds.Scale);
        scrollBounds.SetBounds();
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