using MareLib;
using OpenTK.Mathematics;
using System.Collections.Generic;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Equimancy;

public class SpellAttribute : ClassAttribute
{
    public SpellAttribute()
    {

    }
}

/// <summary>
/// Spell, instance is spawned by the spell manager.
/// </summary>
public abstract class Spell
{
    // Only set on client.
    private Vector3d lastPosition;
    private Vector3d nextPosition;

    public Vector3d Position { get; private set; }

    /// <summary>
    /// Set position here when the client receives a packet.
    /// </summary>
    public void OnNewClientPosition(Vector3d position)
    {
        lastPosition = Position;
        nextPosition = position;
    }

    /// <summary>
    /// Sets client position.
    /// Takes current delta (time between last position received and the interval between packets) at a 0-1 range.
    /// </summary>
    public void SetPositionFromTickDelta(float delta)
    {
        Position = Vector3d.Lerp(lastPosition, nextPosition, delta);
    }

    public bool Alive { get; private set; } = true;

    /// <summary>
    /// For the server, what players are in range and will receive updates.
    /// </summary>
    public readonly List<IServerPlayer> trackedPlayers = new();
    public IServerPlayer? castedBy;

    public readonly SpellManager spellManager;
    public long InstanceId { get; private set; }

    public Spell(SpellManager spellManager, long instanceId, Vector3d position)
    {
        this.spellManager = spellManager;
        InstanceId = instanceId;

        lastPosition = position;
        Position = position;
        nextPosition = position;
    }

    /// <summary>
    /// Spell will be initialized with a set of attributes. All attributes will be synced from server -> client.
    /// Maybe also have spawn attributes?
    /// </summary>
    public virtual void Initialize(SpellConfig spellConfig)
    {

    }

    /// <summary>
    /// Called 10 times per second on active spells, on client and server.
    /// </summary>
    public virtual void OnTick()
    {

    }

    /// <summary>
    /// Removes spell.
    /// Will be removed during next tick update.
    /// </summary>
    public void Kill()
    {
        Alive = false;
    }

    /// <summary>
    /// Handle a packet on the client.
    /// </summary>
    public virtual void HandlePacket(int id, byte[]? data)
    {

    }

    /// <summary>
    /// Send update packet from server.
    /// </summary>
    public void SendPacket(int id)
    {
        spellManager.SendPacket(new SpellPacket() { spellInstance = InstanceId, id = id });
    }

    /// <summary>
    /// Send update packet from server with object.
    /// </summary>
    public void SendPacket<T>(int id, T data)
    {
        spellManager.SendPacket(new SpellPacket() { spellInstance = InstanceId, id = id, data = SerializerUtil.Serialize(data) });
    }

    /// <summary>
    /// When spell is removed by any means.
    /// </summary>
    public virtual void OnRemoved()
    {

    }
}