using Vintagestory.API.Client;

using Vintagestory.API.Common;

namespace NutsLib;

/// <summary>
/// Implementation of a scroll bar missing: rendering, textures.
/// At 0 steps per page scrolling is continuous rather than snapped.
/// </summary>
public class WidgetBaseScrollBar : Widget
{
    // Progress from top (0) to bottom (1).
    protected float scrollProgress = 0f;

    // Active = dragging.
    protected EnumButtonState barState = EnumButtonState.Normal;

    protected bool fullBarHovered;
    protected bool hoveringScrollArea;

    protected float barGrabRatio; // Ratio of the position of the mouse to the bar when started dragging.

    // Bounds that will be scrolled/offset.
    // The fixed position will be set as the offset.
    protected Widget? scrollWidget;
    protected int stepsPerPage;
    protected bool horizontal;

    public WidgetBaseScrollBar(Widget? parent, Gui gui, Widget? scrollWidget, int stepsPerPage = 10, bool horizontal = false) : base(parent, gui)
    {
        this.scrollWidget = scrollWidget;
        this.stepsPerPage = stepsPerPage;
        this.horizontal = horizontal;
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        RenderBackground(X, Y, Width, Height, shader);

        if (horizontal)
        {
            RenderCursor(X + offset, Y, size, Height, shader, barState);
        }
        else
        {
            RenderCursor(X, Y + offset, Width, size, shader, barState);
        }
    }

    public void SetScrollArea(Widget scrollWidget)
    {
        this.scrollWidget = scrollWidget;
        Reset();
    }

    protected virtual void RenderBackground(int x, int y, int width, int height, ShaderGui shader)
    {

    }

    protected virtual void RenderCursor(int x, int y, int width, int height, ShaderGui shader, EnumButtonState barState)
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

            if (GetScrollBarRatio() >= 1f) { Reset(); return; }
            if (stepsPerPage == 0)
            {
                scrollProgress = Math.Clamp(scrollProgress - (obj.delta * 0.05f), 0f, 1f);
                SetOffset();
                return;
            }

            int steps = GetSteps(GetScrollBarRatio());
            float oneStepProgress = 1f / steps;

            scrollProgress -= obj.delta * oneStepProgress;

            scrollProgress = MathF.Round(scrollProgress * steps) / steps;
            scrollProgress = Math.Clamp(scrollProgress, 0f, 1f);

            SetOffset();
        }
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (scrollWidget == null) return;

        if (barState == EnumButtonState.Active)
        {
            MoveBar(horizontal ? obj.X : obj.Y);
            obj.Handled = true;
            return;
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
        if (obj.Button == EnumMouseButton.Left && !obj.Handled && GetScrollBarRatio() < 1f && IsInAllBounds(obj) && IsMouseOnScrollBar(obj.X, obj.Y))
        {
            obj.Handled = true;

            barState = EnumButtonState.Active;
            SetScrollBarGrabRatio(horizontal ? obj.X : obj.Y);
        }
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        if (obj.Button != EnumMouseButton.Left) return;
        if (barState == EnumButtonState.Active) obj.Handled = true;
        barState = IsInAllBounds(obj) && IsMouseOnScrollBar(obj.X, obj.Y) ? EnumButtonState.Hovered : EnumButtonState.Normal;
    }

    /// <summary>
    /// Get the ratio of the scroll cursor to the bounds.
    /// </summary>
    protected float GetScrollBarRatio()
    {
        return scrollWidget == null || scrollWidget.Width <= 0 || scrollWidget.Height <= 0
            ? 1f
            : horizontal
            ? scrollWidget == null ? 1f : Math.Clamp((float)Width / scrollWidget.Width, 0f, 1f)
            : scrollWidget == null ? 1f : Math.Clamp((float)Height / scrollWidget.Height, 0f, 1f);
    }

    /// <summary>
    /// Get width or height of scroll bar.
    /// </summary>
    protected int GetScrollBarSize(float scrollBarRatio)
    {
        return horizontal ? (int)MathF.Round(Width * scrollBarRatio) : (int)MathF.Round(Height * scrollBarRatio);
    }

    /// <summary>
    /// Get the offset from the beginning of the bounds to the beginning of the scroll bar.
    /// </summary>
    protected int GetScrollBarOffset(float scrollBarSize)
    {
        return horizontal
            ? (int)MathF.Round((Width - scrollBarSize) * scrollProgress)
            : (int)MathF.Round((Height - scrollBarSize) * scrollProgress);
    }

    protected bool IsMouseOnScrollBar(int x, int y)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        return horizontal
            ? x >= X + offset && x <= X + offset + size && y >= Y && y <= Y + Height
            : x >= X && x <= X + Width && y >= Y + offset && y <= Y + offset + size;
    }

    /// <summary>
    /// If a scroll bar is clicked, sets where it started grabbing.
    /// </summary>
    protected void SetScrollBarGrabRatio(int mouseY)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        int offset = GetScrollBarOffset(size);

        if (horizontal)
        {
            float startX = X + offset;
            float endX = X + offset + size;
            barGrabRatio = (mouseY - startX) / (endX - startX);
            barGrabRatio = MathF.Round(Math.Clamp(barGrabRatio, 0f, 1f), 1);
        }
        else
        {
            float startY = Y + offset;
            float endY = Y + offset + size;

            barGrabRatio = (mouseY - startY) / (endY - startY);
            barGrabRatio = MathF.Round(Math.Clamp(barGrabRatio, 0f, 1f), 1);
        }
    }

    /// <summary>
    /// When moving mouse if dragging, move the bar here.
    /// </summary>
    protected void MoveBar(int mouseXorY)
    {
        float ratio = GetScrollBarRatio();
        int size = GetScrollBarSize(ratio);
        if ((horizontal ? Width : Height) <= size) { Reset(); return; }

        // Offset mouse position up to simulate grabbing the bar at that position.
        if (horizontal)
        {
            float relativePosition = mouseXorY - X - (barGrabRatio * size);
            scrollProgress = relativePosition / (Width - size);
        }
        else
        {
            float relativePosition = mouseXorY - Y - (barGrabRatio * size);
            scrollProgress = relativePosition / (Height - size);
        }

        if (stepsPerPage > 0)
        {
            int steps = GetSteps(ratio);
            scrollProgress = MathF.Round(scrollProgress * steps) / steps;
        }

        scrollProgress = Math.Clamp(scrollProgress, 0f, 1f);

        SetOffset();
    }

    /// <summary>
    /// Set the new offset of the bounds being scrolled.
    /// </summary>
    protected void SetOffset()
    {
        if (scrollWidget == null) return;

        float offset;

        // Prevent scroll bar bigger than bounds from going up, probably a bigger issue.
        if (horizontal)
        {
            offset = (scrollWidget.Width - Width) * scrollProgress;
            if (offset < 0f) offset = 0f;
            scrollWidget.FixedX(-(int)offset);
        }
        else
        {
            offset = (scrollWidget.Height - Height) * scrollProgress;
            if (offset < 0f) offset = 0f;
            scrollWidget.FixedY(-(int)offset);
        }
    }

    /// <summary>
    /// Get how many steps this scroll should be separated into per page.
    /// </summary>
    protected int GetSteps(float scrollBarRatio)
    {
        return Math.Max(1, (int)(1 / Math.Max(scrollBarRatio, 0.0001f) * Math.Max(1, stepsPerPage)));
    }

    /// <summary>
    /// Resets the scroll bar to the default position.
    /// </summary>
    public void Reset()
    {
        scrollProgress = 0f;
        SetOffset();
    }
}
