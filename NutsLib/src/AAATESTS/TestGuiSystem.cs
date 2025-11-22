//using Vintagestory.API.Client;
//using Vintagestory.API.Common;
//using Vintagestory.Client;

//namespace NutsLib;

//[GameSystem(forSide = EnumAppSide.Client)]
//public class TestGuiSystem : GameSystem
//{
//    private GuiTest? gui;

//    public TestGuiSystem(bool isServer, ICoreAPI api) : base(isServer, api)
//    {
//    }

//    public override void OnStart()
//    {
//        ScreenManager.hotkeyManager.RegisterHotKey("togglenuttygui", "Toggle Nutty Gui", (int)GlKeys.V, triggerOnUpAlso: true);
//        MainAPI.Capi.Input.SetHotKeyHandler("togglenuttygui", key =>
//        {
//            if (key.OnKeyUp)
//            {

//            }
//            else
//            {
//                if (gui != null)
//                {
//                    gui?.TryClose();
//                    gui = null;
//                    return true;
//                }

//                gui = new GuiTest();
//                gui.TryOpen();
//            }

//            return true;
//        });
//    }
//}