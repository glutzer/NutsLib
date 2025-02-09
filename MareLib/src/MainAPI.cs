using HarmonyLib;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.Server;

namespace MareLib;

/// <summary>
/// Handles systems, ticking, and some events like window/gui scale.
/// </summary>
public class MainAPI : ModSystem, IRenderer
{
    #region Game Systems

    private readonly List<GameSystem> gameSystems = new();
    private readonly Dictionary<string, GameSystem> gameSystemsDictionary = new();

    public T GetGameSystem<T>() where T : GameSystem
    {
        return (T)gameSystemsDictionary[InnerClass<T>.Name];
    }

    public bool TryGetGameSystem<T>([NotNullWhen(true)] out T? gameSystem) where T : GameSystem
    {
        if (gameSystemsDictionary.TryGetValue(InnerClass<T>.Name, out GameSystem? system))
        {
            gameSystem = (T)system;
            return true;
        }

        gameSystem = null;
        return false;
    }

    public static T GetGameSystem<T>(EnumAppSide side) where T : GameSystem
    {
        // Problem - the game disposes game systems, THEN unloads blocks on the client. Maybe because blocks are a game system.
        return side == EnumAppSide.Client ? ClientHook.GetGameSystem<T>() : ServerHook.GetGameSystem<T>();
    }

    public static bool TryGetGameSystem<T>(EnumAppSide side, [NotNullWhen(true)] out T? gameSystem) where T : GameSystem
    {
        if (side == EnumAppSide.Client)
        {
            if (ClientHook == null)
            {
                gameSystem = null;
                return false;
            }
            return ClientHook.TryGetGameSystem<T>(out gameSystem!);
        }
        else
        {
            if (ServerHook == null)
            {
                gameSystem = null;
                return false;
            }
            return ServerHook.TryGetGameSystem<T>(out gameSystem!);
        }
    }

    protected void LoadGameSystems(ICoreAPI api)
    {
        (Type, GameSystemAttribute)[] types = AttributeUtilities.GetAllAnnotatedClasses<GameSystemAttribute>();

        List<(Type type, double loadOrder)> list = new();

        foreach ((Type type, GameSystemAttribute attribute) tuple in types)
        {
            if (isServer && tuple.attribute.forSide == EnumAppSide.Client) continue;
            if (!isServer && tuple.attribute.forSide == EnumAppSide.Server) continue;

            list.Add((tuple.type, tuple.attribute.loadOrder));
        }

        void RegisterGameSystem(Type type)
        {
            try
            {
                GameSystem? system = Activator.CreateInstance(type, isServer, api) as GameSystem ?? throw new Exception($"{type} must inherit from GameSystem!");
                gameSystems.Add(system);
                gameSystemsDictionary.Add(type.Name, system);
            }
            catch
            {
                Console.WriteLine($"Error loading {type}");
            }
        }

        // Order systems by stage, by load order, and then alphabetically.
        list = list.OrderBy(t => t.loadOrder).ThenBy(t => t.type.Name).ToList();
        list.ForEach(item => RegisterGameSystem(item.type));

        // Initialize all systems.
        foreach (GameSystem system in gameSystems) system.PreInitialize();
        foreach (GameSystem system in gameSystems) system.Initialize();
        foreach (GameSystem system in gameSystems) system.PostInitialize();
    }

    #endregion

    private bool isServer;

    public static ICoreClientAPI Capi { get; private set; } = null!;
    public static ICoreServerAPI Sapi { get; private set; } = null!;

    public static MainAPI ClientHook { get; private set; } = null!;
    public static MainAPI ServerHook { get; private set; } = null!;

    public static ClientMain Client { get; private set; } = null!;
    public static ServerMain Server { get; private set; } = null!;

    private static Harmony? Harmony { get; set; }

    public static int RenderWidth { get; private set; } = 512;
    public static int RenderHeight { get; private set; } = 512;
    public static int GuiScale { get; private set; } = 1;

    public static Vector3d OriginOffset { get; private set; } // Effectively the same thing for now.
    public static Vector3d CameraPosition { get; private set; } //
    public static Vector3 CameraNormal { get; private set; }

    public static Matrix4 OriginViewMatrix { get; private set; }
    public static Matrix4 PerspectiveMatrix { get; private set; }
    public static Matrix4 OrthographicMatrix { get; private set; }

    // Only initialized on client.
    private UboHandle<RenderGlobals> renderGlobalsUbo = null!;

    /// <summary>
    /// Used as a global ubo.
    /// </summary>
    public struct RenderGlobals
    {
        public Matrix4 originViewMatrix;
        public Matrix4 perspectiveMatrix;
        public Matrix4 orthographicMatrix;

        public RenderGlobals(Matrix4 originViewMatrix, Matrix4 perspectiveMatrix, Matrix4 orthographicMatrix)
        {
            this.originViewMatrix = originViewMatrix;
            this.perspectiveMatrix = perspectiveMatrix;
            this.orthographicMatrix = orthographicMatrix;
        }
    }

    public static event Action<int, int>? OnWindowResize;
    public static event Action<int>? OnGuiRescale;

    public static MeshHandle GuiQuad { get; private set; } = null!;

    public MainAPI()
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
        // Register new asset paths.
        AssetCategory.categories["fonts"] = new AssetCategory("fonts", false, EnumAppSide.Client);
        AssetCategory.categories["objs"] = new AssetCategory("objs", false, EnumAppSide.Client);

        if (api is ICoreClientAPI capi)
        {
            Capi = capi;
            Client = (ClientMain)capi.World;
            ClientHook = this;

            renderGlobalsUbo = new UboHandle<RenderGlobals>(BufferUsageHint.DynamicDraw);
            UboRegistry.SetUbo("renderGlobals", renderGlobalsUbo.handle);

            RenderTools.OnStart();
        }
        else if (api is ICoreServerAPI sapi)
        {
            Sapi = sapi;
            Server = (ServerMain)sapi.World;
            ServerHook = this;
            isServer = true;
        }

        LoadGameSystems(api);

        Patch();
    }

    // No blocks/assets. Register packets and events here.

    public override void Start(ICoreAPI api)
    {
        foreach ((Type, ItemAttribute) item in AttributeUtilities.GetAllAnnotatedClasses<ItemAttribute>())
        {
            api.RegisterItemClass(item.Item1.Name, item.Item1);
        }

        foreach ((Type, BlockAttribute) block in AttributeUtilities.GetAllAnnotatedClasses<BlockAttribute>())
        {
            api.RegisterBlockClass(block.Item1.Name, block.Item1);
        }

        foreach ((Type, BlockEntityAttribute) blockEntity in AttributeUtilities.GetAllAnnotatedClasses<BlockEntityAttribute>())
        {
            api.RegisterBlockEntityClass(blockEntity.Item1.Name, blockEntity.Item1);
        }

        foreach ((Type, EntityBehaviorAttribute) behavior in AttributeUtilities.GetAllAnnotatedClasses<EntityBehaviorAttribute>())
        {
            api.RegisterEntityBehaviorClass(behavior.Item1.Name, behavior.Item1);
        }
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Event.RegisterRenderer(this, EnumRenderStage.Before);

        GuiQuad = QuadMeshUtility.CreateGuiQuadMesh(vertex =>
        {
            return new GuiVertex(vertex.position, vertex.uv);
        });

        MareShaderRegistry.AddShader("marelib:gui", "marelib:gui", "gui");
        MareShaderRegistry.AddShader("marelib:gui", "marelib:colorwheelgui", "colorwheelgui");

        foreach (GameSystem sys in gameSystems)
        {
            sys.OnStart();
        }
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        foreach (GameSystem sys in gameSystems)
        {
            sys.OnStart();
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        if (api is ICoreClientAPI)
        {
            FontRegistry.LoadFonts();
        }

        foreach (GameSystem system in gameSystems) system.OnAssetsLoaded();
    }

    public override void Dispose()
    {
        if (isServer && Sapi != null)
        {
            foreach (GameSystem system in gameSystems) system.OnClose();
            Server = null!;
            Sapi = null!;
            ServerHook = null!;
        }
        else if (Capi != null)
        {
            foreach (GameSystem system in gameSystems) system.OnClose();
            renderGlobalsUbo.Dispose();

            OnWindowResize = null;
            OnGuiRescale = null;
            FontRegistry.Dispose();
            GuiQuad?.Dispose();
            GuiQuad = null!;
            UboRegistry.Dispose();
            RenderTools.OnStop();
            MareShaderRegistry.Dispose();
            ClientCache.Dispose();

            Client = null!;
            Capi = null!;
            ClientHook = null!;
        }

        // Might need to be careful on client/server...
        Unpatch();

        // Clear attributes.
        AttributeUtilities.ReloadAttributes();
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

    public double RenderOrder => 1000;
    public int RenderRange => 0;

    // Pretty much the only rendering that should be done from this library.

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        int previousRenderWidth = RenderWidth;
        int previousRenderHeight = RenderHeight;
        float previousGuiScale = GuiScale;

        RenderWidth = Capi.Render.FrameWidth;
        RenderHeight = Capi.Render.FrameHeight;
        GuiScale = (int)(RuntimeEnv.GUIScale * 4f); // 1x - 4x instead of 0.25x - 1x.

        float zNear = Client.MainCamera.GetField<float>("ZNear");
        float zFar = Client.MainCamera.GetField<float>("ZFar");

        // According to the camera, this is the near/far. I'm not sure where it's set though.
        PerspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(ClientSettings.FieldOfView), (float)RenderWidth / RenderHeight, zNear, zFar);
        OrthographicMatrix = Matrix4.CreateOrthographicOffCenter(0, RenderWidth, RenderHeight, 0, -1000, 1000);

        // In the game, the camera matrix is calculated incorrectly, so here it is too.
        Vector3 front;
        front.Z = -MathF.Cos((float)Client.MainCamera.Pitch) * MathF.Cos((float)Client.MainCamera.Yaw);
        front.Y = MathF.Sin((float)Client.MainCamera.Pitch);
        front.X = -MathF.Cos((float)Client.MainCamera.Pitch) * MathF.Sin((float)Client.MainCamera.Yaw);
        front = Vector3.Normalize(front);
        Vector3 up = Vector3.UnitY;
        OriginViewMatrix = Matrix4.LookAt(Vector3.Zero, front, up);

        // Update global ubo.
        renderGlobalsUbo.UpdateData(new RenderGlobals(OriginViewMatrix, PerspectiveMatrix, OrthographicMatrix));

        Vec3d cameraPos = Client.MainCamera.CameraEyePos;
        OriginOffset = new Vector3d(cameraPos.X, cameraPos.Y, cameraPos.Z);
        CameraPosition = new Vector3d(cameraPos.X, cameraPos.Y, cameraPos.Z);

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