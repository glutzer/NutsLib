using MareLib;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Equimancy;

public class PageData
{
    public List<string> lines = new();
}

public class PipeDebugGui : Gui
{
    private WidgetTextBox? leftPageBack;
    private WidgetTextBox? rightPageBack;

    private WidgetTextBox? leftPage;
    private WidgetTextBox? rightPage;

    private readonly List<PageData> pages = new();
    private int leftPageIndex; // Index of the left page. The right page is 1 higher.

    public PipeDebugGui() : base()
    {
        for (int i = 0; i < 20; i++)
        {
            PageData page = new();
            pages.Add(page);
        }
    }

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();
        capi.Gui.PlaySound(new AssetLocation("equimancy:sounds/flippage"));
    }

    public void AddPage(Bounds pageBounds, System.Func<float, Matrix4>? transformDelegate, bool forwardTab, out WidgetTextBox page)
    {
        if (transformDelegate != null) AddWidget(new TransformWidget(this, pageBounds, transformDelegate, true));

        AddWidget(new BackgroundWidget(this, pageBounds, "equimancy:textures/singlepage.png"));
        Bounds textBoxBounds = Bounds.CreateFrom(pageBounds).Percent(0f, 0f, 1f, 1f);
        AddWidget(page = new TextBoxWidget(this, textBoxBounds, FontRegistry.GetFont("friz"), 20, new Vector4(0f, 0f, 0f, 0.45f)));

        if (forwardTab)
        {
            Bounds tabBounds = Bounds.CreateFrom(pageBounds).Fixed(0, 0, 32, 32).Alignment(Align.LeftBottom);
            AddWidget(new CornerButton(this, tabBounds, () => FlipPage(false), $"equimancy:textures/pagetabl.png"));
        }
        else
        {
            Bounds tabBounds = Bounds.CreateFrom(pageBounds).Fixed(0, 0, 32, 32).Alignment(Align.RightBottom);
            AddWidget(new CornerButton(this, tabBounds, () => FlipPage(true), $"equimancy:textures/pagetabr.png"));
        }

        AddWidget(new ItemGridImpl(new ItemSlot[] { MainAPI.Capi.World.Player.InventoryManager.ActiveHotbarSlot, MainAPI.Capi.World.Player.InventoryManager.ActiveHotbarSlot, MainAPI.Capi.World.Player.InventoryManager.ActiveHotbarSlot, MainAPI.Capi.World.Player.InventoryManager.ActiveHotbarSlot }, 2, 2, 64, this,
            Bounds.CreateFrom(pageBounds).Percent(0, -0.1f, 1, 0.5f).Alignment(Align.CenterBottom)));

        if (transformDelegate != null) AddWidget(new TransformWidget(this, pageBounds, transformDelegate, false));
    }

    public override void PopulateWidgets(out Bounds mainBounds)
    {
        mainBounds = Bounds.Create().Alignment(Align.Center).Fixed(0, 0, 800, 600);
        mainBounds.NoScaling();

        // Main bounds is the entire area including the cover, these are the page bounds.
        Bounds leftPageBounds = Bounds.CreateFrom(mainBounds).Percent(0.025f, 0.025f, 0.475f, 0.95f);
        Bounds rightPageBounds = Bounds.CreateFrom(mainBounds).Percent(0.5f, 0.025f, 0.475f, 0.95f);

        //AddWidget(new BackgroundWidgetSkia(this, mainBounds, () =>
        //{
        //    return TextureBuilder.Begin(800, 600)
        //    .SetColor(new SkiaSharp.SKColor(40, 15, 0))
        //    .FillMode()
        //    .DrawRectangle(0, 0, 800, 600)
        //    .StrokeMode(16)
        //    .SetColor(new SkiaSharp.SKColor(160, 15, 0))
        //    .DrawEmbossedRectangle(0, 0, 800, 600, true)
        //    .End();
        //}));

        // Backgrounds pages.
        AddPage(leftPageBounds, dt => LeftPageTransform(dt, true), true, out leftPageBack);
        AddPage(leftPageBounds, dt => LeftPageTransform(dt, false), true, out leftPage);

        AddPage(rightPageBounds, dt => RightPageTransform(dt, true), false, out rightPageBack);
        AddPage(rightPageBounds, dt => RightPageTransform(dt, false), false, out rightPage);
    }

    private float transformProgress = 1;
    private int transformDirection = 1;
    private bool contentSwapped = true; // True if it doesn't need to change pages in the direction.

    public void FlipPage(bool forward)
    {
        // At ends.
        if (leftPageIndex == 0 && !forward) return;
        if (leftPageIndex == pages.Count - 2 && forward) return;
        if (transformProgress is not 0 and not 1) return; // In progress.

        transformDirection = forward ? 1 : -1;
        transformProgress = forward ? 0 : 1;
        contentSwapped = false;

        capi.Gui.PlaySound(new AssetLocation("equimancy:sounds/flippage"));
    }

    public void SwapContent()
    {
        contentSwapped = true;

        // Gui for some reason not initialized.
        if (leftPage == null || rightPage == null || leftPageBack == null || rightPageBack == null) return;

        // Save content on current pages.
        pages[leftPageIndex].lines = leftPage.SaveContent();
        pages[leftPageIndex + 1].lines = rightPage.SaveContent();

        leftPageIndex += transformDirection * 2;

        leftPage.LoadContent(pages[leftPageIndex].lines);
        rightPage.LoadContent(pages[leftPageIndex + 1].lines);

        int bottomLeftIndex = Math.Clamp(leftPageIndex - 2, 0, pages.Count - 2);
        int bottomRightIndex = Math.Clamp(leftPageIndex + 3, 0, pages.Count - 1);

        leftPageBack.LoadContent(pages[bottomLeftIndex].lines);
        rightPageBack.LoadContent(pages[bottomRightIndex].lines);
    }

    /// <summary>
    /// Only increases when rendering back page to avoid page artifacts.
    /// </summary>
    public Matrix4 RightPageTransform(float dt, bool backPage)
    {
        if (backPage)
        {
            if (dt > 0)
            {
                transformProgress += dt * transformDirection;
                transformProgress = Math.Clamp(transformProgress, 0, 1);

                if (!contentSwapped)
                {
                    if (transformDirection == -1 && transformProgress < 0.5f)
                    {
                        SwapContent();
                    }
                    else if (transformDirection == 1 && transformProgress > 0.5f)
                    {
                        SwapContent();
                    }
                }
            }

            return Matrix4.Identity;
        }

        if (transformProgress < 0.5)
        {
            float halfScreenWidth = MainAPI.RenderWidth / 2f;
            float angle = MathHelper.DegreesToRadians(GameMath.Lerp(0, 180, transformProgress));
            return Matrix4.CreateTranslation(-halfScreenWidth, 0, 0) * Matrix4.CreateRotationY(angle) * Matrix4.CreateTranslation(halfScreenWidth, 0, 0);
        }
        else
        {
            return Matrix4.CreateRotationY(0);
        }
    }

    public Matrix4 LeftPageTransform(float dt, bool backPage)
    {
        if (backPage) return Matrix4.Identity;

        if (transformProgress > 0.5)
        {
            float halfScreenWidth = MainAPI.RenderWidth / 2f;
            float angle = GameMath.Lerp(90, -90, transformProgress - 0.5f);
            return Matrix4.CreateTranslation(-halfScreenWidth, 0, 0) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angle)) * Matrix4.CreateTranslation(halfScreenWidth, 0, 0);
        }
        else
        {
            return Matrix4.CreateRotationY(0);
        }
    }
}