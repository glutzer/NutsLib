using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NutsLib;

/// <summary>
/// Class for adding cubes to meshes in different ways.
/// </summary>
public static class CubeMeshUtility
{
    private static readonly MeshVertexData[] centeredCubeVertices = new MeshVertexData[24];
    private static readonly MeshVertexData[] gridAlignedVertices = new MeshVertexData[24];

    private static readonly MeshVertexData[] gridAlignedLineVertices = new MeshVertexData[8];
    public static readonly int[] lineCubeIndices = new int[] { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };

    public static readonly int[] faceIndices = new int[] { 0, 2, 1, 1, 2, 3 };
    public static readonly int[] cubeIndices; // Initialized in static constructor.

    static CubeMeshUtility()
    {
        // North.
        centeredCubeVertices[0] = new MeshVertexData(new Vector3(0.5f, 0.5f, -0.5f), new Vector2(0f, 1f), -Vector3.UnitZ);
        centeredCubeVertices[1] = new MeshVertexData(new Vector3(-0.5f, 0.5f, -0.5f), new Vector2(1f, 1f), -Vector3.UnitZ);
        centeredCubeVertices[2] = new MeshVertexData(new Vector3(0.5f, -0.5f, -0.5f), new Vector2(0f, 0f), -Vector3.UnitZ);
        centeredCubeVertices[3] = new MeshVertexData(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1f, 0f), -Vector3.UnitZ);

        gridAlignedLineVertices[0] = new MeshVertexData(new Vector3(0f, 0f, 0f), new Vector2(0f, 0f), new Vector3(-1f, -1f, -1f).Normalized());
        gridAlignedLineVertices[1] = new MeshVertexData(new Vector3(1f, 0f, 0f), new Vector2(0f, 0f), new Vector3(1f, -1f, -1f).Normalized());
        gridAlignedLineVertices[2] = new MeshVertexData(new Vector3(1f, 0f, 1f), new Vector2(0f, 0f), new Vector3(1f, -1f, 1f).Normalized());
        gridAlignedLineVertices[3] = new MeshVertexData(new Vector3(0f, 0f, 1f), new Vector2(0f, 0f), new Vector3(-1f, -1f, 1f).Normalized());
        gridAlignedLineVertices[4] = new MeshVertexData(new Vector3(0f, 1f, 0f), new Vector2(0f, 0f), new Vector3(-1f, 1f, -1f).Normalized());
        gridAlignedLineVertices[5] = new MeshVertexData(new Vector3(1f, 1f, 0f), new Vector2(0f, 0f), new Vector3(1f, 1f, -1f).Normalized());
        gridAlignedLineVertices[6] = new MeshVertexData(new Vector3(1f, 1f, 1f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f).Normalized());
        gridAlignedLineVertices[7] = new MeshVertexData(new Vector3(0f, 1f, 1f), new Vector2(0f, 0f), new Vector3(-1f, 1f, 1f).Normalized());

        // East, south, west.
        for (int f = 1; f < 4; f++)
        {
            for (int v = 0; v < 4; v++)
            {
                MeshVertexData vertex = centeredCubeVertices[v];
                vertex.RotateY(MathHelper.DegreesToRadians(-90f * f));
                centeredCubeVertices[(f * 4) + v] = vertex;
            }
        }

        // Top.
        for (int v = 0; v < 4; v++)
        {
            MeshVertexData vertex = centeredCubeVertices[v];
            vertex.RotateX(MathHelper.DegreesToRadians(90f));
            centeredCubeVertices[(4 * 4) + v] = vertex;
        }

        // Bottom.
        for (int v = 0; v < 4; v++)
        {
            MeshVertexData vertex = centeredCubeVertices[v];
            vertex.RotateX(MathHelper.DegreesToRadians(-90f));
            centeredCubeVertices[(5 * 4) + v] = vertex;
        }

        // Create grid aligned vertices.
        for (int i = 0; i < 24; i++)
        {
            MeshVertexData centeredVertex = centeredCubeVertices[i];
            centeredVertex.position += new Vector3(0.5f);
            gridAlignedVertices[i] = centeredVertex;
        }

        // Create cube indices.
        cubeIndices = new int[36];
        for (int f = 0; f < 6; f++)
        {
            for (int i = 0; i < 6; i++)
            {
                cubeIndices[(f * 6) + i] = (f * 4) + faceIndices[i];
            }
        }
    }

    /// <summary>
    /// Add a centered cube to a mesh.
    /// </summary>
    public static void AddCenteredCubeData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(cubeIndices);

        for (int i = 0; i < 24; i++)
        {
            meshData.AddVertex(meshDelegate(centeredCubeVertices[i]));
        }
    }

    /// <summary>
    /// Add a grid aligned cube to a mesh.
    /// </summary>
    public static void AddGridAlignedCubeData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(cubeIndices);

        for (int i = 0; i < 24; i++)
        {
            meshData.AddVertex(meshDelegate(gridAlignedVertices[i]));
        }
    }

    /// <summary>
    /// Add cube from 2 points.
    /// </summary>
    public static void AddRangeCubeData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate, Vector3 from, Vector3 to) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(cubeIndices);

        for (int i = 0; i < 24; i++)
        {
            MeshVertexData vertex = gridAlignedVertices[i];

            vertex.position.X = MathHelper.Lerp(from.X, to.X, vertex.position.X);
            vertex.position.Y = MathHelper.Lerp(from.Y, to.Y, vertex.position.Y);
            vertex.position.Z = MathHelper.Lerp(from.Z, to.Z, vertex.position.Z);

            meshData.AddVertex(meshDelegate(vertex));
        }
    }

    public static void AddCenteredFaceData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate, EnumBlockFacing face) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(faceIndices);

        int faceStart = (int)face * 4;

        for (int i = faceStart; i < faceStart + 4; i++)
        {
            meshData.AddVertex(meshDelegate(centeredCubeVertices[i]));
        }
    }

    public static void AddGridAlignedFaceData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate, EnumBlockFacing face) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(faceIndices);

        int faceStart = (int)face * 4;

        for (int i = faceStart; i < faceStart + 4; i++)
        {
            meshData.AddVertex(meshDelegate(gridAlignedVertices[i]));
        }
    }

    public static void AddRangeFaceData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate, Vector3 from, Vector3 to, EnumBlockFacing face) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(faceIndices);

        int faceStart = (int)face * 4;

        for (int i = faceStart; i < faceStart + 4; i++)
        {
            MeshVertexData vertex = gridAlignedVertices[i];
            vertex.position.X = MathHelper.Lerp(from.X, to.X, vertex.position.X);
            vertex.position.Y = MathHelper.Lerp(from.Y, to.Y, vertex.position.Y);
            vertex.position.Z = MathHelper.Lerp(from.Z, to.Z, vertex.position.Z);
            meshData.AddVertex(meshDelegate(vertex));
        }
    }

    public static void AddRangeFaceData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate, Vector3 from, Vector3 to, EnumFaceFlags faceFlags) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(faceIndices);

        for (int i = 0; i < 6; i++)
        {
            if ((faceFlags & (EnumFaceFlags)(1 << i)) == 0)
                continue;
            int faceStart = i * 4;
            for (int j = faceStart; j < faceStart + 4; j++)
            {
                MeshVertexData vertex = gridAlignedVertices[j];
                vertex.position.X = MathHelper.Lerp(from.X, to.X, vertex.position.X);
                vertex.position.Y = MathHelper.Lerp(from.Y, to.Y, vertex.position.Y);
                vertex.position.Z = MathHelper.Lerp(from.Z, to.Z, vertex.position.Z);
                meshData.AddVertex(meshDelegate(vertex));
            }
        }
    }

    /// <summary>
    /// Create and upload a centered cube.
    /// </summary>
    public static MeshHandle CreateCenteredCubeMesh<T>(MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        MeshInfo<T> meshData = new(24, 36);
        AddCenteredCubeData(meshData, meshDelegate);
        return RenderTools.UploadMesh(meshData);
    }

    /// <summary>
    /// Create and upload a grid aligned cube.
    /// </summary>
    public static MeshHandle CreateGridAlignedCubeMesh<T>(MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        MeshInfo<T> meshData = new(24, 36);
        AddGridAlignedCubeData(meshData, meshDelegate);
        return RenderTools.UploadMesh(meshData);
    }

    /// <summary>
    /// Create and upload a cube from 2 points.
    /// </summary>
    public static MeshHandle CreateRangeCubeMesh<T>(MeshDelegate<T> meshDelegate, Vector3 from, Vector3 to) where T : unmanaged
    {
        MeshInfo<T> meshData = new(24, 36);
        AddRangeCubeData(meshData, meshDelegate, from, to);
        return RenderTools.UploadMesh(meshData);
    }

    /// <summary>
    /// Add a wireframe cube to a mesh using lines.
    /// </summary>
    public static void AddWireframeCubeData<T>(MeshInfo<T> meshInfo, MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        meshInfo.AddIndicesFromLastVertex(lineCubeIndices);

        for (int i = 0; i < 8; i++)
        {
            meshInfo.AddVertex(meshDelegate(gridAlignedLineVertices[i]));
        }
    }

    /// <summary>
    /// Create and upload a wireframe cube, with line draw type.
    /// </summary>
    public static MeshHandle CreateWireframeCubeMesh<T>(MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        MeshInfo<T> meshInfo = new(8, 24);
        meshInfo.SetDrawMode(PrimitiveType.Lines);
        AddWireframeCubeData(meshInfo, meshDelegate);
        return meshInfo.Upload();
    }
}