using OpenTK.Mathematics;

namespace NutsLib;

/// <summary>
/// Quad mesh generators.
/// Faces forwards on z: towards camera or south.
/// </summary>
public static class QuadMeshUtility
{
    private static readonly MeshVertexData[] guiVertices = new MeshVertexData[4];
    private static readonly MeshVertexData[] vertices = new MeshVertexData[4];

    public static readonly int[] quadIndices = [0, 2, 1, 1, 2, 3];

    static QuadMeshUtility()
    {
        guiVertices[0] = new MeshVertexData(new Vector3(0f, 1f, 0f), new Vector2(0f, 1f), Vector3.UnitZ);
        guiVertices[1] = new MeshVertexData(new Vector3(1f, 1f, 0f), new Vector2(1f, 1f), Vector3.UnitZ);
        guiVertices[2] = new MeshVertexData(new Vector3(0f, 0f, 0f), new Vector2(0f, 0f), Vector3.UnitZ);
        guiVertices[3] = new MeshVertexData(new Vector3(1f, 0f, 0f), new Vector2(1f, 0f), Vector3.UnitZ);

        vertices[0] = new MeshVertexData(new Vector3(-0.5f, 0.5f, 0f), new Vector2(0f, 1f), Vector3.UnitZ);
        vertices[1] = new MeshVertexData(new Vector3(0.5f, 0.5f, 0f), new Vector2(1f, 1f), Vector3.UnitZ);
        vertices[2] = new MeshVertexData(new Vector3(-0.5f, -0.5f, 0f), new Vector2(0f, 0f), Vector3.UnitZ);
        vertices[3] = new MeshVertexData(new Vector3(0.5f, -0.5f, 0f), new Vector2(1f, 0f), Vector3.UnitZ);
    }

    /// <summary>
    /// Adds a quad suitable for gui rendering.
    /// </summary>
    public static void AddGuiQuadData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(quadIndices);

        for (int i = 0; i < 4; i++)
        {
            meshData.AddVertex(meshDelegate(guiVertices[i]));
        }
    }

    /// <summary>
    /// Adds a quad from -0.5 to 0.5.
    /// </summary>
    public static void AddCenteredQuadData<T>(MeshInfo<T> meshData, MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        meshData.AddIndicesFromLastVertex(quadIndices);

        for (int i = 0; i < 4; i++)
        {
            meshData.AddVertex(meshDelegate(vertices[i]));
        }
    }

    /// <summary>
    /// Creates a quad suitable for gui rendering.
    /// </summary>
    public static MeshHandle CreateGuiQuadMesh<T>(MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        MeshInfo<T> meshData = new(4, 6);
        AddGuiQuadData(meshData, meshDelegate);
        return RenderTools.UploadMesh(meshData);
    }

    /// <summary>
    /// Creates a quad from -0.5 to 0.5.
    /// </summary>
    public static MeshHandle CreateCenteredQuadMesh<T>(MeshDelegate<T> meshDelegate) where T : unmanaged
    {
        MeshInfo<T> meshData = new(4, 6);
        AddCenteredQuadData(meshData, meshDelegate);
        return RenderTools.UploadMesh(meshData);
    }
}