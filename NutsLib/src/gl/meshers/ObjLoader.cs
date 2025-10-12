using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace NutsLib;

public static class ObjLoader
{
    /// <summary>
    /// Takes asset path including /objs and file name.
    /// </summary>
    public static MeshInfo<T> LoadObj<T>(string assetPath, MeshDelegate<T> dele) where T : unmanaged
    {
        List<Vector3> vertices = [];
        List<Vector3> normals = [];
        List<Vector2> uvs = [];
        List<Vector3i> trianglePart = [];

        IAsset objAsset = MainAPI.Capi.Assets.TryGet(assetPath);
        string objString = objAsset.ToText();

        string[] lines = objString.Split("\n");

        try
        {
            foreach (string line in lines)
            {
                string[] split = line.Split(' ');
                if (split[0] == "v")
                {
                    vertices.Add(new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3])));
                }
                else if (split[0] == "vn")
                {
                    normals.Add(new Vector3(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3])));
                }
                else if (split[0] == "vt")
                {
                    uvs.Add(new Vector2(float.Parse(split[1]), float.Parse(split[2])));
                }
                else if (split[0] == "f")
                {
                    for (int i = 1; i < 4; i++)
                    {
                        string[] faceData = split[i].Split("/");
                        int vertIndex = int.Parse(faceData[0]) - 1;
                        int uvIndex = int.Parse(faceData[1]) - 1;
                        int normIndex = int.Parse(faceData[2]) - 1;
                        trianglePart.Add(new Vector3i(vertIndex, uvIndex, normIndex));
                    }
                }
            }
        }
        catch
        {
            Console.WriteLine($"Error loading obj {assetPath}.");
        }

        MeshInfo<T> meshInfo = new(6, 6);
        Dictionary<Vector3i, int> indices = [];

        for (int i = 0; i < trianglePart.Count; i++)
        {
            if (!indices.TryGetValue(trianglePart[i], out int index))
            {
                meshInfo.AddVertex(dele(new MeshVertexData(vertices[trianglePart[i].X], uvs[trianglePart[i].Y], normals[trianglePart[i].Z])));
                index = indices.Count;
                indices[trianglePart[i]] = index;
            }

            meshInfo.AddIndex(index);
        }

        return meshInfo;
    }
}