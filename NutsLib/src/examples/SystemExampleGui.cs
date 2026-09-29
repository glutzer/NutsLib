using OpenTK.Mathematics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client;

namespace NutsLib;

/// <summary>A client-only, interactive catalogue of the GUI API.</summary>
//[GameSystem(forSide = EnumAppSide.Client)]
public class SystemExampleGui : GameSystem
{
    private ExampleGui? gui;
    public SystemExampleGui(bool isServer, ICoreAPI api) : base(isServer, api) { }

    public override void OnStart()
    {
        ScreenManager.hotkeyManager.RegisterHotKey("nutslibexamplegui", "NutsLib: GUI examples", (int)GlKeys.F7);
        MainAPI.Capi.Input.SetHotKeyHandler("nutslibexamplegui", _ =>
        {
            gui ??= new ExampleGui();
            gui.Toggle();
            return true;
        });
    }

    public override void OnClose()
    {
        if (gui?.IsOpened() == true) gui.TryClose();
        gui?.Dispose();
        gui = null;
    }
}

/// <summary>
/// Every concrete GUI widget, styled base controls, text rendering, layout,
/// clipping, transforms, events and all five transitions in one dialog.
/// Fixed offsets are pixels; fixed sizes are GUI units in this API.
/// </summary>
public class ExampleGui : Gui
{
    private int page;
    private bool checkedValue;
    private int sliderValue = 50;
    private string input = "Edit me";
    private string notes = "Multiline editing\nSelect, copy and paste text here.";
    private Vector3 color = new(0.2f, 0.7f, 0.9f);
    private WidgetTextBox? editor;
    private WidgetTextLine? status;
    private bool rebuild;
    private bool alternate;

    public override void RegisterEvents(GuiEvents events)
    {
        events.BeforeRender += _ =>
        {
            // Defer rebuilding until outside the input event dispatch.
            if (!rebuild) return;
            rebuild = false;
            SetWidgets();
        };
    }

    public override void PopulateWidgets()
    {
        if (editor != null) notes = string.Join("\n", editor.SaveContent());
        editor = null;
        int scale = Math.Max(1, MainAPI.GuiScale);
        int width = Math.Max(1, Math.Min(260, (int)(MainAPI.RenderWidth * 0.9f / scale)));
        int height = Math.Max(1, Math.Min(175, (int)(MainAPI.RenderHeight * 0.85f / scale)));
        Widget panel = new WidgetSliceBackground(null, this, VanillaThemes.OutsetTexture, Vector4.One)
            .FixedSize(width, height).Alignment(Align.Center);
        AddWidget(panel);
        VanillaThemes.AddTitleBar(panel, "NutsLib GUI examples - F7 / Esc to close");

        string[] pages = ["Controls", "Text", "Scrolling", "Motion"];
        for (int i = 0; i < pages.Length; i++)
        {
            int index = i;
            new WidgetVanillaButton(panel, this, () => { page = index; rebuild = true; }, pages[i])
                .SetColor(i == page ? new Vector4(0.7f, 0.9f, 1f, 1f) : Vector4.One)
                .Percent(0.025f + (i * 0.245f), 0.025f, 0.23f, 0.09f);
        }

        Widget body = new WidgetContainer(panel, this).Percent(0.04f, 0.15f, 0.92f, 0.75f);
        status = new WidgetTextLine(panel, this, VanillaThemes.Font, "Try the controls; drag the title to move the window.", VanillaThemes.WhitishTextColor);
        status.Percent(0.03f, 0.92f, 0.94f, 0.055f);
        switch (page)
        {
            case 0: Controls(body); break;
            case 1: Text(body); break;
            case 2: Scrolling(body); break;
            case 3: Motion(body); break;
        }
        panel.CalculateBounds();
    }

    private void Label(Widget parent, string text, float x, float y, float w, float h)
        => new WidgetTextLine(parent, this, VanillaThemes.Font, text, VanillaThemes.WhitishTextColor, false)
            .Percent(x, y, w, h);

    private void Controls(Widget body)
    {
        Label(body, "Buttons, checkbox and slider", 0, 0, 0.55f, 0.07f);
        new WidgetVanillaButton(body, this, () => status?.SetText("Button clicked!"), "Click me")
            .Percent(0, 0.11f, 0.26f, 0.12f);
        new WidgetVanillaCheckbox(body, this, value => { checkedValue = value; status?.SetText($"Checkbox: {value}"); }, checkedValue)
            .Percent(0.3f, 0.11f, 0.07f, 0.12f);
        Label(body, "Toggle", 0.39f, 0.11f, 0.2f, 0.12f);
        new WidgetVanillaSlider(body, this, value => { sliderValue = value; status?.SetText($"Slider: {value}"); }, 100)
            .SetCurrentStep(sliderValue).Percent(0, 0.33f, 0.55f, 0.09f);
        new WidgetVanillaSlider(body, this, value => status?.SetText($"Released at: {value}"), 10, true)
            .Percent(0, 0.53f, 0.55f, 0.09f);
        Label(body, "Second slider reports on release", 0, 0.65f, 0.55f, 0.07f);
        Label(body, "Color + hue", 0.66f, 0, 0.32f, 0.07f);
        new WidgetColorPicker(body, this, value => { color = value; status?.SetText($"RGB: {value.X:F2}, {value.Y:F2}, {value.Z:F2}"); }, color)
            .Percent(0.66f, 0.12f, 0.32f, 0.4f);
        Label(body, "Active hand slot (live inventory)", 0, 0.79f, 0.65f, 0.07f);
        new WidgetVanillaItemSlotGrid([MainAPI.Capi.World.Player.Entity.ActiveHandItemSlot], 1, 1, 18, body, this)
            .PercentPos(0.75f, 0.76f).FixedSize(18, 18);
    }

    private void Text(Widget body)
    {
        Label(body, "Single-line inputs: Tab / Shift+Tab changes focus", 0, 0, 1, 0.07f);
        new WidgetVanillaTextInputBox(body, this, centerValues: false, onNewText: value => input = value, defaultText: input)
            .Percent(0, 0.1f, 0.47f, 0.1f);
        new WidgetTextBoxSingle(body, this, VanillaThemes.Font, VanillaThemes.YellowColor, emptyText: "Plain input (no background)")
            .Percent(0.52f, 0.1f, 0.48f, 0.1f);
        Label(body, "Multiline editor", 0, 0.25f, 1, 0.07f);
        editor = new WidgetTextBox(body, this, VanillaThemes.Font, Math.Max(1, Scaled(4)), Vector4.One, maxLines: 8);
        editor.Percent(0, 0.34f, 1, 0.35f);
        editor.LoadContent(notes);
        new ExampleRichText(body, this).Percent(0, 0.74f, 1, 0.25f);
    }

    private void Scrolling(Widget body)
    {
        Label(body, "Clipped vertical stack: wheel or drag either scrollbar", 0, 0, 1, 0.08f);
        Widget viewport = new WidgetContainer(body, this).Percent(0, 0.13f, 0.93f, 0.75f);
        // Matching clip markers surround content in render order, reversed for input.
        new WidgetClip(true, viewport, this).Fill();
        Widget stack = new WidgetVerticalStack(viewport, this, Scaled(2))
            .PercentWidth(1.5f).SetChildSizing(ChildSizing.Height | ChildSizing.Once);
        for (int i = 0; i < 18; i++)
        {
            int row = i + 1;
            new WidgetVanillaButton(stack, this, () => status?.SetText($"Clicked row {row}"), $"Scrollable row {row} - horizontal overflow")
                .PercentWidth(1).FixedHeight(10);
        }
        new WidgetClip(false, viewport, this).Fill();
        new WidgetVanillaScrollBar(body, this, stack).Percent(0.95f, 0.13f, 0.05f, 0.75f);
        new WidgetVanillaScrollBar(body, this, stack, stepsPerPage: 0, horizontal: true).Percent(0, 0.91f, 0.93f, 0.06f);
    }

    private void Motion(Widget body)
    {
        Label(body, "Fixed / percent layout, fade, size and position transitions", 0, 0, 1, 0.08f);
        Widget fixedDemo = new WidgetVanillaButton(body, this, () => status?.SetText("Fixed widget clicked"), "Fixed")
            .Fixed(0, Scaled(18), 35, 12);
        Widget percentDemo = new WidgetVanillaButton(body, this, () => status?.SetText("Percent widget clicked"), "Percent")
            .Percent(0.5f, 0.18f, 0.25f, 0.12f);
        new WidgetVanillaButton(body, this, () =>
        {
            alternate = !alternate;
            fixedDemo.TransitionToFixedPos(alternate ? Scaled(30) : 0, Scaled(18), 0.6f);
            fixedDemo.TransitionToFixedSize(alternate ? 48 : 35, alternate ? 18 : 12, 0.6f);
            percentDemo.TransitionToPercentPos(alternate ? 0.7f : 0.5f, 0.18f, 0.6f);
            percentDemo.TransitionToPercentSize(alternate ? 0.3f : 0.25f, alternate ? 0.2f : 0.12f, 0.6f);
            fixedDemo.FadeTo(alternate ? 0.4f : 1f, 0.6f, () => status?.SetText("Transition completed"));
        }, "Animate all five transitions").Percent(0, 0.45f, 0.65f, 0.12f);
        Widget transformed = new WidgetContainer(body, this).Percent(0, 0.65f, 0.9f, 0.15f);
        System.Func<float, Matrix4> transform = _ => Matrix4.CreateTranslation(Scaled(5), 0, 0);
        new WidgetTransform(transformed, this, transform, true);
        new WidgetVanillaButton(transformed, this, () => status?.SetText("Transformed hit testing works"), "Transformed button").Fill();
        new WidgetTransform(transformed, this, transform, false);
        Label(body, "Unscaled label (pixel-sized)", 0, 0.88f, 0.65f, 0.08f);
        new WidgetTextLine(body, this, VanillaThemes.Font, "24 px", VanillaThemes.YellowColor)
            .NoScaling().FixedSize(85, 24).Alignment(Align.RightBottom);
    }

    private sealed class ExampleRichText : Widget
    {
        private readonly TextObjectGroup rich = new();
        private readonly TextObjectIndecipherable cipher;

        public ExampleRichText(Widget parent, Gui gui) : base(parent, gui)
        {
            rich.textObjects = Markdown.ConvertMarkdownLine("Normal <strong>bold</strong> <i>italic</i> <font color=#66ccff>color</font> <indecipherable>secret</indecipherable>", 16);
            cipher = new TextObjectIndecipherable("Randomized text", FontRegistry.GetFont("friz"), 16, VanillaThemes.YellowColor,
                VanillaThemes.Font, CipherType.AllRandomized);
        }

        public override void OnRender(float dt, ShaderGui shader)
        {
            float size = Math.Max(1, Height * 0.3f);
            foreach (TextObject text in rich.textObjects) { text.SetScale(size); text.Shadow = true; }
            cipher.SetScale(size);
            rich.RenderLine(X, Y + (Height * 0.25f), shader, centerVertically: true);
            cipher.RenderLine(X, Y + (Height * 0.75f), shader, centerVertically: true);
        }
    }
}
