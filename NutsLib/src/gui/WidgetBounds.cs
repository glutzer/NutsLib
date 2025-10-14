using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;

namespace NutsLib;

[Flags]
public enum ChildSizing
{
    None = 0,
    Width = 1 << 0,
    Height = 1 << 1,
    Once = 1 << 2
}

public enum BoundsSizing
{
    FixedSize,
    PercentSize
}

[Flags]
public enum AlignFlags
{
    None = 0,
    OutsideH = 1 << 0,
    OutsideV = 1 << 1
}

public enum Align
{
    None,
    LeftTop,
    LeftMiddle,
    LeftBottom,
    CenterTop,
    Center,
    CenterBottom,
    RightTop,
    RightMiddle,
    RightBottom
}

/// <summary>
/// Bounds are merged with widgets because:
/// A bounds is only ever used by widgets, usually only once.
/// Keeping track of parent/children with separated bounds is difficult.
/// Have to initialize 2 objects.
/// 
/// A widget must know about it's parents to set bounds, so when adding/removing children that must be handled now.
/// </summary>
public abstract partial class Widget
{
    public bool NoScale { get; private set; }

    private BoundsSizing positioningH;
    private BoundsSizing positioningV;
    private BoundsSizing sizingH;
    private BoundsSizing sizingV;
    private Align alignment;
    private AlignFlags alignmentFlags;
    private ChildSizing childSizing;

    // Either fixed or percentage positioning.
    private float xPos;
    private float yPos;

    private float xWidth;
    private float yHeight;

    // Publicly available for rendering.
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Scale { get; private set; }

    public int XCenter => X + (Width / 2);
    public int YCenter => Y + (Height / 2);

    public float SetFade { get; set; }
    public float Fade { get; private set; }

    public event Action? OnResize;

    public Widget NoScaling(bool noScale = true)
    {
        NoScale = noScale;
        return this;
    }

    // Topmost widget.
    public Widget MainWidget => Parent ?? this;

    // Bounds setting.
    public void SetBounds()
    {
        int cWidth = Width;
        int cHeight = Height;

        int guiScale = MainAPI.GuiScale;
        if (Parent?.NoScale == true) NoScale = true; // Don't scale if parent doesn't scale.
        if (NoScale) guiScale = 1;

        Scale = guiScale;

        // Set current fade.
        Fade = Parent?.Fade ?? 0f;
        Fade += SetFade;

        float scaledX = positioningH switch
        {
            BoundsSizing.FixedSize => xPos/* * guiScale*/,
            _ => Parent == null ? (int)(xPos * MainAPI.RenderWidth) : (int)(xPos * Parent.Width)
        };

        // Don't scale fixed offsets.

        float scaledY = positioningV switch
        {
            BoundsSizing.FixedSize => yPos/* * guiScale*/,
            _ => Parent == null ? (int)(yPos * MainAPI.RenderHeight) : (int)(yPos * Parent.Height)
        };

        float scaledWidth = sizingH switch
        {
            BoundsSizing.FixedSize => xWidth * guiScale,
            _ => Parent == null ? (int)(xWidth * MainAPI.RenderWidth) : (int)(xWidth * Parent.Width)
        };

        float scaledHeight = sizingV switch
        {
            BoundsSizing.FixedSize => yHeight * guiScale,
            _ => Parent == null ? (int)(yHeight * MainAPI.RenderHeight) : (int)(yHeight * Parent.Height)
        };

        // Set based on the screen size if no parent.
        if (Parent == null)
        {
            Vector4 scaledScreen = new(scaledX, scaledY, scaledWidth, scaledHeight);
            scaledScreen = SetAlignments(MainAPI.RenderWidth, MainAPI.RenderHeight, scaledScreen);
            SetRenderPos(0, 0, scaledScreen);
        }
        else
        {
            Vector4 scaledParent = new(scaledX, scaledY, scaledWidth, scaledHeight);
            scaledParent = SetAlignments(Parent.Width, Parent.Height, scaledParent);
            SetRenderPos(Parent.X, Parent.Y, scaledParent);
        }

        // Initialize children.
        foreach (Widget child in children)
        {
            child.SetBounds();
        }

        // After children are set, try to size this one to fit them if set.
        if (childSizing != ChildSizing.None)
        {
            Queue<Widget> toCheck = new();
            foreach (Widget child in children) toCheck.Enqueue(child);

            int startX = X;
            int startY = Y;
            int maxX = X + Width;
            int maxY = Y + Height;

            while (toCheck.Count > 0)
            {
                Widget boundsToCheck = toCheck.Dequeue();

                if ((childSizing & ChildSizing.Width) != 0)
                {
                    startX = Math.Min(boundsToCheck.X, startX);
                    maxX = Math.Max(boundsToCheck.X + boundsToCheck.Width, maxX);
                }

                if ((childSizing & ChildSizing.Height) != 0)
                {
                    startY = Math.Min(boundsToCheck.Y, startY);
                    maxY = Math.Max(boundsToCheck.Y + boundsToCheck.Height, maxY);
                }

                if ((childSizing & ChildSizing.Once) == 0)
                {
                    foreach (Widget child in boundsToCheck.children)
                    {
                        toCheck.Enqueue(child);
                    }
                }
            }

            X = startX;
            Y = startY;
            Width = maxX - startX;
            Height = maxY - startY;

            // Once again, set the bounds of the children based on this new size.
            foreach (Widget child in children)
            {
                child.SetBounds();
            }
        }

        // Widget size differed from the start of this method.
        if (cWidth != Width || cHeight != Height) OnResize?.Invoke();
    }

    private Vector4 SetAlignments(int parentWidth, int parentHeight, Vector4 scaled)
    {
        bool alignOutsideH = (alignmentFlags & AlignFlags.OutsideH) != 0;
        bool alignOutsideV = (alignmentFlags & AlignFlags.OutsideV) != 0;

        switch (alignment)
        {
            case Align.None:
                break;
            case Align.LeftTop: // These are both effectively the same.
                if (alignOutsideH) scaled.X -= scaled.Z;
                if (alignOutsideV) scaled.Y -= scaled.W;
                break;
            case Align.LeftMiddle:
                if (alignOutsideH) scaled.X -= scaled.Z;
                scaled.Y += (parentHeight / 2) - (scaled.W / 2);
                break;
            case Align.LeftBottom:
                if (alignOutsideH) scaled.X -= scaled.Z;
                if (alignOutsideV) scaled.Y += scaled.W;
                scaled.Y += parentHeight - scaled.W;
                break;
            case Align.CenterTop:
                if (alignOutsideV) scaled.Y -= scaled.W;
                scaled.X += (parentWidth / 2) - (scaled.Z / 2);
                break;
            case Align.Center: // Both of these for absolute center.
                scaled.X += (parentWidth / 2) - (scaled.Z / 2);
                scaled.Y += (parentHeight / 2) - (scaled.W / 2);
                break;
            case Align.CenterBottom:
                if (alignOutsideV) scaled.Y += scaled.W;
                scaled.X += (parentWidth / 2) - (scaled.Z / 2);
                scaled.Y += parentHeight - scaled.W;
                break;
            case Align.RightTop:
                if (alignOutsideH) scaled.X += scaled.Z;
                if (alignOutsideV) scaled.Y -= scaled.W;
                scaled.X += parentWidth - scaled.Z;
                break;
            case Align.RightMiddle:
                if (alignOutsideH) scaled.X += scaled.Z;
                scaled.X += parentWidth - scaled.Z;
                scaled.Y += (parentHeight / 2) - (scaled.W / 2);
                break;
            case Align.RightBottom:
                if (alignOutsideH) scaled.X += scaled.Z;
                if (alignOutsideV) scaled.Y += scaled.W;
                scaled.X += parentWidth - scaled.Z;
                scaled.Y += parentHeight - scaled.W;
                break;
        }

        return scaled;
    }

    /// <summary>
    /// Set final render position, as an integer.
    /// </summary>
    public void SetRenderPos(int parentRenderX, int parentRenderY, Vector4 scaled)
    {
        X = (int)scaled.X + parentRenderX;
        Y = (int)scaled.Y + parentRenderY;
        Width = (int)scaled.Z;
        Height = (int)scaled.W;
    }

    /// <summary>
    /// Will try to move fixed positions, then set the bounds.
    /// </summary>
    public void Move(int newX, int newY)
    {
        if (positioningH == BoundsSizing.FixedSize) xPos = newX;
        if (positioningV == BoundsSizing.FixedSize) yPos = newY;

        SetBounds();
    }

    public Widget FixedX(int x)
    {
        positioningH = BoundsSizing.FixedSize;
        xPos = x;
        return this;
    }

    public Widget FixedY(int y)
    {
        positioningV = BoundsSizing.FixedSize;

        yPos = y;
        return this;
    }

    public Widget FixedPos(int x, int y)
    {
        positioningH = BoundsSizing.FixedSize;
        positioningV = BoundsSizing.FixedSize;

        xPos = x;
        yPos = y;
        return this;
    }

    public Widget FixedWidth(int width)
    {
        sizingH = BoundsSizing.FixedSize;

        xWidth = width;
        return this;
    }

    public Widget FixedHeight(int height)
    {
        sizingV = BoundsSizing.FixedSize;

        yHeight = height;
        return this;
    }

    public Widget FixedSize(int width, int height)
    {
        sizingH = BoundsSizing.FixedSize;
        sizingV = BoundsSizing.FixedSize;

        xWidth = width;
        yHeight = height;
        return this;
    }

    public Widget Fixed(int x, int y, int width, int height)
    {
        positioningH = BoundsSizing.FixedSize;
        positioningV = BoundsSizing.FixedSize;
        sizingH = BoundsSizing.FixedSize;
        sizingV = BoundsSizing.FixedSize;

        xPos = x;
        yPos = y;
        xWidth = width;
        yHeight = height;
        return this;
    }

    public Widget PercentX(float x)
    {
        positioningH = BoundsSizing.PercentSize;

        xPos = x;
        return this;
    }

    public Widget PercentY(float y)
    {
        positioningV = BoundsSizing.PercentSize;

        yPos = y;
        return this;
    }

    public Widget PercentPos(float x, float y)
    {
        positioningH = BoundsSizing.PercentSize;
        positioningV = BoundsSizing.PercentSize;

        xPos = x;
        yPos = y;
        return this;
    }

    public Widget PercentWidth(float width)
    {
        sizingH = BoundsSizing.PercentSize;

        xWidth = width;
        return this;
    }

    public Widget PercentHeight(float height)
    {
        sizingV = BoundsSizing.PercentSize;

        yHeight = height;
        return this;
    }

    public Widget PercentSize(float width, float height)
    {
        sizingH = BoundsSizing.PercentSize;
        sizingV = BoundsSizing.PercentSize;

        xWidth = width;
        yHeight = height;
        return this;
    }

    public Widget Percent(float x, float y, float width, float height)
    {
        positioningH = BoundsSizing.PercentSize;
        positioningV = BoundsSizing.PercentSize;
        sizingH = BoundsSizing.PercentSize;
        sizingV = BoundsSizing.PercentSize;

        xPos = x;
        yPos = y;
        xWidth = width;
        yHeight = height;
        return this;
    }

    public Widget Fill()
    {
        positioningH = BoundsSizing.PercentSize;
        positioningV = BoundsSizing.PercentSize;
        sizingH = BoundsSizing.PercentSize;
        sizingV = BoundsSizing.PercentSize;

        xPos = 0;
        yPos = 0;
        xWidth = 1;
        yHeight = 1;
        return this;
    }

    public Widget Alignment(Align boundsAlignment, AlignFlags flags = 0)
    {
        alignment = boundsAlignment;
        alignmentFlags = flags;
        return this;
    }

    public Widget SetChildSizing(ChildSizing sizing)
    {
        childSizing = sizing;
        return this;
    }

    // Full bounds checks.
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

    /// <summary>
    /// Is the bounds of this widget intersecting another?
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIntersecting(Widget bounds)
    {
        return X < bounds.X + bounds.Width && X + Width > bounds.X && Y < bounds.Y + bounds.Height && Y + Height > bounds.Y;
    }

    // Get the current xPos/yPos.
    public Vector2i GetFixedPos()
    {
        return new Vector2i((int)xPos, (int)yPos);
    }
}