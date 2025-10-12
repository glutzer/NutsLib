﻿using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace NutsLib;

public class AttachmentInfo
{
    public PixelInternalFormat InternalFormat { get; set; }
    public PixelFormat Format { get; set; }
    public PixelType Type { get; set; }

    public AttachmentInfo(PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
    {
        InternalFormat = internalFormat;
        Format = format;
        Type = type;
    }
}

/// <summary>
/// Framebuffer, no mipmap support.
/// </summary>
public class FboHandle : IDisposable
{
    private readonly int handle;

    public int Width { get; private set; }
    public int Height { get; private set; }

    /// <summary>
    /// Dictionary to the handle.
    /// </summary>
    private readonly Dictionary<FramebufferAttachment, Texture> attachments = [];
    private readonly Dictionary<FramebufferAttachment, AttachmentInfo> attachmentInfo = [];

    public FboHandle(int width, int height)
    {
        Width = width;
        Height = height;
        handle = GL.GenFramebuffer();
    }

    /// <summary>
    /// Get the texture of an attachment.
    /// </summary>
    public Texture this[FramebufferAttachment attachment] => attachments[attachment];

    /// <summary>
    /// Binds this fbo for use.
    /// Can either bind to the read, write, or both.
    /// </summary>
    public void Bind(FramebufferTarget target)
    {
        GL.BindFramebuffer(target, handle);
    }

    /// <summary>
    /// Set the draw buffers for this framebuffer.
    /// Should only need to be set once.
    /// </summary>
    public void DrawBuffers(bool isBound, params DrawBuffersEnum[] attachments)
    {
        if (!isBound)
        {
            int previousFramebufferId = GL.GetInteger(GetPName.FramebufferBinding);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

            GL.DrawBuffers(attachments.Length, attachments);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebufferId);
        }
        else
        {
            GL.DrawBuffers(attachments.Length, attachments);
        }
    }

    public void SetDimensions(int width, int height)
    {
        if (width == Width && height == Height) return;

        Width = width;
        Height = height;

        int previousFramebufferId = GL.GetInteger(GetPName.FramebufferBinding);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

        // These SHOULD already be attached so no need to re-attach them?
        foreach (KeyValuePair<FramebufferAttachment, Texture> attachment in attachments)
        {
            int textureHandle = attachment.Value.Handle;
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);

            if (attachment.Key == FramebufferAttachment.DepthAttachment)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, 0);
            }
            else
            {
                AttachmentInfo info = attachmentInfo[attachment.Key];
                GL.TexImage2D(TextureTarget.Texture2D, 0, info.InternalFormat, Width, Height, 0, info.Format, info.Type, 0);
            }

            attachment.Value.Width = width;
            attachment.Value.Height = height;

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment.Key, TextureTarget.Texture2D, textureHandle, 0);
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebufferId);
    }

    public FboHandle AddAttachment(FramebufferAttachment attachment, bool aliased, PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
    {
        if (attachments.ContainsKey(attachment))
        {
            throw new Exception($"Attachment already exists: {attachment}!");
        }

        int textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);

        if (attachment == FramebufferAttachment.DepthAttachment)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, 0);
        }
        else
        {

            AttachmentInfo info = new(internalFormat, format, type);
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, Width, Height, 0, format, type, 0);
            attachmentInfo.Add(attachment, info);
        }

        // Make it nearest.
        Texture.SetAliasing(aliased, false, TextureTarget.Texture2D);

        int previousFramebufferId = GL.GetInteger(GetPName.FramebufferBinding);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2D, textureHandle, 0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebufferId);

        attachments.Add(attachment, new Texture() { Handle = textureHandle, Width = Width, Height = Height });

        return this;
    }

    public FboHandle AddAttachment(FramebufferAttachment attachment, bool aliased = true)
    {
        if (attachments.ContainsKey(attachment))
        {
            throw new Exception($"Attachment already exists: {attachment}!");
        }

        int textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);

        if (attachment == FramebufferAttachment.DepthAttachment)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, 0);
        }
        else
        {

            AttachmentInfo info = new(PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0);
            attachmentInfo.Add(attachment, info);
        }

        // Make it nearest.
        Texture.SetAliasing(aliased, false, TextureTarget.Texture2D);

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

    public FramebufferErrorCode Status()
    {
        int previousFramebufferId = GL.GetInteger(GetPName.FramebufferBinding);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);
        FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebufferId);
        return status;
    }
}