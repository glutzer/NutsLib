using MareLib;
using ProtoBuf;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Equimancy;

/// <summary>
/// Saves/loads player data and manages it.
/// </summary>
[GameSystem]
public class EquimancySaveDataSystem : NetworkedGameSystem
{
    private readonly DatabaseFile equimancyDb;
    private readonly Dictionary<string, EquimancyPlayerData> playerData = new();

    public EquimancySaveDataSystem(bool isServer, ICoreAPI api) : base(isServer, api, "epd")
    {
        equimancyDb = new DatabaseFile($"Equimancy/{api.World.SavegameIdentifier}/equimancy.db");
    }

    protected override void RegisterClientMessages(IClientNetworkChannel channel)
    {
        channel
            .RegisterMessageType(typeof(EquimancyPlayerData))
            .SetMessageHandler<EquimancyPlayerData>(ClientReceivesPlayerData);
    }

    protected override void RegisterServerMessages(IServerNetworkChannel channel)
    {
        channel
            .RegisterMessageType(typeof(EquimancyPlayerData));
    }

    private void ClientReceivesPlayerData(EquimancyPlayerData packet)
    {
        // Do not replace already initialized data.
        EquimancyPlayerData epd = GetPlayerData(packet.PlayerName);
        byte[] data = SerializerUtil.Serialize(epd);
        SerializerUtil.DeserializeInto(epd, data);
    }

    public override void PreInitialize()
    {
        if (api is ICoreServerAPI sapi)
        {
            sapi.Event.PlayerJoin += LoadPlayerData;
        }
    }

    /// <summary>
    /// Loads data from the db.
    /// </summary>
    private void LoadPlayerData(IServerPlayer player)
    {
        equimancyDb.Open();
        EquimancyPlayerData? epd = equimancyDb.Get<EquimancyPlayerData>("playerData", player.PlayerName);

        if (epd == null)
        {
            epd = GetPlayerData(player.PlayerName);
        }
        else
        {
            playerData[player.PlayerName] = epd;
        }

        equimancyDb.Close();

        // Send the player his data.
        SendPacket(epd, player);
    }

    public EquimancyPlayerData GetPlayerData(string playerName)
    {
        if (!playerData.TryGetValue(playerName, out EquimancyPlayerData? epd))
        {
            // Re-initialize it in case it was never made.
            epd = new EquimancyPlayerData(playerName);
            playerData[playerName] = epd;
        }

        return epd;
    }

    public override void OnClose()
    {
        equimancyDb.Open();

        foreach (EquimancyPlayerData data in playerData.Values)
        {
            equimancyDb.Insert("playerData", data.PlayerName, data);
        }

        equimancyDb.Close();
    }
}

[ProtoContract]
public class EquimancyPlayerData
{
    [ProtoMember(1)]
    public string PlayerName { get; private set; }

    [ProtoMember(2)]
    public float Mana { get; private set; } = 100;

    [ProtoMember(3)]
    public float MaxMana { get; private set; } = 100;

    public EquimancyPlayerData(string playerName)
    {
        PlayerName = playerName;
    }

#pragma warning disable CS8618
    public EquimancyPlayerData()
#pragma warning restore CS8618
    {

    }

    public void ChangeMana(float amount)
    {
        Mana += amount;
        Mana = Math.Clamp(amount, 0, MaxMana);
    }
}