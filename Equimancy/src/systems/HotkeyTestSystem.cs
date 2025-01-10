using Equimancy.src.spells.fx.fxtypes;
using MareLib;
using OpenTK.Mathematics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace Equimancy;

[GameSystem]
public class HotkeyTestSystem : NetworkedGameSystem
{
    public HotkeyTestSystem(bool isServer, ICoreAPI api) : base(isServer, api, "keytest")
    {
    }

    protected override void RegisterClientMessages(IClientNetworkChannel channel)
    {

    }

    protected override void RegisterServerMessages(IServerNetworkChannel channel)
    {

    }

    public override void OnStart()
    {
        if (!isServer)
        {
            MainAPI.Capi.Input.RegisterHotKey("testeffect", "Test Effect", GlKeys.V);
            MainAPI.Capi.Input.SetHotKeyHandler("testeffect", Listen);
        }
    }

    public static bool Listen(KeyCombination keyComb)
    {
        FXManager.GetFX<ArcLightning>().SpawnInstance(new ArcLightningInstance(() =>
        {
            EntityPos playerPos = MainAPI.Capi.World.Player.Entity.Pos;
            Vector3d pos = new(playerPos.X, playerPos.Y + 2, playerPos.Z);
            return pos;
        }, () =>
        {
            BlockSelection bs = MainAPI.Capi.World.Player.CurrentBlockSelection;
            if (bs == null) return Vector3d.Zero;
            Vector3d selPos = new(bs.Position.X + 0.5f, bs.Position.Y + 0.5f, bs.Position.Z + 0.5f);
            return selPos;
        }));

        return true;
    }
}