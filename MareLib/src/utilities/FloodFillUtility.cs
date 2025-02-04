using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace MareLib;

public static class FloodFillUtility
{
    /// <summary>
    /// Flood fills blocks of the same type.
    /// Returns visited locations.
    /// </summary>
    public static HashSet<GridPos> FloodFillBlocks(BlockPos pos, Action<GridPos> forEachUniqueBlock, System.Func<Block, bool> isBlockValid, ICoreAPI api)
    {
        HashSet<GridPos> visited = new();
        Queue<GridPos> queue = new();
        queue.Enqueue(new GridPos(pos));

        BlockPos tempPos = pos.Copy();

        while (queue.Count > 0)
        {
            GridPos current = queue.Dequeue();

            if (visited.Contains(current)) continue;
            visited.Add(current);

            forEachUniqueBlock(current);

            tempPos.Set(current.X, current.Y, current.Z);
            BlockFaces.IterateBlocksAtFaces(tempPos, api.World.BlockAccessor, (face, block, blockPos) =>
            {
                if (isBlockValid(block))
                {
                    queue.Enqueue(new GridPos(blockPos));
                }
            });
        }

        return visited;
    }

    /// <summary>
    /// Tessellates a mesh of every location as a cube.
    /// Good for x-ray or visualizing paths.
    /// Uses a standard vertex.
    /// Skips inside faces.
    /// </summary>
    public static void CreateFloodFillMesh(HashSet<GridPos> positions, GridPos basePos, Vector4 color, out MeshHandle handle)
    {
        MeshInfo<StandardVertex> meshInfo = new(40, 60);

        foreach (GridPos pos in positions)
        {
            GridPos relativePos = pos - basePos;

            Vector3 relativeVec = new(relativePos.X, relativePos.Y, relativePos.Z);

            BlockFaces.ForEachFace(face =>
            {
                // Occluded at this point.
                if (positions.Contains(pos.OffsetByFace(face))) return;

                CubeMeshUtility.AddGridAlignedFaceData(meshInfo, vertex =>
                {
                    return new StandardVertex(vertex.position + relativeVec, vertex.uv, vertex.normal, color);
                }, face);
            });
        }

        handle = RenderTools.UploadMesh(meshInfo);
    }
}