using MareLib;
using OpenTK.Mathematics;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Equimancy;

/// <summary>
/// Pipe block that may carry fluid or gasses.
/// </summary>
[BlockEntity]
public class BlockEntityPipe : BlockEntity
{
    // Placeholder constants.
    public static readonly float PipeRadius = 0.25f;

    // If -1, no group is assigned.
    // Should be set in initialize.
    public int groupId = -1;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);

        // Pipe begins existing.
        PipeSystem pipeSystem = MainAPI.GetGameSystem<PipeSystem>(Api.Side);
        pipeSystem.OnPipeAdded(this);
    }

    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        OnPipeNoLongerExisting();
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        OnPipeNoLongerExisting();
    }

    /// <summary>
    /// Unregister from the pipe system here.
    /// </summary>
    public void OnPipeNoLongerExisting()
    {
        if (MainAPI.TryGetGameSystem(Api.Side, out PipeSystem? pipeSystem))
        {
            pipeSystem.OnPipeRemoved(this);
        }
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
    {
        int variant = (Pos.GetHashCode() % 4) + 1;

        // This will be the pipe texture, once it's made.
        TextureAtlasPosition texPos = MainAPI.Capi.BlockTextureAtlas[new AssetLocation($"block/metal/sheet/cupronickel{variant}")];

        MeshInfo<StandardVertex> meshInfo = new(40, 60);

        BlockFaces.IterateBlocksAtFaces(Pos, Api.World.BlockAccessor, (face, block, newPos) =>
        {
            // Tessellate connection.
            if (block is IPipeConnectable connectable && connectable.CanConnectTo(Pos))
            {
                GridPos faceOff = BlockFaces.GetFaceOffset(face);
                Vector3 faceOffset = new(faceOff.X, faceOff.Y, faceOff.Z);

                int sign = Math.Sign(faceOffset.X + faceOffset.Y + faceOffset.Z);
                Vector3 widthOffset = new Vector3(sign) - faceOffset;

                Vector3 start = new Vector3(0.5f) + (faceOffset * PipeRadius) - (widthOffset * PipeRadius);
                Vector3 end = new Vector3(0.5f) + (faceOffset * 0.5f) + (widthOffset * PipeRadius);

                BlockFaces.ForEachPerpendicularFace(face, perpendicularFace =>
                {
                    CubeMeshUtility.AddRangeFaceData(meshInfo, vertex =>
                    {
                        vertex = SetUvsBasedOnPosition(vertex);

                        return new StandardVertex(vertex.position, vertex.uv, vertex.normal, Vector4.One);
                    }, Vector3.ComponentMin(start, end), Vector3.ComponentMax(start, end), perpendicularFace);
                });

                // Actual side.
                CubeMeshUtility.AddRangeFaceData(meshInfo, vertex =>
                {
                    vertex = SetUvsBasedOnPosition(vertex);

                    return new StandardVertex(vertex.position, vertex.uv, vertex.normal, Vector4.One);
                }, Vector3.ComponentMin(start, end), Vector3.ComponentMax(start, end), face);

                return;
            }

            // Tessellate center.

            // Needs to be tessellated to window if open.

            CubeMeshUtility.AddRangeFaceData(meshInfo, vertex =>
            {
                // Create uv based on position of the pipe.
                vertex = SetUvsBasedOnPosition(vertex);

                return new StandardVertex(vertex.position, vertex.uv, vertex.normal, Vector4.One);
            }, new Vector3(0.5f - PipeRadius), new Vector3(0.5f + PipeRadius), face);
        });

        // Map uvs to the supplied position.
        TessellatorTools.MapUvToAtlasTexture(meshInfo, texPos);

        // 3 = transparent render pass? 0 = regular?
        MeshData meshData = TessellatorTools.ConvertToMeshData(meshInfo, texPos.atlasTextureId, Vector4.One, 0, ColorSpace.GBRA);

        mesher.AddMeshData(meshData);

        return true;
    }

    /// <summary>
    /// Set the uvs based on the inner block position.
    /// </summary>
    public static MeshVertexData SetUvsBasedOnPosition(MeshVertexData vertex)
    {
        int sign = vertex.normal.X + vertex.normal.Y + vertex.normal.Z > 0 ? 1 : -1;

        if (vertex.normal.X != 0)
        {
            vertex.uv.X = vertex.position.Z;
            if (sign == -1)
            {
                vertex.uv.X = 1 - vertex.uv.X;
            }

            vertex.uv.Y = 1 - vertex.position.Y;
        }
        else if (vertex.normal.Y != 0)
        {
            vertex.uv.X = vertex.position.X;

            vertex.uv.Y = vertex.position.Z;
            if (sign == -1)
            {
                vertex.uv.Y = 1 - vertex.uv.Y;
            }
        }
        else if (vertex.normal.Z != 0)
        {
            vertex.uv.X = vertex.position.X;
            if (sign == -1)
            {
                vertex.uv.X = 1 - vertex.uv.X;
            }

            vertex.uv.Y = 1 - vertex.position.Y;
        }

        return vertex;
    }
}