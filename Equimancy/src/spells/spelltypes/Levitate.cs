using MareLib;
using OpenTK.Mathematics;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.ServerMods;

namespace Equimancy;

[Spell]
public class Levitate : Spell, IPhysicsTickable
{
    public Entity levitatedEntity = null!;
    public double selectionDistance;
    public LevitateInstance? fx;

    public Levitate(SpellManager spellManager, long instanceId, Vector3d position, string code, SpellConfig? config) : base(spellManager, instanceId, position, code, config)
    {
    }

    public override void Initialize()
    {
        if (castedBy is not EntityPlayer player)
        {
            Kill();
            return;
        }

        if (isServer)
        {
            EntitySelection selection = player.Player.CurrentEntitySelection;
            if (selection == null)
            {
                Kill();
                return;
            }

            levitatedEntity = selection.Entity;
            SendPacket(0, levitatedEntity.EntityId);

            selectionDistance = player.Pos.DistanceTo(selection.Entity.Pos);

            MainAPI.Sapi.Server.AddPhysicsTickable(this);
        }
    }

    public override void OnTick()
    {

    }

    public override void HandlePacket(SpellPacket packet)
    {
        if (packet.packetId == 0)
        {
            Entity? entity = spellManager.api.World.GetEntityById(packet.GetData<long>());

            if (entity == null)
            {
                Kill();
                return;
            }

            fx = new LevitateInstance()
            {
                color = new Vector4(1, 0, 0.5f, 0.5f),
                entity = entity
            };

            FXManager.GetFX<FXLevitate>().SpawnInstance(fx);

            levitatedEntity = entity;
        }
    }

    public override void OnNoLongerTrackingPlayer(TrackedPlayer player)
    {
        if (player.player.Entity == castedBy)
        {
            Kill();
        }
    }

    public volatile int flag;
    public ref int FlagTickDone => ref flag;
    public static bool Ticking => true;
    bool IPhysicsTickable.Ticking { get; set; } = true;

    public void OnPhysicsTick(float dt)
    {
        TrackedPlayer? player = spellManager.TryGetTrackedPlayerFromCaster(castedBy);
        if (player == null || player.currentlyCasting == false || player.lastCastTime > castedAtTime)
        {
            Kill();
            return;
        }

        EntityPlayer playerEntity = player.player.Entity;

        float headPitch = -playerEntity.ServerPos.HeadPitch;
        float headYaw = playerEntity.Pos.Yaw;

        Vector3 headVector = new(MathF.Sin(headYaw) * MathF.Cos(headPitch), MathF.Sin(headPitch), MathF.Cos(headYaw) * MathF.Cos(headPitch));

        Vector3d targetPos = new(playerEntity.Pos.X + (headVector.X * selectionDistance), playerEntity.Pos.Y + 1 + (headVector.Y * selectionDistance), playerEntity.Pos.Z + (headVector.Z * selectionDistance));

        Vector3d vectorToTarget = targetPos - levitatedEntity.Pos.ToVector();

        if (vectorToTarget.Length > 2f)
        {
            Kill();
            return;
        }

        const float MAX_LENGTH_MULTIPLIER = 1f;

        Vector3d pushVector = Vector3d.ComponentMin(new Vector3d(MAX_LENGTH_MULTIPLIER), vectorToTarget);

        pushVector *= 0.01f;

        levitatedEntity.ServerPos.Motion.Add(pushVector.X, pushVector.Y * 2, pushVector.Z);
    }

    public override void OnRemoved()
    {
        if (isServer)
        {
            MainAPI.Sapi.Server.RemovePhysicsTickable(this);
        }
        else
        {
            if (fx != null) fx.alive = false;
        }
    }

    public bool CanProceedOnThisThread()
    {
        return true;
    }

    public void OnPhysicsTickDone()
    {

    }

    public void AfterPhysicsTick(float dt)
    {
        GenRockStrataNew
    }
}