using OpenTK.Mathematics;
using System;

namespace MareLib;

/// <summary>
/// Animator for animating the position and size of something.
/// </summary>
public class GuiAnimator
{
    private Vector2 lastPos;
    private Vector2 lastSize;

    private Vector2 targetPos;
    private Vector2 targetSize;

    public Vector2 Pos { get; private set; }
    public Vector2 Size { get; private set; }

    private float accum;
    private float duration;

    public GuiAnimator(Bounds currentState)
    {
        lastPos = new Vector2(currentState.X, currentState.Y);
        lastSize = new Vector2(currentState.Width, currentState.Height);
        targetPos = new Vector2(currentState.X, currentState.Y);
        targetSize = new Vector2(currentState.Width, currentState.Height);
    }

    public GuiAnimator(int x, int y, int w, int h)
    {
        lastPos = new Vector2(x, y);
        lastSize = new Vector2(w, h);
        targetPos = new Vector2(x, y);
        targetSize = new Vector2(w, h);
        Pos = new Vector2(x, y);
        Size = new Vector2(w, h);
    }

    public void LerpTo(int x, int y, float duration)
    {
        lastPos = Pos;
        targetPos = new Vector2(x, y);
        accum = 0;
        this.duration = duration;
    }

    public void LerpSize(int w, int h, float duration)
    {
        lastSize = Size;
        targetSize = new Vector2(w, h);
        accum = 0;
        this.duration = duration;
    }

    public void LerpTo(int x, int y, int w, int h, float duration)
    {
        LerpTo(x, y, duration);
        LerpSize(w, h, duration);
    }

    /// <summary>
    /// Progress this ui animation.
    /// </summary>
    public void Progress(float dt)
    {
        accum += dt;
        float t = Math.Min(accum / duration, 1);
        Pos = Vector2.Lerp(lastPos, targetPos, t);
        Size = Vector2.Lerp(lastSize, targetSize, t);
    }
}