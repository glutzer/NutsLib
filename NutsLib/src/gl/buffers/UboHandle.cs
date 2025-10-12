using OpenTK.Graphics.OpenGL4;
using System;

namespace NutsLib;

public unsafe class UboHandle<T> : IUbo, IDisposable where T : unmanaged
{
    public readonly int handle;
    public int Handle => handle;

    private readonly BufferUsageHint usageType;

    public UboHandle(BufferUsageHint usageType)
    {
        handle = GL.GenBuffer();
        this.usageType = usageType;

        GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        GL.BufferData(BufferTarget.UniformBuffer, sizeof(T), IntPtr.Zero, usageType);
    }

    public void BufferData(T data)
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        GL.BufferData(BufferTarget.UniformBuffer, sizeof(T), ref data, usageType);
    }

    public void UpdateData(T data)
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, sizeof(T), ref data);
    }

    public void BufferData(T[] data)
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        GL.BufferData(BufferTarget.UniformBuffer, sizeof(T) * data.Length, data, usageType);
    }

    public void UpdateData(T[] data)
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, sizeof(T) * data.Length, data);
    }

    public void Bind(int bindingPoint)
    {
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, handle);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GL.DeleteBuffer(handle);
    }
}