using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Bounds used by ui elements/scissor etc.
/// </summary>
public class Bounds
{
    public Bounds? parentBounds;

    public List<Bounds>? children;

    public bool NoScale { get; private set; }
    private bool alignOutsideH;
    private bool alignOutsideV;
    private BoundsSizing positioningH;
    private BoundsSizing positioningV;
    private BoundsSizing sizingH;
    private BoundsSizing sizingV;
    private Align alignment;

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

    public int XCenter => X + (Width / 2);
    public int YCenter => Y + (Height / 2);

    private readonly int frameWidth;
    private readonly int frameHeight;

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

        int guiScale = MainAPI.GuiScale;
        if (parentBounds?.NoScale == true) NoScaling(); // Don't scale if parent doesn't scale.
        if (NoScale) guiScale = 1;

        Scale = guiScale;

        // X and Y were previously multiplied by gui scale, don't do that now?

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
        if (children != null)
        {
            foreach (Bounds child in children)
            {
                child.SetBounds();
            }
        }

        if (cWidth != Width || cHeight != Height) OnResize?.Invoke();
    }

    private void SetAlignments(int parentWidth, int parentHeight)
    {
        switch (alignment)
        {
            case Align.None:
                break;
            case Align.LeftTop: // These are both effectively the same.
                if (alignOutsideH) scaledX -= scaledWidth;
                if (alignOutsideV) scaledY -= scaledHeight;
                break;
            case Align.LeftMiddle:
                if (alignOutsideH) scaledX -= scaledWidth;
                scaledY += (parentHeight / 2) - (scaledHeight / 2);
                break;
            case Align.LeftBottom:
                if (alignOutsideH) scaledX -= scaledWidth;
                if (alignOutsideV) scaledY += scaledHeight;
                scaledY += parentHeight - scaledHeight;
                break;
            case Align.CenterTop:
                if (alignOutsideV) scaledY -= scaledHeight;
                scaledX += (parentWidth / 2) - (scaledWidth / 2);
                break;
            case Align.Center: // Both of these for absolute center.
                scaledX += (parentWidth / 2) - (scaledWidth / 2);
                scaledY += (parentHeight / 2) - (scaledHeight / 2);
                break;
            case Align.CenterBottom:
                if (alignOutsideV) scaledY += scaledHeight;
                scaledX += (parentWidth / 2) - (scaledWidth / 2);
                scaledY += parentHeight - scaledHeight;
                break;
            case Align.RightTop:
                if (alignOutsideH) scaledX += scaledWidth;
                if (alignOutsideV) scaledY -= scaledHeight;
                scaledX += parentWidth - scaledWidth;
                break;
            case Align.RightMiddle:
                if (alignOutsideH) scaledX += scaledWidth;
                scaledX += parentWidth - scaledWidth;
                scaledY += (parentHeight / 2) - (scaledHeight / 2);
                break;
            case Align.RightBottom:
                if (alignOutsideH) scaledX += scaledWidth;
                if (alignOutsideV) scaledY += scaledHeight;
                scaledX += parentWidth - scaledWidth;
                scaledY += parentHeight - scaledHeight;
                break;
        }
    }

    private Bounds(int frameWidth, int frameHeight)
    {
        this.frameWidth = frameWidth;
        this.frameHeight = frameHeight;
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
        return new Bounds(MainAPI.RenderWidth, MainAPI.RenderHeight);
    }

    /// <summary>
    /// Create a top level bounds.
    /// </summary>
    public static Bounds Create(int renderWidth, int renderHeight)
    {
        return new Bounds(renderWidth, renderHeight);
    }

    public static Bounds CreateFrom(Bounds parent)
    {
        return new Bounds(parent);
    }

    public Bounds NoScaling()
    {
        NoScale = true;
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

    public Bounds Alignment(Align boundsAlignment, bool alignOutsideH = false, bool alignOutsideV = false)
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
        parent.children ??= new List<Bounds>();
        parent.children.Add(this);

        // If the parent bounds does not scale, neither should this.
        if (parent.NoScale) NoScaling();

        return this;
    }

    // Transform checks.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInAllBounds(int x, int y, int boundsX, int boundsY, int boundsWidth, int boundsHeight)
    {
        if (RenderTools.GuiTransformStack.Count > 1)
        {
            Matrix4 currentTransform = RenderTools.GuiTransformStack.Peek();

            Vector4 startVector = new Vector4(boundsX, boundsY, 0, 1) * currentTransform;
            Vector4 endVector = new Vector4(boundsX + boundsWidth, boundsY + boundsHeight, 0, 1) * currentTransform;

            Vector4 min = Vector4.ComponentMin(startVector, endVector);
            Vector4 max = Vector4.ComponentMax(startVector, endVector);

            bool isInside = x >= min.X && x <= max.X && y >= min.Y && y <= max.Y;

            return isInside && RenderTools.IsPointInsideScissor(x, y);
        }
        else
        {
            return x >= boundsX && x <= boundsX + boundsWidth && y >= boundsY && y <= boundsY + boundsHeight
            && RenderTools.IsPointInsideScissor(x, y);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInAllBounds(MouseEvent mouseEvent)
    {
        return IsInAllBounds(mouseEvent.X, mouseEvent.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInAllBounds(int x, int y)
    {
        return IsInAllBounds(x, (float)y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInAllBounds(float x, float y)
    {
        if (RenderTools.GuiTransformStack.Count > 1)
        {
            Matrix4 currentTransform = RenderTools.GuiTransformStack.Peek();

            Vector4 startVector = new Vector4(X, Y, 0, 1) * currentTransform;
            Vector4 endVector = new Vector4(X + Width, Y + Height, 0, 1) * currentTransform;

            Vector4 min = Vector4.ComponentMin(startVector, endVector);
            Vector4 max = Vector4.ComponentMax(startVector, endVector);

            bool isInside = x >= min.X && x <= max.X && y >= min.Y && y <= max.Y;

            return isInside && RenderTools.IsPointInsideScissor((int)x, (int)y);
        }
        else
        {
            return IsInside(x, y) && RenderTools.IsPointInsideScissor((int)x, (int)y);
        }
    }

    // Clip + bounds checks.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInsideAndClip(int x, int y, int boundsX, int boundsY, int boundsWidth, int boundsHeight)
    {
        return x >= boundsX && x <= boundsX + boundsWidth && y >= boundsY && y <= boundsY + boundsHeight
            && RenderTools.IsPointInsideScissor(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInsideAndClip(MouseEvent mouseEvent)
    {
        return IsInside(mouseEvent.X, mouseEvent.Y) && RenderTools.IsPointInsideScissor(mouseEvent.X, mouseEvent.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInsideAndClip(int x, int y)
    {
        return IsInside(x, y) && RenderTools.IsPointInsideScissor(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInsideAndClip(float x, float y)
    {
        return IsInside(x, y) && RenderTools.IsPointInsideScissor((int)x, (int)y);
    }

    // Bounds checks.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInside(int x, int y, int boundsX, int boundsY, int boundsWidth, int boundsHeight)
    {
        return x >= boundsX && x <= boundsX + boundsWidth && y >= boundsY && y <= boundsY + boundsHeight;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInside(MouseEvent mouseEvent)
    {
        return mouseEvent.X >= X && mouseEvent.X <= X + Width && mouseEvent.Y >= Y && mouseEvent.Y <= Y + Height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInside(int x, int y)
    {
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInside(float x, float y)
    {
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }

    // Intersecting.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIntersecting(Bounds bounds)
    {
        return X < bounds.X + bounds.Width && X + Width > bounds.X && Y < bounds.Y + bounds.Height && Y + Height > bounds.Y;
    }

    public Vector2i GetFixedPos()
    {
        return new Vector2i(fixedX, fixedY);
    }
}