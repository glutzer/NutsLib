using Vintagestory.API.Common;
using Vintagestory.Common;

namespace MareLib;

/// <summary>
/// System abstraction, loaded by MainHook.
/// </summary>
public abstract class GameSystem
{
    public readonly bool isServer;
    public readonly ICoreAPI api;
    public readonly GameMain game;

    protected GameSystem(bool isServer, ICoreAPI api)
    {
        this.isServer = isServer;
        this.api = api;
        game = (GameMain)api.World;
    }

    /// <summary>
    /// Called in StartPre.
    /// </summary>
    public virtual void PreInitialize()
    {

    }

    /// <summary>
    /// Called in StartPre.
    /// </summary>
    public virtual void Initialize()
    {

    }

    /// <summary>
    /// Called in StartPre.
    /// </summary>
    public virtual void PostInitialize()
    {

    }

    /// <summary>
    /// Called on AssetsLoaded.
    /// </summary>
    public virtual void OnAssetsLoaded()
    {

    }

    /// <summary>
    /// Called on ClientStart/ServerStart.
    /// </summary>
    public virtual void OnStart()
    {

    }

    /// <summary>
    /// Called when shutting down.
    /// </summary>
    public virtual void OnClose()
    {

    }
}