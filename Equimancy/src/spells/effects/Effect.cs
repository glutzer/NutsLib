using MareLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common.Entities;

namespace Equimancy;

[JsonObject(MemberSerialization.OptIn)]
public abstract class Effect
{
    /// <summary>
    /// Is this entity removed when the entity dies? (Usually only applicable to players.)
    /// </summary>
    public virtual bool PersistThroughDeath => false;
    public virtual EffectType Type => EffectType.Instant;

    [JsonProperty]
    public virtual float Duration { get; set; } = 10f;

    public Effect()
    {
        
    }

    public virtual void OnTick(float dt)
    {

    }

    /// <summary>
    /// Called before meta effects applied when the instance is created.
    /// Also called when loaded from sync or disk before OnLoaded.
    /// Anything that relies on meta effects should be done in OnLoaded (BaseStrength is already set at this point).
    /// </summary>
    public virtual void Initialize(Entity entity, EntityBehaviorEffects effectSystem)
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
    /// Called when effect is over.
    /// Not called when entity is unloaded, only when effect duration expires.
    /// </summary>
    public virtual void OnDurationExpired()
    {

    }

    /// <summary>
    /// Dispose event listeners. Called when duration expires or when unloading.
    /// </summary>
    public virtual void OnEntityUnloaded()
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
    public string code;

    public EffectAttribute(string code)
    {
        this.code = code;
    }
}

/// <summary>
/// What is this for, storing special attributes about an effect? Not used.
/// </summary>
public class EffectProps : IComparable<EffectProps>
{
    public string effect = "none";

    public Dictionary<string, string> properties = new();

    public override int GetHashCode()
    {
        string effect = this.effect;

        foreach (KeyValuePair<string, string> property in properties)
        {
            effect += property.Key + property.Value;
        }

        return effect.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj?.GetHashCode() == GetHashCode();
    }

    public int CompareTo(EffectProps? other)
    {
        return effect.CompareTo(other?.effect);
    }
}