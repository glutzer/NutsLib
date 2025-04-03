using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace MareLib;

[JsonObject(MemberSerialization.OptIn)]
public abstract class Effect
{
    /// <summary>
    /// Code of this effect, which is the type name.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Is this entity removed when the entity dies? (Usually only applicable to players.)
    /// </summary>
    public virtual bool PersistThroughDeath => false;
    public virtual EffectType Type => EffectType.Instant;

    public virtual float BaseDuration => 10f;

    [JsonProperty]
    public float Duration { get; set; }

    /// <summary>
    /// Available after SetBehavior.
    /// </summary>
    public Entity Entity { get; private set; } = null!;

    /// <summary>
    /// Available after SetBehavior.
    /// </summary>
    public EntityBehaviorEffects EffectBehavior { get; private set; } = null!;

    /// <summary>
    /// Set in SetBehavior, will be false before then.
    /// </summary>
    public bool IsServer { get; private set; }

    public Effect()
    {
        Code = GetType().Name;
        Duration = BaseDuration;
    }

    public virtual void OnTick(float dt)
    {

    }

    /// <summary>
    /// Called when apply effect is called, or load effect data on the client.
    /// </summary>
    public void SetBehavior(Entity entity, EntityBehaviorEffects effectsSystem)
    {
        Entity = entity;
        EffectBehavior = effectsSystem;
        IsServer = entity.Api.Side == EnumAppSide.Server;
    }

    /// <summary>
    /// Called once on the server when the effect is first created.
    /// </summary>
    public virtual void Initialize()
    {

    }

    /// <summary>
    /// Called when adding effect to active effects, after every effect has been initialized and meta effects applied.
    /// Also called when loaded from sync or disk.
    /// </summary>
    public virtual void OnLoaded()
    {

    }

    /// <summary>
    /// Returns if the effect should be overwritten completely by the new one.
    /// Only applies to duration effects.
    /// Return false if merging.
    /// </summary>
    public virtual bool MergeEffects(Effect other)
    {
        return true;
    }

    /// <summary>
    /// Applies when effect is first applied regardless of type.
    /// All meta effects have been applied at this point.
    /// This only applies on the server.
    /// </summary>
    public virtual void ApplyInstantEffect()
    {

    }

    /// <summary>
    /// Dispose event listeners. Called when duration expires OR when unloading.
    /// </summary>
    public virtual void OnUnloaded()
    {

    }

    /// <summary>
    /// Called on the server when the duration expires. Not called if removed.
    /// </summary>
    public virtual void OnDurationExpired()
    {

    }
}

public enum EffectType
{
    Instant,
    Duration
}

public class EffectAttribute : ClassAttribute
{
    public EffectAttribute()
    {

    }
}