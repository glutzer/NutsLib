using MareLib;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Equimancy;

[Spell]
public class ArcLightning : Spell
{
    private int chainsLeft = 10;
    private readonly List<Entity> chainedToEntities = new();
    private int ticks;
    private Vector3 color = new(1, 0, 0.5f);

    public ArcLightning(SpellManager spellManager, long instanceId, Vector3d position, string code, SpellConfig? config) : base(spellManager, instanceId, position, code, config)
    {
    }

    public override void Initialize()
    {
        if (castedBy is not EntityPlayer player)
        {
            Kill();
            return;
        }

        chainedToEntities.Add(castedBy);

        if (isServer)
        {
            EntitySelection? sel = player.Player.CurrentEntitySelection;
            BlockSelection? blockSel = player.Player.CurrentBlockSelection;

            if (sel == null)
            {
                if (blockSel == null)
                {
                    Kill();
                    return;
                }

                Entity[] nearEntities = spellManager.api.World.GetEntitiesAround(blockSel.Position.ToVec3d(), 10, 10);

                // Get closest entity.
                Entity? closestEntity = nearEntities
                    .Where(e => !chainedToEntities.Contains(e))
                    .OrderBy(e => e.Pos.DistanceTo(castedBy.Pos))
                    .FirstOrDefault();

                if (closestEntity == null)
                {
                    Kill();
                    return;
                }

                ChainToEntity(closestEntity);
                return;
            }

            ChainToEntity(sel.Entity);
        }
    }

    public override void OnTick()
    {
        if (!isServer) return;

        ticks++;

        while (ticks > 1)
        {
            ticks -= 1;

            Entity lastEntity = chainedToEntities.Last();

            Entity[] nearEntities = spellManager.api.World.GetEntitiesAround(lastEntity.Pos.XYZ, 20, 20);

            // Get closest entity.
            //Entity? closestEntity = nearEntities
            //    .Where(e => !chainedToEntities.Contains(e))
            //    .OrderBy(e => e.Pos.DistanceTo(lastEntity.Pos))
            //    .FirstOrDefault();

            Entity[] closestEntities = nearEntities
                .Where(e => !chainedToEntities.Contains(e))
                .ToArray();

            if (closestEntities.Length == 0)
            {
                Kill();
            }
            else
            {
                Entity closestEntity = closestEntities[MainAPI.Capi.World.Rand.Next(closestEntities.Length)];
                ChainToEntity(closestEntity);
            }

            chainsLeft--;
            if (chainsLeft == 0) Kill();
        }
    }

    /// <summary>
    /// Damage an entity, send a chain packet to the client.
    /// </summary>
    public void ChainToEntity(Entity entity)
    {
        chainedToEntities.Add(entity);
        entity.ReceiveDamage(new DamageSource()
        {
            Source = EnumDamageSource.Player,
            Type = EnumDamageType.Electricity,
            SourceEntity = castedBy,
            KnockbackStrength = 1.2f,
            YDirKnockbackDiv = 0.5f
        }, 5);

        if (!entity.Alive)
        {
            //MainAPI.Sapi.ModLoader.GetModSystem<WeatherSystemServer>().SpawnLightningFlash(entity.Pos.XYZ);
            //entity.Die(EnumDespawnReason.Expire);
        }

        SendPacket(0, entity.EntityId);
    }

    public override void HandlePacket(SpellPacket packet)
    {
        // Spawn an fx when chaining.
        if (packet.packetId == 0)
        {
            long entityId = packet.GetData<long>();
            Entity entity = spellManager.api.World.GetEntityById(entityId) ?? castedBy!;
            Entity lastChained = chainedToEntities.Last();

            FXManager.GetFX<FXArcLightning>().SpawnInstance(new ArcLightningInstance(() =>
            {
                Vector3d vec = lastChained.Pos.ToVector();

                if (lastChained is EntityPlayer player)
                {
                    vec += GetLocalHornPos(player);
                }
                else
                {
                    vec.Y += 0.5f;
                }

                return vec;
            }, () =>
            {
                Vector3d vec = entity.Pos.ToVector();
                vec.Y += entity.CollisionBox.Y2 / 2;
                return vec;
            }, color));

            MainAPI.Capi.World.PlaySoundAt(new AssetLocation("equimancy:sounds/spark"), entity);

            chainedToEntities.Add(entity);
        }
    }
}