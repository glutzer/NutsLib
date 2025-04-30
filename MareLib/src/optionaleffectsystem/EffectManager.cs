using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace MareLib;

/// <summary>
/// Manages effects on entities, like buffs/debuffs or spells that can only affect once.
/// Active effects are held in the entity behavior.
/// </summary>
[GameSystem]
public class EffectManager : NetworkedGameSystem
{
    public readonly Dictionary<string, Type> effectTypes = new();

    public Action<(int previousCount, int currentCount, EntityPlayer player)>? OnPlayerEffectCountChanged;

    public EffectManager(bool isServer, ICoreAPI api) : base(isServer, api, "effectmanager")
    {
    }

    public override void Initialize()
    {
        // Load all effect types on client/server.
        (Type, EffectAttribute)[] effectAttributes = AttributeUtilities.GetAllAnnotatedClasses<EffectAttribute>();
        foreach ((Type type, EffectAttribute _) in effectAttributes)
        {
            effectTypes.Add(type.Name, type);
        }
    }

    /// <summary>
    /// Creates a registered effect, if possible.
    /// </summary>
    public Effect? CreateEffect(string effectName)
    {
        return effectTypes.TryGetValue(effectName, out Type? effectType) ? Activator.CreateInstance(effectType) as Effect : null;
    }

    /// <summary>
    /// Creates a registered effect.
    /// </summary>
    public T? CreateEffect<T>(string effectName) where T : Effect
    {
        return CreateEffect(effectName) as T;
    }

    protected override void RegisterClientMessages(IClientNetworkChannel channel)
    {

    }

    protected override void RegisterServerMessages(IServerNetworkChannel channel)
    {

    }

    public override void OnAssetsLoaded()
    {
        if (effectTypes.Count > 0)
        {
            ReplaceBehaviors();
        }
    }

    /// <summary>
    /// Add effect behavior to everything with health.
    /// </summary>
    public void ReplaceBehaviors()
    {
        JObject nuJObjectPlayer = new()
        {
            ["code"] = "EntityBehaviorPlayerEffects"
        };

        JsonObject effectObjectPlayer = new(nuJObjectPlayer);

        foreach (EntityProperties entityType in api.World.EntityTypes)
        {
            JObject nuJObject = new()
            {
                ["code"] = "EntityBehaviorEffects"
            };

            JsonObject effectObject = new(nuJObject);

            bool player = entityType.Code.FirstCodePart() == "player";

            if (api.Side == EnumAppSide.Server)
            {
                if (entityType.Server.BehaviorsAsJsonObj.FirstOrDefault(x => x.ToString().ToLower().Contains("health")) != null)
                {
                    JsonObject[] newBehaviors = new JsonObject[entityType.Server.BehaviorsAsJsonObj.Length + 1];
                    Array.Copy(entityType.Server.BehaviorsAsJsonObj, 0, newBehaviors, 1, entityType.Server.BehaviorsAsJsonObj.Length);
                    newBehaviors[0] = player ? effectObjectPlayer : effectObject;
                    entityType.Server.BehaviorsAsJsonObj = newBehaviors;

                    entityType.Attributes ??= new JsonObject(new JObject());
                    entityType.Attributes.Token["mareEff"] = new JValue(true);
                }
            }
            else
            {
                if (entityType.Attributes?.KeyExists("mareEff") == true)
                {
                    JsonObject[] newBehaviors = new JsonObject[entityType.Client.BehaviorsAsJsonObj.Length + 1];
                    Array.Copy(entityType.Client.BehaviorsAsJsonObj, 0, newBehaviors, 1, entityType.Client.BehaviorsAsJsonObj.Length);
                    newBehaviors[0] = player ? effectObjectPlayer : effectObject;
                    entityType.Client.BehaviorsAsJsonObj = newBehaviors;
                }
            }
        }
    }
}