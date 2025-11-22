//using OpenTK.Mathematics;

//namespace NutsLib;

//public class GuiTest : Gui
//{
//    public override void PopulateWidgets()
//    {
//        VanillaThemes.ClearCache();

//        Widget bg;
//        AddWidget(bg = new WidgetSliceBackground(null, this, VanillaThemes.OutsetTexture, Vector4.One).Percent(0, 0, 0.5f, 0.5f).Alignment(Align.Center));

//        new WidgetVanillaButton(bg, this, () => Console.WriteLine("Button 1 clicked!"), "Button 1").Fixed(20, 20, 50, 20).Alignment(Align.Center);

//        bg.SetFade = 1f;
//        bg.FadeTo(1f, 0.5f);

//        Vintagestory.API.Common.ItemSlot slots = MainAPI.Capi.World.Player.Entity.ActiveHandItemSlot;

//        new WidgetVanillaItemSlotGrid([slots], 1, 1, 16, bg, this).Alignment(Align.LeftTop).Fixed(0, 0, 16, 16);
//    }
//}