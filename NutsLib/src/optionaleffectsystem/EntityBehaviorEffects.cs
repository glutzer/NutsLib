using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace NutsLib;

/// <summary>
/// Delegate on the damage receiver's effects.
/// </summary>
public delegate void DamageModifierDelegate(ref float damage, DamageSource source);

/// <summary>
/// Delegate invoked on the attacker's effects.
/// </summary>
public delegate void DamageModifierToDelegate(ref float damage, DamageSource source, Entity toEntity);

public static class EntityEffectExtensions
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
/// Added to every entity with health.
/// </summary>
[EntityBehavior]
public class EntityBehaviorEffects : EntityBehavior
{
    public override string PropertyName()
    {
        return "nuttyeffects";
    }

    public SortedDictionary<string, Effect> ActiveEffects { get; private set; } = [];
    private readonly List<string> deadEffects = [];

    public DamageModifierDelegate? onDamaged;
    public DamageModifierToDelegate? onDamaging;

    public float accum;

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
        if (entity.Api.Side == EnumAppSide.Client)
        {
            entity.WatchedAttributes.RegisterModifiedListener("activeEffects", LoadEffectData);
        }

        LoadEffectData();
    }

    public override void OnEntityDespawn(EntityDespawnData despawn)
    {
        if (entity.Api.Side == EnumAppSide.Client)
        {
            entity.WatchedAttributes.UnregisterListener(LoadEffectData);
        }
        else
        {
            SaveEffectData();
        }

        ActiveEffects.Values.Foreach(effect => effect.OnUnloaded());
    }

    public override void OnEntityDeath(DamageSource damageSourceForDeath)
    {
        if (ActiveEffects.Count == 0) return;

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
    /// ONLY apply this on the server.
    /// The code is the code of the effect.
    /// </summary>
    public void ApplyEffect(Effect effect)
    {
        effect.SetBehavior(entity, this);

        effect.Initialize();

        if (effect.Type == EffectType.Duration)
        {
            if (ActiveEffects.TryGetValue(effect.Code, out Effect? existingEffect))
            {
                if (existingEffect.MergeEffects(effect))
                {
                    // Effect has not been marged, replace it.
                    existingEffect.OnUnloaded();
                    ActiveEffects[effect.Code] = effect;
                    effect.OnLoaded();
                }
                else
                {
                    // Effect has been merged, reload.
                    existingEffect.OnUnloaded();
                    existingEffect.OnLoaded();
                }
            }
            else
            {
                ActiveEffects[effect.Code] = effect;
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
            existingEffect.OnUnloaded();
            // No duration expiry when removed.
            SaveEffectData();
        }
    }

    /// <summary>
    /// Save all effects as json objects, then serialize the json as bytes.
    /// Effects will be moved to watched attributes -> sent to client -> and loaded.
    /// </summary>
    protected virtual void SaveEffectData()
    {
        Dictionary<string, string> container = [];
        foreach (KeyValuePair<string, Effect> effect in ActiveEffects)
        {
            container.Add(effect.Key, JsonConvert.SerializeObject(effect.Value));
        }
        string json = JsonConvert.SerializeObject(container);
        byte[] bytes = SerializerUtil.Serialize(json);
        entity.WatchedAttributes.SetBytes("activeEffects", bytes);
    }

    /// <summary>
    /// When saved, disposes all active effects and re-initializes them.
    /// </summary>
    protected virtual void LoadEffectData()
    {
        // Load bytes.
        byte[] bytes = entity.WatchedAttributes.GetBytes("activeEffects");
        if (bytes == null) return;

        // Load json.
        string? json = SerializerUtil.Deserialize<string>(bytes);
        if (json == null) return;

        Dictionary<string, string>? container = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (container == null) return;

        // Attempt to deserialize every json entry into a type.
        SortedDictionary<string, Effect> deserialized = [];

        EffectManager effectManager = MainAPI.GetGameSystem<EffectManager>(entity.Api.Side);

        foreach (KeyValuePair<string, string> effect in container)
        {
            if (effectManager.effectTypes.TryGetValue(effect.Key, out Type? type))
            {
                Effect deserializedEffect = (Effect)JsonConvert.DeserializeObject(effect.Value, type)!;
                deserialized.Add(effect.Key, deserializedEffect);
            }
        }

        // Reload and re-initialize all effects.
        ActiveEffects.Values.Foreach(effect => effect.OnUnloaded());
        ActiveEffects = deserialized;
        foreach (Effect effect in ActiveEffects.Values)
        {
            effect.SetBehavior(entity, this);
            effect.OnLoaded();
        }
    }

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

            foreach (Effect effect in ActiveEffects.Values)
            {
                effect.Duration = Math.Clamp(effect.Duration - interval, 0, float.MaxValue);

                if (effect.Duration <= 0)
                {
                    if (entity.Api.Side == EnumAppSide.Server)
                    {
                        effect.OnDurationExpired(); // Only play expiry effects on server to avoid desync.
                        deadEffects.Add(effect.Code);
                    }
                }
                else
                {
                    effect.OnTick(dt);
                }
            }

            if (deadEffects.Count > 0)
            {
                deadEffects.Foreach(effectCode =>
                {
                    ActiveEffects[effectCode].OnUnloaded();
                    ActiveEffects.Remove(effectCode);
                });

                deadEffects.Clear();
                SaveEffectData();
            }
        }
    }
}