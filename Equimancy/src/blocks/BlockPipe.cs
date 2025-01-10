using MareLib;
using OpenTK.Mathematics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Equimancy;

[Block]
public class BlockPipe : Block, IPipeConnectable
{
    public bool CanConnectTo(BlockPos pos)
    {
        return true;
    }

    public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex = -1)
    {
        TextureAtlasPosition texPos = MainAPI.Capi.BlockTextureAtlas[new AssetLocation($"block/metal/sheet/cupronickel1")];
        return capi.BlockTextureAtlas.GetRandomColor(texPos, rndIndex);
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        renderinfo.ModelRef = ObjectCacheUtil.GetOrCreate(capi, "pipemesh", () =>
        {
            TextureAtlasPosition texPos = MainAPI.Capi.BlockTextureAtlas[new AssetLocation($"block/metal/sheet/cupronickel1")];
            MeshInfo<StandardVertex> meshInfo = new(40, 60);

            CubeMeshUtility.AddRangeCubeData(meshInfo, vertex =>
            {
                // Create uv based on position of the pipe.
                vertex = BlockEntityPipe.SetUvsBasedOnPosition(vertex);

                return new StandardVertex(vertex.position, vertex.uv, vertex.normal, Vector4.One);
            }, new Vector3(0.5f - BlockEntityPipe.PipeRadius), new Vector3(0.5f + BlockEntityPipe.PipeRadius));

            TessellatorTools.MapUvToAtlasTexture(meshInfo, texPos);

            MeshData meshData = TessellatorTools.ConvertToMeshData(meshInfo, texPos.atlasTextureId, Vector4.One, 1, ColorSpace.RGBA);

            return capi.Render.UploadMultiTextureMesh(meshData);
        });
    }
}