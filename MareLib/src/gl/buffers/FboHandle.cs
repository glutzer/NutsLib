using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace MareLib;

/// <summary>
/// Framebuffer, no mipmap support.
/// </summary>
public class FboHandle : IDisposable
{
    private readonly int handle;

    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly bool[] activeColorBuffers = new bool[4];

    /// <summary>
    /// Dictionary to the handle.
    /// </summary>
    private readonly Dictionary<FramebufferAttachment, Texture> attachments = new();

    public FboHandle(int width, int height)
    {
        Width = width;
        Height = height;
        handle = GL.GenFramebuffer();
    }

    /// <summary>
    /// Get the texture of an attachment.
    /// </summary>
    public Texture this[FramebufferAttachment attachment]
    {
        get => attachments[attachment];
    }

    /// <summary>
    /// Binds this fbo for use.
    /// Can either bind to the read, write, or both.
    /// </summary>
    public void Bind(FramebufferTarget target)
    {
        GL.BindFramebuffer(target, handle);
    }

    public void SetDimensions(int width, int height)
    {
        if (width == Width && height == Height) return;

        Width = width;
        Height = height;

        // These SHOULD already be attached so no need to re-attach them?
        foreach (KeyValuePair<FramebufferAttachment, Texture> attachment in attachments)
        {
            int textureHandle = attachment.Value.Handle;
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);

            if (attachment.Key == FramebufferAttachment.DepthAttachment)
            {
                GL.TexStorage2D(TextureTarget2d.Texture2D, 0, SizedInternalFormat.DepthComponent32f, width, height);
            }
            else
            {
                GL.TexStorage2D(TextureTarget2d.Texture2D, 0, SizedInternalFormat.Rgba8, width, height);
            }

            attachment.Value.Width = width;
            attachment.Value.Height = height;
        }
    }

    public FboHandle AddAttachment(FramebufferAttachment attachment, bool aliased = true)
    {
        if (attachments.ContainsKey(attachment))
        {
            throw new Exception($"Attachment already exists: {attachment}!");
        }

        int textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);

        // Make it nearest.
        Texture.SetAliasing(aliased, false, TextureTarget.Texture2D);

        if (attachment == FramebufferAttachment.DepthAttachment)
        {
            GL.TexStorage2D(TextureTarget2d.Texture2D, 0, SizedInternalFormat.DepthComponent32f, Width, Height);
        }
        else
        {
            GL.TexStorage2D(TextureTarget2d.Texture2D, 0, SizedInternalFormat.Rgba8, Width, Height);
            activeColorBuffers[attachment - FramebufferAttachment.ColorAttachment0] = true;
        }

        int previousFramebufferId = GL.GetInteger(GetPName.FramebufferBinding);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2D, textureHandle, 0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebufferId);

        attachments.Add(attachment, new Texture() { Handle = textureHandle, Width = Width, Height = Height });

        return this;
    }

    public void Dispose()
    {
        foreach (KeyValuePair<FramebufferAttachment, Texture> attachment in attachments)
        {
            GL.DeleteTexture(attachment.Value.Handle);
        }

        GL.DeleteFramebuffer(handle);
        GC.SuppressFinalize(this);
    }
}