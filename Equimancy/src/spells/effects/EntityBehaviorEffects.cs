using MareLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Equimancy;

/// <summary>
/// Delegate on the damage receiver's effects.
/// </summary>
public delegate void DamageModifierDelegate(ref float damage, DamageSource source);

/// <summary>
/// Delegate invoked on the attacker's effects.
/// </summary>
public delegate void DamageModifierToDelegate(ref float damage, DamageSource source, Entity toEntity);

public static class EntityExtensions
{
    public static void AddEffect(this Entity entity, Effect effect)
    {
        entity.GetBehavior<EntityBehaviorEffects>()?.ApplyEffect(effect);
    }

    public static bool GetEffect<T>(this Entity entity, [NotNullWhen(true)] out T? effect) where T : Effect
    {
        effect = null;
        return entity.GetBehavior<EntityBehaviorEffects>()?.GetEffect(out effect) ?? false;
    }

    public static bool HasEffect<T>(this Entity entity) where T : Effect
    {
        return entity.GetBehavior<EntityBehaviorEffects>()?.HasEffect<T>() ?? false;
    }

    public static void RemoveEffect<T>(this Entity entity) where T : Effect
    {
        entity.GetBehavior<EntityBehaviorEffects>()?.RemoveEffect<T>();
    }

    public static EntityBehaviorEffects? GetEffectBehavior(this Entity entity)
    {
        return entity.GetBehavior<EntityBehaviorEffects>();
    }
}

/// <summary>
/// Holds effects currently on an entity.
/// </summary>
[EntityBehavior]
public class EntityBehaviorEffects : EntityBehavior
{
    public override string PropertyName()
    {
        return "effects";
    }

    public SortedDictionary<string, Effect> ActiveEffects { get; private set; } = new();
    private readonly List<string> deadEffects = new();

    public DamageModifierDelegate? onDamaged;
    public DamageModifierToDelegate? onDamaging;

    public EntityBehaviorEffects(Entity entity) : base(entity)
    {
    }

    public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
    {
        if (damageSource.SourceEntity is EntityAgent sourceAgent)
        {
            EntityBehaviorEffects? behavior = sourceAgent.GetBehavior<EntityBehaviorEffects>();
            behavior?.onDamaging?.Invoke(ref damage, damageSource, entity);
        }
        else if (damageSource.CauseEntity is EntityAgent causeAgent)
        {
            EntityBehaviorEffects? behavior = causeAgent.GetBehavior<EntityBehaviorEffects>();
            behavior?.onDamaging?.Invoke(ref damage, damageSource, entity);
        }

        onDamaged?.Invoke(ref damage, damageSource);
    }

    public override void Initialize(EntityProperties properties, JsonObject attributes)
    {
        if (entity.Api.Side == EnumAppSide.Client) entity.WatchedAttributes.RegisterModifiedListener("activeEffects", LoadEffectData);
    }

    public override void OnEntityDespawn(EntityDespawnData despawn)
    {
        if (entity.Api.Side == EnumAppSide.Client) entity.WatchedAttributes.UnregisterListener(LoadEffectData);
        ActiveEffects.Values.Foreach(effect => effect.OnEntityUnloaded());
    }

    public override void OnEntityDeath(DamageSource damageSourceForDeath)
    {
        foreach (KeyValuePair<string, Effect> effect in ActiveEffects)
        {
            if (effect.Value.PersistThroughDeath) continue;
            deadEffects.Add(effect.Key);
        }

        if (deadEffects.Count > 0)
        {
            deadEffects.Foreach(effectCode => ActiveEffects.Remove(effectCode));
            deadEffects.Clear();
        }
        ;

        SaveEffectData();
    }

    /// <summary>
    /// Does this entity have an effect of this type?
    /// </summary>
    public bool HasEffect<T>() where T : Effect
    {
        return ActiveEffects.ContainsKey(InnerClass<T>.Name);
    }

    /// <summary>
    /// Tries to get an active effect of this type.
    /// </summary>
    public bool GetEffect<T>([NotNullWhen(true)] out T? effect) where T : Effect
    {
        if (ActiveEffects.TryGetValue(InnerClass<T>.Name, out Effect? foundEffect))
        {
            effect = (T)foundEffect;
            return true;
        }

        effect = null;
        return false;
    }

    /// <summary>
    /// Applies a newly created effect and initializes it.
    /// You can make an effect anywhere.
    /// </summary>
    public void ApplyEffect<T>(T effect) where T : Effect
    {
        string code = InnerClass<T>.Name;

        effect.Initialize(entity, this);

        if (effect.Type == EffectType.Duration)
        {
            if (ActiveEffects.TryGetValue(code, out Effect? existingEffect))
            {
                if (existingEffect.MergeEffects(effect))
                {
                    existingEffect.OnEntityUnloaded();
                    ActiveEffects[code] = effect;
                    effect.OnLoaded();
                }
            }
            else
            {
                ActiveEffects[code] = effect;
                effect.OnLoaded();
            }
        }

        effect.ApplyInstantEffect();

        SaveEffectData();
    }

    /// <summary>
    /// Remove an effect on the server.
    /// </summary>
    public void RemoveEffect<T>() where T : Effect
    {
        string code = InnerClass<T>.Name;
        if (ActiveEffects.TryGetValue(code, out Effect? existingEffect))
        {
            ActiveEffects.Remove(code);
            existingEffect.OnEntityUnloaded();
            // No duration expiry.
            SaveEffectData();
        }
    }

    /// <summary>
    /// Save all effects as json objects, then serialize the json as bytes.
    /// Effects will be moved to watched attributes -> sent to client -> and loaded.
    /// </summary>
    protected virtual void SaveEffectData()
    {
        Dictionary<string, string> container = new();
        foreach (KeyValuePair<string, Effect> effect in ActiveEffects)
        {
            container.Add(effect.Key, JsonConvert.SerializeObject(effect.Value));
        }
        string json = JsonConvert.SerializeObject(container);
        byte[] bytes = SerializerUtil.Serialize(json);
        entity.WatchedAttributes.SetBytes("activeEffects", bytes);
    }

    /// <summary>
    /// When saved, disposes all active effects and re-initializes them on the client only.
    /// </summary>
    public virtual void LoadEffectData()
    {
        // Load bytes.
        byte[] bytes = entity.WatchedAttributes.GetBytes("activeEffects");
        if (bytes == null) return;

        // Load json.
        string json = SerializerUtil.Deserialize<string>(bytes);
        Dictionary<string, string>? container = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (container == null) return;

        // Attempt to deserialize every json entry into a type.
        SortedDictionary<string, Effect> deserialized = new();
        foreach (KeyValuePair<string, string> effect in container)
        {
            if (MainAPI.GetGameSystem<EffectManager>(entity.Api.Side).effectTypes.TryGetValue(effect.Key, out Type? type))
            {
                Effect deserializedEffect = (Effect)JsonConvert.DeserializeObject(effect.Value, type)!;
                deserialized.Add(effect.Key, deserializedEffect);
            }
        }

        // Reload and re-initialize all effects.
        ActiveEffects.Values.Foreach(effect => effect.OnEntityUnloaded());
        ActiveEffects = deserialized;
        foreach (Effect effect in ActiveEffects.Values)
        {
            effect.Initialize(entity, this);
            effect.OnLoaded();
        }
    }

    public float accum;

    /// <summary>
    /// Leverage entity for ticking, 10 TPS.
    /// </summary>
    public override void OnGameTick(float dt)
    {
        if (ActiveEffects.Count == 0) return;

        const float interval = 1 / 10f;

        accum += dt;
        while (accum > interval)
        {
            accum -= interval;

            foreach (KeyValuePair<string, Effect> effect in ActiveEffects)
            {
                effect.Value.Duration -= interval;
                if (effect.Value.Duration <= 0)
                {
                    if (entity.Api.Side == EnumAppSide.Server)
                    {
                        effect.Value.OnDurationExpired(); // Only play expiry effects on server to avoid desync.
                    }

                    deadEffects.Add(effect.Key);
                }
                else
                {
                    effect.Value.OnTick(interval);
                }
            }

            if (deadEffects.Count > 0)
            {
                deadEffects.Foreach(effectCode =>
                {
                    ActiveEffects[effectCode].OnEntityUnloaded();
                    ActiveEffects.Remove(effectCode);
                });
                deadEffects.Clear();

                if (entity.Api.Side == EnumAppSide.Server) SaveEffectData();
            }
        }
    }
}