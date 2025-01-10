using MareLib;
using OpenTK.Mathematics;
using ProtoBuf;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Equimancy;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class SpellPacket
{
    public long spellInstance;
    public int id;
    public byte[]? data;
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public struct SpellPositionPacket
{
    public Vector3d position;
}

/// <summary>
/// Manages networking of spells.
/// Spells function similarly to entities, but at a base only have a position.
/// </summary>
[GameSystem]
public class SpellManager : NetworkedGameSystem, IRenderer
{
    private readonly Dictionary<long, Spell> activeSpells = new();
    private readonly Dictionary<string, Type> spellTypeMapping = new();
    private long nextInstanceId;

    public SpellManager(bool isServer, ICoreAPI api) : base(isServer, api, "spellmanager")
    {

    }

    public override void Initialize()
    {
        RegisterSpells();
    }

    private void OnOtherTick()
    {
        // Remove all dead spells.
        activeSpells.RemoveAll((inst, spell) =>
        {
            if (!spell.Alive)
            {
                spell.OnRemoved();
                return true;
            }

            return false;
        });

        if (activeSpells.Count == 0)
        {
            OnLastSpellRemoved();
            return;
        }

        foreach (Spell spell in activeSpells.Values)
        {
            spell.OnTick();
        }
    }

    protected override void RegisterClientMessages(IClientNetworkChannel channel)
    {
        channel.RegisterMessageType<SpellPacket>()
            .SetMessageHandler<SpellPacket>(HandleSpellPacket)
            .RegisterMessageType<SpellPositionPacket>();
    }

    protected override void RegisterServerMessages(IServerNetworkChannel channel)
    {
        channel.RegisterMessageType<SpellPacket>()
            .RegisterMessageType<SpellPositionPacket>();
    }

    /// <summary>
    /// Server sends updates through packets to spells, not constant attributes.
    /// </summary>
    public void HandleSpellPacket(SpellPacket packet)
    {
        if (activeSpells.TryGetValue(packet.spellInstance, out Spell? spell))
        {
            spell.HandlePacket(packet.id, packet.data);
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

    public void OnFirstSpellAdded()
    {
        MainAPI.GetGameSystem<TickManager>(api.Side).OnOtherTick += OnOtherTick;
        if (!isServer) MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.Before);
    }

    public void OnLastSpellRemoved()
    {
        MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.Before);
        if (!isServer) MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.Before);
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
    /// Spawn a spell at a position.
    /// Only called server-side.
    /// </summary>
    public void SpawnSpell(string code, Vector3d position, SpellConfig config)
    {
        if (!spellTypeMapping.TryGetValue(code, out Type? spellType))
        {
            return;
        }

        Spell spell = (Spell)Activator.CreateInstance(spellType, this, nextInstanceId++, position)!;
        spell.Initialize(config);
        activeSpells[spell.InstanceId] = spell;

        if (activeSpells.Count == 1)
        {
            OnFirstSpellAdded();
        }
    }

    /// <summary>
    /// Spawn a spell at a position.
    /// Only called server-side.
    /// </summary>
    public void SpawnSpell<T>(Vector3d position, SpellConfig config) where T : Spell
    {
        T spell = (T)Activator.CreateInstance(typeof(T), this, nextInstanceId++, position)!;
        spell.Initialize(config);
        activeSpells[spell.InstanceId] = spell;

        if (activeSpells.Count == 1)
        {
            OnFirstSpellAdded();
        }
    }

    public double RenderOrder => 1;
    public int RenderRange => 0;
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}