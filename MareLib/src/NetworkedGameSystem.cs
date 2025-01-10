using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace MareLib;

/// <summary>
/// Also contains a framework for sending packets between client and server.
/// </summary>
public abstract class NetworkedGameSystem : GameSystem
{
    private readonly IClientNetworkChannel clientChannel = null!;
    private readonly IServerNetworkChannel serverChannel = null!;

    public NetworkedGameSystem(bool isServer, ICoreAPI api, string channelName) : base(isServer, api)
    {
        if (isServer)
        {
            serverChannel = (IServerNetworkChannel)api.Network.RegisterChannel(channelName);
            RegisterServerMessages(serverChannel);
        }
        else
        {
            clientChannel = (IClientNetworkChannel)api.Network.RegisterChannel(channelName);
            RegisterClientMessages(clientChannel);
        }
    }

    protected abstract void RegisterClientMessages(IClientNetworkChannel channel);
    protected abstract void RegisterServerMessages(IServerNetworkChannel channel);

    /// <summary>
    /// Send a packet from the client.
    /// </summary>
    public void SendPacket<T>(T packet)
    {
        clientChannel.SendPacket(packet);
    }

    /// <summary>
    /// Send a packet from the server.
    /// </summary>
    public void SendPacket<T>(T packet, params IServerPlayer[] players)
    {
        serverChannel.SendPacket(packet, players);
    }

    /// <summary>
    /// Send a packet from the server.
    /// </summary>
    public void BroadcastPacket<T>(T packet, params IServerPlayer[] exceptPlayers)
    {
        serverChannel.BroadcastPacket(packet, exceptPlayers);
    }
}