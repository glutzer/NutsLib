using OpenTK.Graphics.OpenGL4;
using System;

namespace MareLib;

public unsafe class MappedUboHandle<T> : IUbo, IDisposable where T : unmanaged
{
    private int handle;
    public int Handle => handle;

    public T* Data { get; private set; }

    public int length;

    public T this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }

    public MappedUboHandle(int length = 1)
    {
        this.length = length;

        handle = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        GL.BufferStorage(BufferTarget.UniformBuffer, sizeof(T) * length, IntPtr.Zero, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        Data = (T*)GL.MapBufferRange(BufferTarget.UniformBuffer, IntPtr.Zero, sizeof(T) * length, BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
    }

    public void Bind(int bindingPoint)
    {
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, handle);
    }

    /// <summary>
    /// Will resize the buffer, and clear all data.
    /// Do this before rendering to avoid a disposed buffer bound.
    /// </summary>
    public void Resize(int newLength)
    {
        length = newLength;

        Dispose();

        handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, handle);
        GL.BufferStorage(BufferTarget.UniformBuffer, sizeof(T) * length, IntPtr.Zero, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        Data = (T*)GL.MapBufferRange(BufferTarget.UniformBuffer, IntPtr.Zero, sizeof(T) * length, BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        GL.BindBuffer(BufferTarget.UniformBuffer, handle);

        // Might not need to? Causes errors.
        GL.UnmapBuffer(BufferTarget.UniformBuffer);

        GL.DeleteBuffer(handle);
    }
}
