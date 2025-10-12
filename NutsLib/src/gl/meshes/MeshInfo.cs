using OpenTK.Graphics.OpenGL4;
using System;

namespace NutsLib;

public interface IMeshInfo
{
    /// <summary>
    /// Uploads the mesh data through the RenderEngine.
    /// </summary>
    MeshHandle Upload();

    /// <summary>
    /// Casts this interface to a mesh data object of struct T.
    /// </summary>
    MeshInfo<T> Get<T>() where T : unmanaged;
}

/// <summary>
/// Turns a struct into a mesh data object. Order determines order used in the shader.
/// </summary>
public class MeshInfo<T> : IMeshInfo where T : unmanaged
{
    public T[] vertices;
    public int vertexAmount = 0;

    public int[] indices;
    public int indexAmount = 0;

    public int vertexArraySize;
    public int indexArraySize;

    public PrimitiveType drawMode = PrimitiveType.Triangles;
    public BufferUsageHint usageType = BufferUsageHint.StaticDraw;

    public MeshInfo(int vertexArraySize, int indexArraySize)
    {
        this.vertexArraySize = vertexArraySize;
        this.indexArraySize = indexArraySize;
        vertices = new T[vertexArraySize];
        indices = new int[indexArraySize];
    }

    /// <summary>
    /// Sets draw mode used in the vertex shader.
    /// </summary>
    public MeshInfo<T> SetDrawMode(PrimitiveType drawMode)
    {
        this.drawMode = drawMode;
        return this;
    }

    /// <summary>
    /// Hint for how the buffer will be used for the driver.
    /// Static - Mesh updated once, or very rarely.
    /// Dynamic - Mesh updated often.
    /// Stream - Mesh updated every time it is rendered.
    /// </summary>
    public MeshInfo<T> SetUsage(BufferUsageHint usageType)
    {
        this.usageType = usageType;
        return this;
    }

    public void AddVertex(T vertex)
    {
        if (vertexAmount == vertexArraySize) ResizeVertexArray();

        vertices[vertexAmount] = vertex;
        vertexAmount++;
    }

    public void AddVertices(T[] vertices)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertexAmount == vertexArraySize) ResizeVertexArray();

            this.vertices[vertexAmount] = vertices[i];
            vertexAmount++;
        }
    }

    public void SetVertices(T[] vertices)
    {
        this.vertices = vertices;
        vertexAmount = vertices.Length;
    }

    public void AddIndex(int index)
    {
        if (indexAmount == indexArraySize) ResizeIndexArray();

        indices[indexAmount] = index;
        indexAmount++;
    }

    public void AddIndices(int[] indices)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            if (indexAmount == indexArraySize) ResizeIndexArray();

            this.indices[indexAmount] = indices[i];
            indexAmount++;
        }
    }

    /// <summary>
    /// Offset used when adding indices from another mesh.
    /// </summary>
    public void AddOffsetIndices(int[] indices, int offset)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            if (indexAmount == indexArraySize) ResizeIndexArray();

            this.indices[indexAmount] = indices[i] + offset;
            indexAmount++;
        }
    }

    /// <summary>
    /// Adds indices from the last added vertex position.
    /// </summary>
    public void AddIndicesFromLastVertex(int[] indices)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            if (indexAmount == indexArraySize) ResizeIndexArray();

            this.indices[indexAmount] = indices[i] + vertexAmount;
            indexAmount++;
        }
    }

    public void SetIndices(int[] indices)
    {
        this.indices = indices;
        indexAmount = indices.Length;
    }

    public void AddTriangle(int index1, int index2, int index3)
    {
        AddIndex(index1);
        AddIndex(index2);
        AddIndex(index3);
    }

    public void ResizeVertexArray()
    {
        T[] newVertices = new T[vertexArraySize * 2];
        for (int x = 0; x < vertexArraySize; x++)
        {
            newVertices[x] = vertices[x];
        }
        vertices = newVertices;
        vertexArraySize *= 2;
    }

    public void ResizeIndexArray()
    {
        int[] newIndices = new int[indexArraySize * 2];
        for (int x = 0; x < indexArraySize; x++)
        {
            newIndices[x] = indices[x];
        }
        indices = newIndices;
        indexArraySize *= 2;
    }

    /// <summary>
    /// Appends a mesh data to the end of this mesh data.
    /// Will offset indices based on the current vertex amount.
    /// </summary>
    public void AddMeshData(MeshInfo<T> meshData)
    {
        // Offset indices to match the new vertex amount.
        AddOffsetIndices(meshData.indices, vertexAmount);

        AddVertices(meshData.vertices);
    }

    public MeshHandle Upload()
    {
        return RenderTools.UploadMesh(this);
    }

    public MeshInfo<T1> Get<T1>() where T1 : unmanaged
    {
        return this as MeshInfo<T1> ?? throw new Exception($"{this} was cast to wrong struct!");
    }
}