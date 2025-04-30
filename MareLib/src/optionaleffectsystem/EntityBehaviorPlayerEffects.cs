using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace MareLib;

[EntityBehavior]
public class EntityBehaviorPlayerEffects : EntityBehaviorEffects
{
    public EntityBehaviorPlayerEffects(Entity entity) : base(entity)
    {
    }

    protected override void LoadEffectData()
    {
        int effectCount = ActiveEffects.Values.Count;

        base.LoadEffectData();

        int newCount = ActiveEffects.Values.Count;

        if (newCount != effectCount)
        {
            MainAPI.GetGameSystem<EffectManager>(entity.Api.Side).OnPlayerEffectCountChanged?.Invoke((effectCount, newCount, (EntityPlayer)entity));
        }
    }
}