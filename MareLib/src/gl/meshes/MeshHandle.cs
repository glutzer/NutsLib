using OpenTK.Graphics.OpenGL4;

namespace MareLib;

public struct DrawInfo
{
    public int vaoId;
    public int indexCount;
    public PrimitiveType drawMode;
}

public class MeshHandle
{
    public int vertexId;
    public int indexId;
    public int vaoId;

    /// <summary>
    /// Not always set.
    /// </summary>
    public int indirectId;

    public int indexAmount;
    public PrimitiveType drawMode;
    public BufferUsageHint usageType;

    public MeshHandle()
    {

    }

    public void Dispose()
    {
        if (vertexId != 0)
        {
            GL.DeleteBuffer(vertexId);
            vertexId = 0;
        }

        if (indexId != 0)
        {
            GL.DeleteBuffer(indexId);
            indexId = 0;
        }

        if (vaoId != 0)
        {
            GL.DeleteVertexArray(vaoId);
            vaoId = 0;
        }

        if (indirectId != 0)
        {
            GL.DeleteBuffer(indirectId);
            indirectId = 0;
        }
    }

    /// <summary>
    /// Returns a struct about draw info. Will not be updated.
    /// </summary>
    public DrawInfo GetDrawInfo()
    {
        return new DrawInfo
        {
            vaoId = vaoId,
            indexCount = indexAmount,
            drawMode = drawMode
        };
    }

    ~MeshHandle()
    {
        if (vertexId != 0 || indexId != 0 || vaoId != 0)
        {

        }
    }
}