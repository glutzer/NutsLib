using MareLib;
using OpenTK.Mathematics;
using ProtoBuf;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Client;

namespace Equimancy;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class SpellSpawnPacket
{
    public long instanceId;
    public string code = null!;
    public SpellConfig config = null!;
    public double X;
    public double Y;
    public double Z;
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public struct SpellPositionPacket
{
    public long instanceId;
    public double X;
    public double Y;
    public double Z;
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public struct SpellDespawnPacket
{
    public long instanceId;
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public struct ClientSpellCast
{
    public bool startedCasting;
}

public class TrackedPlayer : IEquatable<TrackedPlayer>
{
    public IServerPlayer player;
    public readonly string uid;
    public readonly int hash;

    public bool currentlyCasting;
    public long lastCastTime;

    private readonly List<SpellPacket> spellPackets = new();
    private readonly List<SpellSpawnPacket> spellsToSpawn = new();
    private readonly List<SpellPositionPacket> positionsToUpdate = new();
    private readonly List<SpellDespawnPacket> spellsToDespawn = new();

    public TrackedPlayer(IServerPlayer player)
    {
        this.player = player;
        uid = player.PlayerUID;
        hash = uid.GetHashCode();
    }

    public bool Equals(TrackedPlayer? other)
    {
        return other != null && other.uid == uid;
    }

    public override int GetHashCode()
    {
        return hash;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TrackedPlayer);
    }

    public void AddSpellPacket(Spell spell, int packetId, byte[]? data)
    {
        SpellPacket packet = new()
        {
            instanceId = spell.InstanceId,
            packetId = packetId,
            data = data
        };
        spellPackets.Add(packet);
    }

    public void AddSpellPositionPacket(Spell spell)
    {
        SpellPositionPacket packet = new()
        {
            instanceId = spell.InstanceId,
            X = spell.Position.X,
            Y = spell.Position.Y,
            Z = spell.Position.Z
        };
        positionsToUpdate.Add(packet);
    }

    public void AddSpellSpawnPacket(Spell spell)
    {
        SpellSpawnPacket packet = new()
        {
            instanceId = spell.InstanceId,
            code = spell.code,
            config = spell.config,
            X = spell.Position.X,
            Y = spell.Position.Y,
            Z = spell.Position.Z
        };
        spellsToSpawn.Add(packet);
    }

    public void AddSpellDespawnPacket(Spell spell)
    {
        SpellDespawnPacket packet = new()
        {
            instanceId = spell.InstanceId
        };
        spellsToDespawn.Add(packet);
    }

    /// <summary>
    /// Send entire list of packets for this player.
    /// </summary>
    public void DispatchPackets(SpellManager spellManager)
    {
        // First spawns.
        if (spellsToSpawn.Count > 0)
        {
            spellManager.SendPacket(spellsToSpawn, player);
            spellsToSpawn.Clear();
        }

        if (positionsToUpdate.Count > 0)
        {
            spellManager.SendPacket(positionsToUpdate, player);
            positionsToUpdate.Clear();
        }

        if (spellPackets.Count > 0)
        {
            spellManager.SendPacket(spellPackets, player);
            spellPackets.Clear();
        }

        if (spellsToDespawn.Count > 0)
        {
            spellManager.SendPacket(spellsToDespawn, player);
            spellsToDespawn.Clear();
        }
    }
}

/// <summary>
/// Manages networking of spells.
/// Spells function similarly to entities, but at a base only have a position.
/// </summary>
[GameSystem]
public class SpellManager : NetworkedGameSystem, IRenderer
{
    public const double SPELL_TRACKING_RANGE = 500.0;

    private readonly Dictionary<long, Spell> activeSpells = new();
    private readonly Dictionary<string, Type> spellTypeMapping = new();
    private long nextInstanceId;

    public readonly Dictionary<string, TrackedPlayer> uidToTrackedPlayer = new();

    public SpellManager(bool isServer, ICoreAPI api) : base(isServer, api, "spellmanager")
    {

    }

    // TRACKING vvv.

    private void OnPlayerJoin(IServerPlayer byPlayer)
    {
        TrackedPlayer trackedPlayer = new(byPlayer);
        uidToTrackedPlayer[trackedPlayer.uid] = trackedPlayer;

        // Get every spell in range.
        foreach (Spell spell in activeSpells.Values)
        {
            double distance = Vector3d.Distance(byPlayer.Entity.Pos.ToVector(), spell.Position);

            if (distance < SPELL_TRACKING_RANGE)
            {
                spell.trackedPlayers.Add(trackedPlayer);
                spell.OnTrackingPlayer(trackedPlayer);

                // SEND SPAWN PACKET.
                trackedPlayer.AddSpellSpawnPacket(spell);
            }
        }
    }

    private void OnPlayerLeave(IServerPlayer byPlayer)
    {
        // Remove tracked player and all spells involving him.
        if (uidToTrackedPlayer.TryGetValue(byPlayer.PlayerUID, out TrackedPlayer? player))
        {
            foreach (Spell spell in activeSpells.Values)
            {
                if (spell.trackedPlayers.Remove(player))
                {
                    spell.OnNoLongerTrackingPlayer(player);
                }

                // Unmanned spell.
                if (spell.castedBy == player.player?.Entity) spell.Kill();
            }

            uidToTrackedPlayer.Remove(player.uid);
        }
    }

    // TRACKING ^^^.

    public override void Initialize()
    {
        RegisterSpells();
    }

    public override void OnStart()
    {
        if (isServer)
        {
            MainAPI.Sapi.Event.PlayerJoin += OnPlayerJoin;
            MainAPI.Sapi.Event.PlayerLeave += OnPlayerLeave;
        }
        else
        {
            ScreenManager.hotkeyManager.RegisterHotKey("castspell", "Cast Spell", (int)GlKeys.V, triggerOnUpAlso: true);
            MainAPI.Capi.Input.SetHotKeyHandler("castspell", key =>
            {
                if (key.OnKeyUp)
                {
                    SendPacket(new ClientSpellCast() { startedCasting = false });
                }
                else
                {
                    SendPacket(new ClientSpellCast() { startedCasting = true });
                }

                return true;
            });
        }
    }

    public void HandleClientSpellCast(IServerPlayer player, ClientSpellCast packet)
    {
        if (uidToTrackedPlayer.TryGetValue(player.PlayerUID, out TrackedPlayer? trackedPlayer))
        {
            if (trackedPlayer.currentlyCasting != packet.startedCasting)
            {
                trackedPlayer.currentlyCasting = packet.startedCasting;

                if (trackedPlayer.currentlyCasting)
                {
                    trackedPlayer.lastCastTime = api.World.ElapsedMilliseconds;

                    // Cast a spell placeholder.
                    SpellConfig config = new();
                    config.SetCastedBy(trackedPlayer.player.Entity);
                    //SpawnSpell<Levitate>(trackedPlayer.player.Entity.Pos.ToVector(), config);
                    SpawnSpell<ArcLightning>(trackedPlayer.player.Entity.Pos.ToVector(), config);   
                }
            }
        }
    }

    /// <summary>
    /// Tick on client and server.
    /// Remove spells flagged as dead, call tick event.
    /// </summary>
    private void OnOtherTick()
    {
        // Remove all dead spells.
        activeSpells.RemoveAll((inst, spell) =>
        {
            if (!spell.Alive)
            {
                spell.OnRemoved();

                if (isServer)
                {
                    foreach (TrackedPlayer player in spell.trackedPlayers)
                    {
                        player.AddSpellDespawnPacket(spell);
                    }
                }

                return true;
            }

            return false;
        });

        if (activeSpells.Count == 0)
        {
            // Also dispatch packets here.
            foreach (TrackedPlayer player in uidToTrackedPlayer.Values)
            {
                player.DispatchPackets(this);
            }

            OnLastSpellRemoved();
            return;
        }

        foreach (Spell spell in activeSpells.Values)
        {
            spell.OnTick();
        }

        if (isServer)
        {
            // Update trackers, use a queue or quadtree for better optimization later.
            foreach (Spell spell in activeSpells.Values)
            {
                foreach (TrackedPlayer player in uidToTrackedPlayer.Values)
                {
                    double distance = Vector3d.Distance(player.player.Entity.Pos.ToVector(), spell.Position);
                    if (distance < SPELL_TRACKING_RANGE)
                    {
                        if (!spell.trackedPlayers.Contains(player))
                        {
                            spell.trackedPlayers.Add(player);
                            spell.OnTrackingPlayer(player);
                            player.AddSpellSpawnPacket(spell);
                        }
                    }
                    else
                    {
                        if (spell.trackedPlayers.Remove(player))
                        {
                            spell.OnNoLongerTrackingPlayer(player);
                            player.AddSpellDespawnPacket(spell);
                        }
                    }
                }
            }

            // Add all position update packets.
            foreach (Spell spell in activeSpells.Values)
            {
                foreach (TrackedPlayer player in spell.trackedPlayers)
                {
                    player.AddSpellPositionPacket(spell);
                }
            }

            // At the end of the tick, dispatch every packet.
            foreach (TrackedPlayer player in uidToTrackedPlayer.Values)
            {
                player.DispatchPackets(this);
            }
        }
    }

    protected override void RegisterClientMessages(IClientNetworkChannel channel)
    {
        channel.RegisterMessageType<List<SpellPacket>>()
            .SetMessageHandler<List<SpellPacket>>(HandleSpellPackets)

            .RegisterMessageType<List<SpellPositionPacket>>()
            .SetMessageHandler<List<SpellPositionPacket>>(HandleSpellPositionPackets)

            .RegisterMessageType<List<SpellSpawnPacket>>()
            .SetMessageHandler<List<SpellSpawnPacket>>(HandleSpellSpawnPackets)

            .RegisterMessageType<List<SpellDespawnPacket>>()
            .SetMessageHandler<List<SpellDespawnPacket>>(HandleSpellDespawnPackets)

            .RegisterMessageType<ClientSpellCast>();
    }

    protected override void RegisterServerMessages(IServerNetworkChannel channel)
    {
        channel.RegisterMessageType<List<SpellPacket>>()

            .RegisterMessageType<List<SpellPositionPacket>>()

            .RegisterMessageType<List<SpellSpawnPacket>>()

            .RegisterMessageType<List<SpellDespawnPacket>>()

            .RegisterMessageType<ClientSpellCast>()
            .SetMessageHandler<ClientSpellCast>(HandleClientSpellCast);
    }

    /// <summary>
    /// Server sends updates through packets to spells, not constant attributes.
    /// </summary>
    public void HandleSpellPackets(List<SpellPacket> packets)
    {
        foreach (SpellPacket packet in packets)
        {
            if (activeSpells.TryGetValue(packet.instanceId, out Spell? spell))
            {
                spell.HandlePacket(packet);
            }
        }
    }

    /// <summary>
    /// Server sends out packets at 10 TPS for every spell.
    /// </summary>
    public void HandleSpellPositionPackets(List<SpellPositionPacket> packets)
    {
        foreach (SpellPositionPacket packet in packets)
        {
            if (activeSpells.TryGetValue(packet.instanceId, out Spell? spell))
            {
                spell.OnNewClientPosition(new Vector3d(packet.X, packet.Y, packet.Z));
            }
        }
    }

    /// <summary>
    /// Handle all spell spawns on the client.
    /// </summary>
    public void HandleSpellSpawnPackets(List<SpellSpawnPacket> packets)
    {
        foreach (SpellSpawnPacket packet in packets)
        {
            SpawnSpellOnClient(packet.code, new Vector3d(packet.X, packet.Y, packet.Z), packet.config, packet.instanceId);
        }
    }

    /// <summary>
    /// Handle all spell despawns on the client.
    /// </summary>
    public void HandleSpellDespawnPackets(List<SpellDespawnPacket> packets)
    {
        foreach (SpellDespawnPacket packet in packets)
        {
            if (activeSpells.TryGetValue(packet.instanceId, out Spell? spell))
            {
                spell.Kill();
            }
        }
    }

    private float clientAccum;
    private const float PacketInterval = 1 / 10f;

    /// <summary>
    /// Lerp spell positions.
    /// Spells register their own renderers (like FX).
    /// </summary>
    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        clientAccum += dt;

        // Delta is reset upon receiving a position.
        float delta = Math.Clamp(clientAccum / PacketInterval, 0, 1);

        foreach (Spell spell in activeSpells.Values)
        {
            spell.SetPositionFromTickDelta(delta);
        }
    }

    /// <summary>
    /// Spawn a spell at a position.
    /// Only called server-side.
    /// </summary>
    public void SpawnSpell<T>(Vector3d position, SpellConfig? config) where T : Spell
    {
        string code = InnerClass<T>.Name;

        SpawnSpell(code, position, config);
    }

    /// <summary>
    /// Spawn a spell at a position.
    /// Only called server-side.
    /// </summary>
    public void SpawnSpell(string code, Vector3d position, SpellConfig? config)
    {
        if (!isServer) return;

        if (!spellTypeMapping.TryGetValue(code, out Type? spellType))
        {
            return;
        }

        Spell spell = (Spell)Activator.CreateInstance(spellType, this, nextInstanceId++, position, code, config)!;
        activeSpells[spell.InstanceId] = spell;

        if (activeSpells.Count == 1)
        {
            OnFirstSpellAdded();
        }

        // Spawn newly created spell.
        foreach (TrackedPlayer player in uidToTrackedPlayer.Values)
        {
            if (Vector3d.Distance(player.player.Entity.Pos.ToVector(), position) < SPELL_TRACKING_RANGE)
            {
                spell.trackedPlayers.Add(player);
                spell.OnTrackingPlayer(player);
                player.AddSpellSpawnPacket(spell);
            }
        }

        spell.Initialize();
    }

    /// <summary>
    /// Spawn a spell when receiving a spawn packet.
    /// </summary>
    public void SpawnSpellOnClient(string code, Vector3d position, SpellConfig config, long instanceId)
    {
        if (!spellTypeMapping.TryGetValue(code, out Type? spellType))
        {
            return;
        }

        Spell spell = (Spell)Activator.CreateInstance(spellType, this, instanceId, position, code, config)!;
        spell.Initialize();
        activeSpells[spell.InstanceId] = spell;

        if (activeSpells.Count == 1)
        {
            OnFirstSpellAdded();
        }
    }

    /// <summary>
    /// On first spell active on the client or server.
    /// </summary>
    public void OnFirstSpellAdded()
    {
        MainAPI.GetGameSystem<TickManager>(api.Side).OnOtherTick += OnOtherTick;
        if (!isServer) MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.Before);
    }

    /// <summary>
    /// On last spell removed on the client or server.
    /// </summary>
    public void OnLastSpellRemoved()
    {
        MainAPI.GetGameSystem<TickManager>(api.Side).OnOtherTick -= OnOtherTick;
        if (!isServer) MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.Before);
    }

    private void RegisterSpells()
    {
        (Type, SpellAttribute)[] spells = AttributeUtilities.GetAllAnnotatedClasses<SpellAttribute>();
        foreach ((Type, SpellAttribute) spellType in spells)
        {
            spellTypeMapping[spellType.Item1.Name] = spellType.Item1;
        }
    }

    /// <summary>
    /// Try to get an entity as a tracked player, null if none.
    /// </summary>
    public TrackedPlayer? TryGetTrackedPlayerFromCaster(Entity? entity)
    {
        if (entity is EntityPlayer player && player.Player is IServerPlayer serverPlayer)
        {
            uidToTrackedPlayer.TryGetValue(serverPlayer.PlayerUID, out TrackedPlayer? trackedPlayer);
            return trackedPlayer;
        }

        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public double RenderOrder => 1;
    public int RenderRange => 0;
}