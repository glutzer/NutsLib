using MareLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Client;

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

    }

    public static bool Listen(KeyCombination keyComb)
    {

        return true;
    }
}