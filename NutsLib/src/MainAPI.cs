global using System;
using HarmonyLib;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;
using Vintagestory.Server;

namespace NutsLib;

/// <summary>
/// Handles systems, ticking, and some events like window/gui scale.
/// </summary>
public class MainAPI : ModSystem, IRenderer
{
    #region Game Systems

    private readonly List<GameSystem> gameSystems = [];
    private readonly Dictionary<string, GameSystem> gameSystemsDictionary = [];

    private static readonly Queue<Action> beforeFrameTasks = new();

    public static void EnqueueBeforeFrameTask(Action action)
    {
        beforeFrameTasks.Enqueue(action);
    }

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

    public static T GetGameSystem<T>(bool isServer) where T : GameSystem
    {
        // Problem - the game disposes game systems, THEN unloads blocks on the client. Maybe because blocks are a game system.
        return !isServer ? ClientHook.GetGameSystem<T>() : ServerHook.GetGameSystem<T>();
    }

    public static T GetClientSystem<T>() where T : GameSystem
    {
        return ClientHook.GetGameSystem<T>();
    }

    public static T GetServerSystem<T>() where T : GameSystem
    {
        return ServerHook.GetGameSystem<T>();
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

        List<(Type type, double loadOrder)> list = [];

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
        foreach (GameSystem system in gameSystems)
        {
            Console.WriteLine($"Starting pre-init {system.GetType().Name}");
            system.PreInitialize();
        }
        foreach (GameSystem system in gameSystems)
        {
            Console.WriteLine($"Starting init {system.GetType().Name}");
            system.Initialize();
        }
        foreach (GameSystem system in gameSystems)
        {
            Console.WriteLine($"Starting post-init {system.GetType().Name}");
            system.PostInitialize();
        }
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

    public static int FrameNumber { get; private set; }

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
        TreeAttribute.RegisterAttribute(434343, typeof(CustomDataAttribute));

        NativesLoader.Load(this, api.Side == EnumAppSide.Server);

        // Register new asset paths.
        AssetCategory.categories["objs"] = new AssetCategory("objs", false, EnumAppSide.Client);

        if (api is ICoreClientAPI capi)
        {
            Capi = capi;
            Client = (ClientMain)capi.World;
            ClientHook = this;

            RenderTools.OnStart();

            DynamicFontAtlas.Initialize();
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

        foreach ((Type, EntityAttribute) entity in AttributeUtilities.GetAllAnnotatedClasses<EntityAttribute>())
        {
            api.RegisterEntity(entity.Item1.Name, entity.Item1);
        }

        foreach ((Type, BlockBehaviorAttribute) blockBeh in AttributeUtilities.GetAllAnnotatedClasses<BlockBehaviorAttribute>())
        {
            api.RegisterBlockBehaviorClass(blockBeh.Item1.Name, blockBeh.Item1);
        }

        foreach ((Type, BlockEntityBehaviorAttribute) blockEntityBeh in AttributeUtilities.GetAllAnnotatedClasses<BlockEntityBehaviorAttribute>())
        {
            api.RegisterBlockEntityBehaviorClass(blockEntityBeh.Item1.Name, blockEntityBeh.Item1);
        }

        foreach ((Type, CollectibleBehaviorAttribute) collBeh in AttributeUtilities.GetAllAnnotatedClasses<CollectibleBehaviorAttribute>())
        {
            api.RegisterCollectibleBehaviorClass(collBeh.Item1.Name, collBeh.Item1);
        }
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        // Set initial fov.
        Client.CallMethod("OnFowChanged", 1);

        api.Event.RegisterRenderer(this, EnumRenderStage.Before);

        GuiQuad = QuadMeshUtility.CreateGuiQuadMesh(vertex =>
        {
            return new GuiVertex(vertex.position, vertex.uv);
        });

        NuttyShaderRegistry.AddShader<ShaderGui>("nutslib:gui", "nutslib:gui", "gui");
        NuttyShaderRegistry.AddShader<ShaderGui>("nutslib:gui", "nutslib:colorwheelgui", "colorwheelgui");

        foreach (GameSystem sys in gameSystems)
        {
            try
            {
                sys.OnStart();
            }
            catch (Exception e)
            {
                api.Logger.Error(e);
            }
        }
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        if (isServer)
        {
            foreach (GameSystem sys in gameSystems)
            {
                try
                {
                    sys.OnStart();
                }
                catch (Exception e)
                {
                    api.Logger.Error(e);
                }
            }
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        if (api is ICoreClientAPI)
        {
            DynamicFontAtlas.AssetsLoaded();
        }

        foreach (GameSystem system in gameSystems)
        {
            Console.WriteLine($"Starting assets loaded {system.GetType().Name}");
            system.OnAssetsLoaded();
        }
    }

    public override void Dispose()
    {
        beforeFrameTasks.Clear();

        if (isServer && Sapi != null)
        {
            foreach (GameSystem system in gameSystems)
            {
                try
                {
                    system.OnClose();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error closing {system.GetType().Name}: {e}");
                }
            }
            Server = null!;
            Sapi = null!;
            ServerHook = null!;
        }
        else if (Capi != null)
        {
            DynamicFontAtlas.OnClosing();
            FontRegistry.Dispose();

            foreach (GameSystem system in gameSystems)
            {
                try
                {
                    system.OnClose();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error closing {system.GetType().Name}: {e}");
                }
            }

            OnWindowResize = null;
            OnGuiRescale = null;
            GuiQuad?.Dispose();
            GuiQuad = null!;
            UboRegistry.Dispose();
            RenderTools.OnStop();
            NuttyShaderRegistry.Dispose();

            ClientCache.Dispose();
            VanillaThemes.ClearCache();

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

        Harmony = new Harmony("nutslib");
        Harmony.PatchCategory("core");
    }

    public static void Unpatch()
    {
        if (Harmony == null) return;

        Harmony.UnpatchAll("nutslib");
        Harmony = null;
    }

    public double RenderOrder => 1000;
    public int RenderRange => 0;

    // Pretty much the only rendering that should be done from this library.

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        FrameNumber++;

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

        RenderGlobals globals = new(OriginViewMatrix, PerspectiveMatrix, OrthographicMatrix)
        {
            zNear = zNear,
            zFar = zFar
        };

        // Update global ubo.
        RenderTools.UpdateRenderGlobals(globals);

        Vec3d cameraPos = Client.MainCamera.CameraEyePos;
        OriginOffset = new Vector3d(cameraPos.X, cameraPos.Y, cameraPos.Z);
        CameraPosition = new Vector3d(cameraPos.X, cameraPos.Y, cameraPos.Z);
        CameraNormal = front;

        if (previousRenderWidth != RenderWidth || previousRenderHeight != RenderHeight)
        {
            OnWindowResize?.Invoke(RenderWidth, RenderHeight);
        }

        if (previousGuiScale != GuiScale)
        {
            OnGuiRescale?.Invoke(GuiScale);
        }

        while (beforeFrameTasks.Count > 0)
        {
            beforeFrameTasks.Dequeue().Invoke();
        }
    }
}