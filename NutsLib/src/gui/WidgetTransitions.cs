using OpenTK.Mathematics;
using Vintagestory.API.MathTools;

namespace NutsLib;

/// <summary>
/// A transition is added to a gui system to move a widget over time, in the before stage.
/// </summary>
public interface IWidgetTransition
{
    Action? OnComplete { get; set; }
    void UpdateTransition(float dt);
    bool IsComplete();
}

public class FixedPosTransition : IWidgetTransition
{
    public Action? OnComplete { get; set; }

    private readonly Widget widget;

    private float accum;
    private readonly float duration;

    private Vector2 currentPos;
    private Vector2 targetPos;

    public FixedPosTransition(Widget widget, float duration, Vector2 currentPos, Vector2 targetPos, Action? onComplete)
    {
        this.widget = widget;
        this.duration = duration;
        this.currentPos = currentPos;
        this.targetPos = targetPos;
        OnComplete = onComplete;
    }

    public bool IsComplete()
    {
        return accum == duration;
    }

    public void UpdateTransition(float dt)
    {
        accum += dt;
        accum = Math.Clamp(accum, 0f, duration);
        float t = accum / duration;

        Vector2 pos = Vector2.Lerp(currentPos, targetPos, t);

        widget.Move(pos.X, pos.Y);
    }
}

public class FixedSizeTransition : IWidgetTransition
{
    public Action? OnComplete { get; set; }

    private readonly Widget widget;

    private float accum;
    private readonly float duration;

    private Vector2 currentSize;
    private Vector2 targetSize;

    public FixedSizeTransition(Widget widget, float duration, Vector2 currentSize, Vector2 targetSize, Action? onComplete)
    {
        this.widget = widget;
        this.duration = duration;
        this.currentSize = currentSize;
        this.targetSize = targetSize;
        OnComplete = onComplete;
    }

    public bool IsComplete()
    {
        return accum == duration;
    }

    public void UpdateTransition(float dt)
    {
        accum += dt;
        accum = Math.Clamp(accum, 0f, duration);
        float t = accum / duration;

        Vector2 size = Vector2.Lerp(currentSize, targetSize, t);

        widget.Resize(size.X, size.Y);
    }
}

public class PercentPosTransition : IWidgetTransition
{
    public Action? OnComplete { get; set; }

    private readonly Widget widget;

    private float accum;
    private readonly float duration;

    private Vector2 currentPos;
    private Vector2 targetPos;

    public PercentPosTransition(Widget widget, float duration, Vector2 currentPos, Vector2 targetPos, Action? onComplete)
    {
        this.widget = widget;
        this.duration = duration;
        this.currentPos = currentPos;
        this.targetPos = targetPos;
        OnComplete = onComplete;
    }

    public bool IsComplete()
    {
        return accum == duration;
    }

    public void UpdateTransition(float dt)
    {
        accum += dt;
        accum = Math.Clamp(accum, 0f, duration);
        float t = accum / duration;

        Vector2 pos = Vector2.Lerp(currentPos, targetPos, t);

        widget.MovePercent(pos.X, pos.Y);
    }
}

public class PercentSizeTransition : IWidgetTransition
{
    public Action? OnComplete { get; set; }

    private readonly Widget widget;

    private float accum;
    private readonly float duration;

    private Vector2 currentSize;
    private Vector2 targetSize;

    public PercentSizeTransition(Widget widget, float duration, Vector2 currentSize, Vector2 targetSize, Action? onComplete)
    {
        this.widget = widget;
        this.duration = duration;
        this.currentSize = currentSize;
        this.targetSize = targetSize;
        OnComplete = onComplete;
    }

    public bool IsComplete()
    {
        return accum == duration;
    }

    public void UpdateTransition(float dt)
    {
        accum += dt;
        accum = Math.Clamp(accum, 0f, duration);
        float t = accum / duration;

        Vector2 size = Vector2.Lerp(currentSize, targetSize, t);

        widget.ResizePercent(size.X, size.Y);
    }
}

public class FadeTransition : IWidgetTransition
{
    public Action? OnComplete { get; set; }

    private readonly Widget widget;

    private float accum;
    private readonly float duration;

    private readonly float currentFade;
    private readonly float targetFade;

    public FadeTransition(Widget widget, float duration, float targetFade, Action? onComplete)
    {
        this.widget = widget;
        this.duration = duration;
        currentFade = widget.SetFade;
        this.targetFade = targetFade;
        OnComplete = onComplete;
    }

    public bool IsComplete()
    {
        return accum == duration;
    }

    public void UpdateTransition(float dt)
    {
        accum += dt;
        accum = Math.Clamp(accum, 0f, duration);
        float t = accum / duration;
        widget.SetFade = GameMath.Lerp(currentFade, targetFade, t);
    }
}

public abstract partial class Widget
{
    public void TransitionToFixedPos(float x, float y, float duration = 0.25f, Action? onComplete = null)
    {
        TransitionManager.Instance.RegisterTransition(new FixedPosTransition(this, duration, new Vector2(xPos, yPos), new Vector2(x, y), onComplete));
    }

    public void TransitionToPercentPos(float x, float y, float duration = 0.25f, Action? onComplete = null)
    {
        TransitionManager.Instance.RegisterTransition(new PercentPosTransition(this, duration, new Vector2(xPos, yPos), new Vector2(x, y), onComplete));
    }

    public void TransitionToFixedSize(float w, float h, float duration = 0.25f, Action? onComplete = null)
    {
        TransitionManager.Instance.RegisterTransition(new FixedSizeTransition(this, duration, new Vector2(xWidth, yHeight), new Vector2(w, h), onComplete));
    }

    public void TransitionToPercentSize(float w, float h, float duration = 0.25f, Action? onComplete = null)
    {
        TransitionManager.Instance.RegisterTransition(new PercentSizeTransition(this, duration, new Vector2(xWidth, yHeight), new Vector2(w, h), onComplete));
    }

    public void FadeTo(float alpha, float duration = 0.25f, Action? onComplete = null)
    {
        TransitionManager.Instance.RegisterTransition(new FadeTransition(this, duration, 1f - alpha, onComplete));
    }
}