using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Vintagestory.Client.NoObf;

namespace MareLib;

public static unsafe class RenderTools
{
    public static Stack<ScissorBounds> ScissorStack { get; set; } = new();

    public static void BindTexture(Texture texture, ShaderProgram program)
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
        program.Uniform("tex2d", 0);
    }

    /// <summary>
    /// Renders a nine-slice texture. Scale with scale the size of the texture that is repeated and the border.
    /// This should be an integer amount for the texture to render correctly, like the gui scale (1-4x).
    /// </summary>
    public static void RenderNineSlice(NineSliceTexture texture, ShaderProgram guiShader, float x, float y, float width, float height, float scale = 1)
    {
        // Round everything to prevent sub-pixel rendering.
        x = MathF.Round(x);
        y = MathF.Round(y);
        width = MathF.Round(width);
        height = MathF.Round(height);

        BindTexture(texture.texture, guiShader);

        guiShader.Uniform("border", texture.Border);
        guiShader.Uniform("dimensions", texture.GetDimensions(width / scale, height / scale));
        guiShader.Uniform("centerScale", texture.GetCenterScale(width / scale, height / scale));

        guiShader.Uniform("shaderType", 1);

        Matrix4 translation = Matrix4.CreateScale(width, height, 1) * Matrix4.CreateTranslation(x, y, 0);

        guiShader.Uniform("modelMatrix", translation);

        RenderMesh(MainHook.GuiQuad);

        guiShader.Uniform("shaderType", 0);
    }

    /// <summary>
    /// Render a gui quad at a position. X and y are locked to the pixel grid.
    /// </summary>
    public static void RenderQuad(ShaderProgram guiShader, float x, float y, float width, float height)
    {
        // Round everything to prevent sub-pixel rendering.
        x = MathF.Round(x);
        y = MathF.Round(y);
        width = MathF.Round(width);
        height = MathF.Round(height);

        Matrix4 translation = Matrix4.CreateScale(width, height, 1) * Matrix4.CreateTranslation(x, y, 0);
        guiShader.Uniform("modelMatrix", translation);

        RenderMesh(MainHook.GuiQuad);
    }

    /// <summary>
    /// Render a single font character.
    /// </summary>
    public static void RenderFontChar(int vaoId)
    {
        GL.BindVertexArray(vaoId);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
    }

    /// <summary>
    /// Render a mesh.
    /// </summary>
    public static void RenderMesh(MeshHandle meshHandle)
    {
        GL.BindVertexArray(meshHandle.vaoId);
        GL.DrawElements(meshHandle.drawMode, meshHandle.indexAmount, DrawElementsType.UnsignedInt, 0);
    }

    public static void PushScissor(Bounds bounds)
    {
        if (ScissorStack.Count == 0)
        {
            ScissorStack.Push(new ScissorBounds()
            {
                x = bounds.X,
                y = bounds.Y,
                width = bounds.Width,
                height = bounds.Height
            });
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(bounds.X, MainHook.RenderHeight - bounds.Y - bounds.Height, bounds.Width, bounds.Height);
        }
        else
        {
            ScissorBounds peekedBounds = ScissorStack.Peek();

            ScissorBounds scissor = new()
            {
                x = Math.Max(bounds.X, peekedBounds.x),
                y = Math.Max(bounds.Y, peekedBounds.y)
            };
            scissor.width = Math.Min(bounds.X + bounds.Width, peekedBounds.x + peekedBounds.width) - scissor.x;
            scissor.height = Math.Min(bounds.Y + bounds.Height, peekedBounds.y + peekedBounds.height) - scissor.y;

            if (scissor.width < 0) scissor.width = 0;
            if (scissor.height < 0) scissor.height = 0;

            ScissorStack.Push(scissor);

            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(scissor.x, MainHook.RenderHeight - scissor.y - scissor.height, scissor.width, scissor.height);
        }
    }

    public static void PopScissor()
    {
        ScissorStack.Pop();
        if (ScissorStack.Count == 0)
        {
            GL.Disable(EnableCap.ScissorTest);
        }
        else
        {
            ScissorBounds bounds = ScissorStack.Peek();
            GL.Scissor(bounds.x, MainHook.RenderHeight - bounds.y - bounds.height, bounds.width, bounds.height);
        }
    }

    /// <summary>
    /// Is a screen coordinate inside the current scissor?
    /// </summary>
    public static bool IsPointInsideScissor(int x, int y)
    {
        if (ScissorStack.Count == 0) return true;

        ScissorBounds bounds = ScissorStack.Peek();
        if (x < bounds.x || x > bounds.x + bounds.width || y < bounds.y || y > bounds.y + bounds.height)
        {
            return false;
        }

        return true;
    }

    public static MeshHandle UploadMesh<T>(MeshInfo<T> meshData) where T : unmanaged
    {
        MeshHandle handle = new()
        {
            vaoId = GL.GenVertexArray()
        };
        GL.BindVertexArray(handle.vaoId);

        int stride = Marshal.SizeOf(typeof(T));

        handle.vertexId = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, handle.vertexId);

        // Buffer entire struct.
        GL.BufferData(BufferTarget.ArrayBuffer, stride * meshData.vertexAmount, meshData.vertices, meshData.usageType);

        uint currentIndex = 0;
        int currentOffset = 0;

        // Iterate over each field in the struct.
        foreach (FieldInfo field in typeof(T).GetFields())
        {
            Type fieldType = field.FieldType;

            if (fieldType == typeof(int))
            {
                GL.VertexAttribIPointer(currentIndex, 1, VertexAttribIntegerType.Int, stride, currentOffset);
                GL.EnableVertexAttribArray(currentIndex++);
                currentOffset += sizeof(int);
                continue;
            }

            if (fieldType == typeof(float))
            {
                GL.VertexAttribPointer(currentIndex, 1, VertexAttribPointerType.Float, false, stride, currentOffset);
                GL.EnableVertexAttribArray(currentIndex++);
                currentOffset += sizeof(float);
                continue;
            }

            if (fieldType == typeof(Vector2))
            {
                GL.VertexAttribPointer(currentIndex, 2, VertexAttribPointerType.Float, false, stride, currentOffset);
                GL.EnableVertexAttribArray(currentIndex++);
                currentOffset += sizeof(float) * 2;
                continue;
            }

            if (fieldType == typeof(Vector3))
            {
                GL.VertexAttribPointer(currentIndex, 3, VertexAttribPointerType.Float, false, stride, currentOffset);
                GL.EnableVertexAttribArray(currentIndex++);
                currentOffset += sizeof(float) * 3;
                continue;
            }

            if (fieldType == typeof(Vector4))
            {
                GL.VertexAttribPointer(currentIndex, 4, VertexAttribPointerType.Float, false, stride, currentOffset);
                GL.EnableVertexAttribArray(currentIndex++);
                currentOffset += sizeof(float) * 4;
                continue;
            }
        }

        // Pointers point to array buffers, not needed to be bound to vao.
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        handle.indexId = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.indexId);
        GL.BufferData(BufferTarget.ElementArrayBuffer, meshData.indexArraySize * sizeof(uint), meshData.indices, meshData.usageType);
        // Don't unbind EBO, it must stay bound to the vertex array.

        // Unbinding of the VAO state is not needed.
        //GL.BindVertexArray(0);

        handle.drawMode = meshData.drawMode;
        handle.usageType = meshData.usageType;
        handle.indexAmount = meshData.indexAmount;

        return handle;
    }

    /// <summary>
    /// Updates a mesh.
    /// Re-allocates the entire buffer.
    /// Basically re-uploading the mesh.
    /// </summary>
    public static void UpdateMesh<T>(MeshInfo<T> meshData, MeshHandle handle) where T : unmanaged
    {
        GL.BindVertexArray(handle.vaoId);

        GL.BindBuffer(BufferTarget.ArrayBuffer, handle.vertexId);

        GL.BufferData(BufferTarget.ArrayBuffer, meshData.vertexAmount * Marshal.SizeOf(typeof(T)), meshData.vertices, meshData.usageType);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.indexId);
        GL.BufferData(BufferTarget.ElementArrayBuffer, meshData.indexArraySize * sizeof(int), meshData.indices, meshData.usageType);

        handle.indexAmount = meshData.indexArraySize;
    }

    /// <summary>
    /// Vertex offset is in order, not bytes.
    /// Updates data at certain vertex and index points.
    /// Used in a system where vertex and index start and count are recorded.
    /// </summary>
    public static void UpdateMesh<T>(MeshInfo<T> meshData, MeshHandle handle, int vertexOffset, int indexOffset) where T : unmanaged
    {
        GL.BindVertexArray(handle.vaoId);

        GL.BindBuffer(BufferTarget.ArrayBuffer, handle.vertexId);
        GL.BufferSubData(BufferTarget.ArrayBuffer, vertexOffset * Marshal.SizeOf(typeof(T)), meshData.vertexAmount * Marshal.SizeOf(typeof(T)), meshData.vertices);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.indexId);
        GL.BufferSubData(BufferTarget.ElementArrayBuffer, indexOffset * sizeof(int), meshData.indexArraySize * sizeof(int), meshData.indices);
    }
}

public struct ScissorBounds
{
    public int x;
    public int y;
    public int width;
    public int height;
}