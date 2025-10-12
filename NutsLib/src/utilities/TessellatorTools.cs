﻿using OpenTK.Mathematics;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace NutsLib;

public enum ColorSpace
{
    GBRA,
    BGRA,
    RGBA,
    ARGB
}

public static class TessellatorTools
{
    public delegate ref Vector2 RefDelegate<T>(ref T structData);

    /// <summary>
    /// Maps the texture atlas to a mesh.
    /// </summary>
    public static void MapUvToAtlasTexture(MeshInfo<StandardVertex> meshInfo, TextureAtlasPosition textureAtlasPosition)
    {
        for (int i = 0; i < meshInfo.vertices.Length; i++)
        {
            StandardVertex vertex = meshInfo.vertices[i];

            vertex.uv.X = textureAtlasPosition.x1 + (vertex.uv.X * (textureAtlasPosition.x2 - textureAtlasPosition.x1));
            vertex.uv.Y = textureAtlasPosition.y1 + (vertex.uv.Y * (textureAtlasPosition.y2 - textureAtlasPosition.y1));

            meshInfo.vertices[i] = vertex;
        }
    }

    /// <summary>
    /// Converts to mesh data for chunks?
    /// </summary>
    public static MeshData ConvertToMeshData(MeshInfo<StandardVertex> meshInfo, int atlasTextureId, Vector4 vectorColor, short renderPassId, ColorSpace colorSpace, float glow = 0)
    {
        MeshData meshData = new MeshData(meshInfo.vertexAmount, meshInfo.indexAmount).WithColorMaps().WithRenderpasses();

        int color = colorSpace switch
        {
            ColorSpace.GBRA => ColorUtil.ColorFromRgba((int)(vectorColor.Y * 255), (int)(vectorColor.Z * 255), (int)(vectorColor.X * 255), (int)(vectorColor.W * 255)),
            ColorSpace.BGRA => ColorUtil.ColorFromRgba((int)(vectorColor.Z * 255), (int)(vectorColor.Y * 255), (int)(vectorColor.X * 255), (int)(vectorColor.W * 255)),
            ColorSpace.RGBA => ColorUtil.ColorFromRgba((int)(vectorColor.X * 255), (int)(vectorColor.Y * 255), (int)(vectorColor.Z * 255), (int)(vectorColor.W * 255)),
            ColorSpace.ARGB => ColorUtil.ColorFromRgba((int)(vectorColor.W * 255), (int)(vectorColor.X * 255), (int)(vectorColor.Y * 255), (int)(vectorColor.Z * 255)),
            _ => ColorUtil.ColorFromRgba(255, 255, 255, 255),
        };

        for (int i = 0; i < meshInfo.vertexAmount; i++)
        {
            byte face = 0;

            StandardVertex vertex = meshInfo.vertices[i];

            if (vertex.normal.X != 0)
            {
                face = vertex.normal.X > 0 ? (byte)2 : (byte)4;
            }
            else if (vertex.normal.Y != 0)
            {
                face = vertex.normal.Y > 0 ? (byte)5 : (byte)6;
            }
            else if (vertex.normal.Z != 0)
            {
                face = vertex.normal.Z > 0 ? (byte)3 : (byte)1;
            }

            // Once every 4 vertices, and also the first one.
            if (i % 4 == 0)
            {
                meshData.AddTextureId(atlasTextureId);
                meshData.AddRenderPass(renderPassId);
                meshData.AddXyzFace(face);
            }

            int flags = BlockFacing.ALLFACES[face - 1].NormalPackedFlags;

            if (glow > 0)
            {
                flags += (int)(glow * 255); // Bits 0-7 for glow.
            }

            meshData.AddWithFlagsVertex(vertex.position.X, vertex.position.Y, vertex.position.Z, vertex.uv.X, vertex.uv.Y, color, flags);
        }

        // Add all indices.
        for (int i = 0; i < meshInfo.indexAmount; i++)
        {
            meshData.AddIndex(meshInfo.indices[i]);
        }

        return meshData;
    }

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