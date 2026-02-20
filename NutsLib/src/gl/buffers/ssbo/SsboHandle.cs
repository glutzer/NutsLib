using OpenTK.Graphics.OpenGL4;

namespace NutsLib;

public unsafe class SsboHandle<T> : IUbo, IDisposable where T : unmanaged
{
    public readonly int handle;
    public int Handle => handle;

    private readonly BufferUsageHint usageType;

    public SsboHandle(BufferUsageHint usageType)
    {
        handle = GL.GenBuffer();
        this.usageType = usageType;

        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, handle);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(T), IntPtr.Zero, usageType);
    }

    public void BufferData(T data)
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, handle);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(T), ref data, usageType);
    }

    public void UpdateData(T data)
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, handle);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, sizeof(T), ref data);
    }

    public void BufferData(T[] data)
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, handle);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(T) * data.Length, data, usageType);
    }

    public void UpdateData(T[] data)
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, handle);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, sizeof(T) * data.Length, data);
    }

    public void UpdateData(T[] data, int changedEntries)
    {
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, handle);
        GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, sizeof(T) * changedEntries, data);
    }

    public void Bind(int bindingPoint)
    {
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, handle);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GL.DeleteBuffer(handle);
    }
}