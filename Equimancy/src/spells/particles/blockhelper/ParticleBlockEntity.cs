using MareLib;
using Newtonsoft.Json;
using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Equimancy;

/// <summary>
/// Tool to help create particles.
/// </summary>
[BlockEntity]
public class ParticleBlockEntity : BlockEntity
{
    public StandardParticleSystem? particleSystem;
    public ParticleConfig currentConfig = new();
    public ParticleConfigGui? gui;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);

        if (api.Side == EnumAppSide.Client)
        {
            particleSystem = new StandardParticleSystem(currentConfig, new Vector3d(Pos.X + 0.5f, Pos.Y + 1.5f, Pos.Z + 0.5f));
            particleSystem.RegisterEmitter();
        }
    }

    public void UpdateConfigFromClient()
    {
        string jsonConfig = JsonConvert.SerializeObject(currentConfig, new JsonSerializerSettings
        {
            ContractResolver = new IgnorePropertiesResolver()
        });

        byte[] data = SerializerUtil.Serialize(jsonConfig);

        MainAPI.Client.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 69, data);
    }

    public override void OnReceivedClientPacket(IPlayer fromPlayer, int packetId, byte[] data)
    {
        base.OnReceivedClientPacket(fromPlayer, packetId, data);

        if (packetId == 69)
        {
            string? configData = SerializerUtil.Deserialize<string>(data);
            if (configData == null) return;

            ParticleConfig? config = JsonConvert.DeserializeObject<ParticleConfig>(configData);
            if (config == null) return;

            currentConfig = config;

            // Update config on clients.
            MarkDirty();
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);

        string jsonConfig = JsonConvert.SerializeObject(currentConfig, new JsonSerializerSettings
        {
            ContractResolver = new IgnorePropertiesResolver()
        });

        tree.SetString("particleConfig", jsonConfig);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);

        string? jsonConfig = tree.GetString("particleConfig");
        if (jsonConfig == null) return;

        ParticleConfig? config = JsonConvert.DeserializeObject<ParticleConfig>(jsonConfig);
        if (config == null) return;

        currentConfig = config;

        // Update particles on client.
        if (worldAccessForResolve.Side == EnumAppSide.Server) return;

        if (particleSystem != null)
        {
            particleSystem.Dispose();
            particleSystem = null;
        }

        if (Api != null)
        {
            particleSystem = new StandardParticleSystem(currentConfig, new Vector3d(Pos.X + 0.5f, Pos.Y + 1.5f, Pos.Z + 0.5f));
            particleSystem.RegisterEmitter();
        }

        if (gui != null)
        {
            gui.currentConfig = currentConfig;
        }
    }

    public override void OnBlockRemoved()
    {
        particleSystem?.Dispose();
        particleSystem = null;

        base.OnBlockRemoved();
    }

    public override void OnBlockBroken(IPlayer? byPlayer = null)
    {
        particleSystem?.Dispose();
        particleSystem = null;

        base.OnBlockBroken(byPlayer);
    }
}