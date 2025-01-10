using OpenTK.Mathematics;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace MareLib;

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
    public static MeshData ConvertToMeshData(MeshInfo<StandardVertex> meshInfo, int atlasTextureId, Vector4 vectorColor, short renderPassId, ColorSpace colorSpace)
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
                if (vertex.normal.X > 0)
                {
                    face = 2;
                }
                else
                {
                    face = 4;
                }
            }
            else if (vertex.normal.Y != 0)
            {
                if (vertex.normal.Y > 0)
                {
                    face = 5;
                }
                else
                {
                    face = 6;
                }
            }
            else if (vertex.normal.Z != 0)
            {
                if (vertex.normal.Z > 0)
                {
                    face = 3;
                }
                else
                {
                    face = 1;
                }
            }

            // Once every 4 vertices, and also the first one.
            if (i % 4 == 0)
            {
                meshData.AddTextureId(atlasTextureId);
                meshData.AddRenderPass(renderPassId);
                meshData.AddXyzFace(face);
            }

            meshData.AddWithFlagsVertex(vertex.position.X, vertex.position.Y, vertex.position.Z, vertex.uv.X, vertex.uv.Y, color, BlockFacing.ALLFACES[face - 1].NormalPackedFlags);
        }

        // Add all indices.
        for (int i = 0; i < meshInfo.indexAmount; i++)
        {
            meshData.AddIndex(meshInfo.indices[i]);
        }

        return meshData;
    }
}