using System;
using System.Collections.Generic;

namespace MareLib;

/// <summary>
/// Bounds used by ui elements/scissor etc.
/// </summary>
public class Bounds
{
    public Bounds? parentBounds;

    public List<Bounds> children = new(4);

    public bool ShouldScale => !noScaling && sizingH != BoundsSizing.PercentSize && sizingV != BoundsSizing.PercentSize;
    private bool noScaling;
    private bool alignOutsideH;
    private bool alignOutsideV;
    private BoundsSizing positioningH;
    private BoundsSizing positioningV;
    private BoundsSizing sizingH;
    private BoundsSizing sizingV;
    private BoundsAlignment alignment;

    private int fixedX;
    private int fixedY;
    private int fixedWidth;
    private int fixedHeight;

    private float percentX;
    private float percentY;
    private float percentWidth;
    private float percentHeight;

    private int scaledX;
    private int scaledY;
    private int scaledWidth;
    private int scaledHeight;

    // Publicly available for rendering.
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Scale { get; private set; }

    /// <summary>
    /// Called when bounds are set if they are a different size.
    /// </summary>
    public event Action? OnResize;

    /// <summary>
    /// Top-most bounds.
    /// </summary>
    public Bounds MainBounds
    {
        get
        {
            if (parentBounds != null)
            {
                return parentBounds.MainBounds;
            }

            return this;
        }
    }

    /// <summary>
    /// Sets bounds of every connected bounds.
    /// </summary>
    public void SetBoundsFromOrigin()
    {
        if (parentBounds == null)
        {
            SetBounds();
            return;
        }
        parentBounds.SetBoundsFromOrigin();
    }

    public void SetBounds()
    {
        int cWidth = Width;
        int cHeight = Height;

        // Implement scaling here...
        int guiScale = MainHook.GuiScale;
        if (noScaling) guiScale = 1;

        Scale = guiScale;

        int frameWidth = MainHook.RenderWidth;
        int frameHeight = MainHook.RenderHeight;

        scaledX = positioningH switch
        {
            BoundsSizing.FixedSize => fixedX * guiScale,
            _ => parentBounds == null ? (int)(percentX * frameWidth) : (int)(percentX * parentBounds.Width)
        };

        scaledY = positioningV switch
        {
            BoundsSizing.FixedSize => fixedY * guiScale,
            _ => parentBounds == null ? (int)(percentY * frameHeight) : (int)(percentY * parentBounds.Height)
        };

        scaledWidth = sizingH switch
        {
            BoundsSizing.FixedSize => fixedWidth * guiScale,
            _ => parentBounds == null ? (int)(percentWidth * frameWidth) : (int)(percentWidth * parentBounds.Width)
        };

        scaledHeight = sizingV switch
        {
            BoundsSizing.FixedSize => fixedHeight * guiScale,
            _ => parentBounds == null ? (int)(percentHeight * frameHeight) : (int)(percentHeight * parentBounds.Height)
        };

        // If scaled width = 0, non-initialized. No gui if it's 0.
        // Will get called on second pass.
        if (parentBounds == null || parentBounds?.scaledWidth != 0)
        {
            if (parentBounds == null)
            {
                SetAlignments(frameWidth, frameHeight);
                SetRenderPos(0, 0);
            }
            else
            {
                SetAlignments(parentBounds.Width, parentBounds.Height);
                SetRenderPos(parentBounds.X, parentBounds.Y);
            }
        }

        // Initialize children.
        foreach (Bounds child in children)
        {
            child.SetBounds();
        }

        if (cWidth != Width || cHeight != Height) OnResize?.Invoke();
    }

    private void SetAlignments(int parentWidth, int parentHeight)
    {
        switch (alignment)
        {
            case BoundsAlignment.None:
                break;
            case BoundsAlignment.LeftTop: // These are both effectively the same.
                if (alignOutsideH) scaledX -= scaledWidth;
                if (alignOutsideV) scaledY -= scaledHeight;
                break;
            case BoundsAlignment.LeftMiddle:
                if (alignOutsideH) scaledX -= scaledWidth;
                scaledY += (parentHeight / 2) - (scaledHeight / 2);
                break;
            case BoundsAlignment.LeftBottom:
                if (alignOutsideH) scaledX -= scaledWidth;
                if (alignOutsideV) scaledY += scaledHeight;
                scaledY += parentHeight - scaledHeight;
                break;
            case BoundsAlignment.CenterTop:
                if (alignOutsideV) scaledY -= scaledHeight;
                scaledX += (parentWidth / 2) - (scaledWidth / 2);
                break;
            case BoundsAlignment.Center: // Both of these for absolute center.
                scaledX += (parentWidth / 2) - (scaledWidth / 2);
                scaledY += (parentHeight / 2) - (scaledHeight / 2);
                break;
            case BoundsAlignment.CenterBottom:
                if (alignOutsideV) scaledY += scaledHeight;
                scaledX += (parentWidth / 2) - (scaledWidth / 2);
                scaledY += parentHeight - scaledHeight;
                break;
            case BoundsAlignment.RightTop:
                if (alignOutsideH) scaledX += scaledWidth;
                if (alignOutsideV) scaledY -= scaledHeight;
                scaledX += parentWidth - scaledWidth;
                break;
            case BoundsAlignment.RightMiddle:
                if (alignOutsideH) scaledX += scaledWidth;
                scaledX += parentWidth - scaledWidth;
                scaledY += (parentHeight / 2) - (scaledHeight / 2);
                break;
            case BoundsAlignment.RightBottom:
                if (alignOutsideH) scaledX += scaledWidth;
                if (alignOutsideV) scaledY += scaledHeight;
                scaledX += parentWidth - scaledWidth;
                scaledY += parentHeight - scaledHeight;
                break;
        }
    }

    private Bounds()
    {

    }

    private Bounds(Bounds parent)
    {
        SetParent(parent);
    }

    /// <summary>
    /// Create a top level bounds.
    /// </summary>
    public static Bounds Create()
    {
        return new Bounds();
    }

    public static Bounds CreateFrom(Bounds parent)
    {
        return new Bounds(parent);
    }

    public Bounds NoScaling()
    {
        noScaling = true;
        return this;
    }

    public void SetRenderPos(int parentRenderX, int parentRenderY)
    {
        X = scaledX + parentRenderX;
        Y = scaledY + parentRenderY;
        Width = scaledWidth;
        Height = scaledHeight;
    }

    /// <summary>
    /// Moves fixed position of the element and sets bounds.
    /// </summary>
    public void Move(int newX, int newY)
    {
        fixedX = newX;
        fixedY = newY;

        SetBounds();
    }

    public bool IsInside(int x, int y)
    {
        // Return if the x and y are inside the render bounds.
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }

    public bool IsInside(float x, float y)
    {
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }

    public bool IsInside(double x, double y)
    {
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }

    public bool IsIntersecting(Bounds bounds)
    {
        return X < bounds.X + bounds.Width && X + Width > bounds.X && Y < bounds.Y + bounds.Height && Y + Height > bounds.Y;
    }

    public Bounds FixedX(int x)
    {
        positioningH = BoundsSizing.FixedSize;

        fixedX = x;
        return this;
    }

    public Bounds FixedY(int y)
    {
        positioningV = BoundsSizing.FixedSize;

        fixedY = y;
        return this;
    }

    public Bounds FixedPos(int x, int y)
    {
        positioningH = BoundsSizing.FixedSize;
        positioningV = BoundsSizing.FixedSize;

        fixedX = x;
        fixedY = y;
        return this;
    }

    public Bounds FixedWidth(int width)
    {
        sizingH = BoundsSizing.FixedSize;

        fixedWidth = width;
        return this;
    }

    public Bounds FixedHeight(int height)
    {
        sizingV = BoundsSizing.FixedSize;

        fixedHeight = height;
        return this;
    }

    public Bounds FixedSize(int width, int height)
    {
        sizingH = BoundsSizing.FixedSize;
        sizingV = BoundsSizing.FixedSize;

        fixedWidth = width;
        fixedHeight = height;
        return this;
    }

    public Bounds Fixed(int x, int y, int width, int height)
    {
        positioningH = BoundsSizing.FixedSize;
        positioningV = BoundsSizing.FixedSize;
        sizingH = BoundsSizing.FixedSize;
        sizingV = BoundsSizing.FixedSize;

        fixedX = x;
        fixedY = y;
        fixedWidth = width;
        fixedHeight = height;
        return this;
    }

    public Bounds PercentX(float x)
    {
        positioningH = BoundsSizing.PercentSize;

        percentX = x;
        return this;
    }

    public Bounds PercentY(float y)
    {
        positioningV = BoundsSizing.PercentSize;

        percentY = y;
        return this;
    }

    public Bounds PercentPos(float x, float y)
    {
        positioningH = BoundsSizing.PercentSize;
        positioningV = BoundsSizing.PercentSize;

        percentX = x;
        percentY = y;
        return this;
    }

    public Bounds PercentWidth(float width)
    {
        sizingH = BoundsSizing.PercentSize;

        percentWidth = width;
        return this;
    }

    public Bounds PercentHeight(float height)
    {
        sizingV = BoundsSizing.PercentSize;

        percentHeight = height;
        return this;
    }

    public Bounds PercentSize(float width, float height)
    {
        sizingH = BoundsSizing.PercentSize;
        sizingV = BoundsSizing.PercentSize;

        percentWidth = width;
        percentHeight = height;
        return this;
    }

    public Bounds Percent(float x, float y, float width, float height)
    {
        positioningH = BoundsSizing.PercentSize;
        positioningV = BoundsSizing.PercentSize;
        sizingH = BoundsSizing.PercentSize;
        sizingV = BoundsSizing.PercentSize;

        percentX = x;
        percentY = y;
        percentWidth = width;
        percentHeight = height;
        return this;
    }

    public Bounds Alignment(BoundsAlignment boundsAlignment, bool alignOutsideH = false, bool alignOutsideV = false)
    {
        alignment = boundsAlignment;
        this.alignOutsideH = alignOutsideH;
        this.alignOutsideV = alignOutsideV;
        return this;
    }

    public Bounds Copy(Bounds parent = null!)
    {
        parent ??= parentBounds!;
        return new Bounds(parent)
        {
            fixedX = fixedX,
            fixedY = fixedY + fixedHeight,
            fixedWidth = fixedWidth,
            fixedHeight = fixedHeight,
            percentX = percentX,
            percentY = percentY + percentHeight,
            percentWidth = percentWidth,
            percentHeight = percentHeight,
            alignment = alignment,
            sizingH = sizingH,
            sizingV = sizingV,
            positioningH = positioningH,
            positioningV = positioningV
        };
    }

    public Bounds CopyDown(Bounds parent = null!, int fixedDelta = 0, float percentDelta = 0)
    {
        parent ??= parentBounds!;
        return new Bounds(parent)
        {
            fixedX = fixedX,
            fixedY = fixedY + fixedHeight + fixedDelta,
            fixedWidth = fixedWidth,
            fixedHeight = fixedHeight,
            percentX = percentX,
            percentY = percentY + percentHeight + percentDelta,
            percentWidth = percentWidth,
            percentHeight = percentHeight,
            alignment = alignment,
            sizingH = sizingH,
            sizingV = sizingV,
            positioningH = positioningH,
            positioningV = positioningV
        };
    }

    public Bounds CopyRight(Bounds parent = null!, int fixedDelta = 0, float percentDelta = 0)
    {
        parent ??= parentBounds!;
        return new Bounds(parent)
        {
            fixedX = fixedX + fixedWidth + fixedDelta,
            fixedY = fixedY,
            fixedWidth = fixedWidth,
            fixedHeight = fixedHeight,
            percentX = percentX + percentWidth + percentDelta,
            percentY = percentY,
            percentWidth = percentWidth,
            percentHeight = percentHeight,
            alignment = alignment,
            sizingH = sizingH,
            sizingV = sizingV,
            positioningH = positioningH,
            positioningV = positioningV
        };
    }

    public Bounds SetParent(Bounds parent)
    {
        parentBounds = parent;
        parent?.children.Add(this);
        return this;
    }
}