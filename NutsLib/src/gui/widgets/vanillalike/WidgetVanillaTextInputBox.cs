namespace NutsLib;

public class WidgetVanillaTextInputBox : WidgetTextBoxSingle
{
    public WidgetVanillaTextInputBox(Widget? parent, Gui gui, bool limitTextToBox = true, bool centerValues = true, Action<string>? onNewText = null, string? defaultText = null, string? emptyText = null) : base(parent, gui, VanillaThemes.Font, VanillaThemes.WhitishTextColor, limitTextToBox, centerValues, onNewText, defaultText, emptyText)
    {
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        RenderTools.RenderNineSlice(VanillaThemes.InsetTexture, shader, X, Y, Width, Height);

        base.OnRender(dt, shader);
    }

    protected override void OnCharacterChanged(bool removed)
    {
        MainAPI.Capi.Gui.PlaySound("tick");
    }
}