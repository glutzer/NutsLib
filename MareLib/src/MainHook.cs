using HarmonyLib;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.Server;

namespace MareLib;

public class MainHook : ModSystem, IRenderer
{
    public static ICoreClientAPI Capi { get; private set; } = null!;
    public static ICoreServerAPI Sapi { get; private set; } = null!;

    public static ClientMain Client { get; private set; } = null!;
    public static ServerMain Server { get; private set; } = null!;

    private static Harmony? Harmony { get; set; }

    public static int RenderWidth { get; private set; } = 256;
    public static int RenderHeight { get; private set; } = 256;
    public static int GuiScale { get; private set; } = 1;

    public static event Action<int, int>? OnWindowResize;
    public static event Action<int>? OnGuiRescale;

    public static MeshHandle GuiQuad { get; private set; } = null!;

    public MainHook()
    {

    }

    public override double ExecuteOrder()
    {
        return 0.2;
    }

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return true;
    }

    public override bool ShouldLoad(ICoreAPI api)
    {
        return true;
    }

    /// <summary>
    /// Called as soon as dll is loaded.
    /// </summary>
    public override void StartPre(ICoreAPI api)
    {
        if (api is ICoreClientAPI capi)
        {
            Capi = capi;
            Client = (ClientMain)capi.World;
        }
        else if (api is ICoreServerAPI sapi)
        {
            Sapi = sapi;
            Server = (ServerMain)sapi.World;
        }

        Patch();
    }

    // No blocks/assets. Register packets and events here.

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Event.RegisterRenderer(this, EnumRenderStage.Before);

        GuiQuad = QuadMeshUtility.CreateGuiQuadMesh(vertex =>
        {
            return new GuiVertex(vertex.position, vertex.uv);
        });

        api.Event.ReloadShader += () =>
        {
            MareShaderRegistry.RegisterShader("gui", "gui", "gui");
            return true;
        };

        api.Event.OnSendChatMessage += Event_OnSendChatMessage;
    }

    private void Event_OnSendChatMessage(int groupId, ref string message, ref EnumHandling handled)
    {
        TestGui test = new(Capi);
        test.TryOpen();
    }

    public override void StartServerSide(ICoreServerAPI api)
    {

    }

    /// <summary>
    /// All blocks and assets available here.
    /// </summary>
    public override void AssetsLoaded(ICoreAPI api)
    {
        if (api is ICoreClientAPI)
        {
            FontRegistry.LoadFonts();
        }
    }

    public override void Dispose()
    {
        Capi = null!;
        Sapi = null!;
        Client = null!;
        Server = null!;

        OnWindowResize = null;
        OnGuiRescale = null;

        Cache.Dispose();
        FontRegistry.Dispose();

        GuiQuad?.Dispose();
        GuiQuad = null!;

        Unpatch();
    }

    public static void Patch()
    {
        if (Harmony != null) return;

        Harmony = new Harmony("marelib");
        Harmony.PatchCategory("core");
    }

    public static void Unpatch()
    {
        if (Harmony == null) return;

        Harmony.UnpatchAll("marelib");
        Harmony = null;
    }

    public double RenderOrder => 0;
    public int RenderRange => 0;
    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        int previousRenderWidth = RenderWidth;
        int previousRenderHeight = RenderHeight;
        float previousGuiScale = GuiScale;

        RenderWidth = Capi.Render.FrameWidth;
        RenderHeight = Capi.Render.FrameHeight;
        GuiScale = (int)(RuntimeEnv.GUIScale * 4f); // 1x - 4x instead of 0.25x - 1x.

        if (previousRenderWidth != RenderWidth || previousRenderHeight != RenderHeight)
        {
            OnWindowResize?.Invoke(RenderWidth, RenderHeight);
        }

        if (previousGuiScale != GuiScale)
        {
            OnGuiRescale?.Invoke(GuiScale);
        }
    }
}