using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace NutsLib;

public struct ScissorBounds
{
    public int x;
    public int y;
    public int width;
    public int height;
}

[StructLayout(LayoutKind.Explicit, Size = 80)]
public struct TransformData
{
    [FieldOffset(0)]
    public Matrix4 transform;

    [FieldOffset(64)]
    public int doTrans;
}

public static unsafe class RenderTools
{
    public static UboHandle<TransformData> TransformUbo { get; set; } = null!;
    private static MareShader guiItemShader = null!;
    public static Stack<Matrix4> GuiTransformStack { get; } = new();

    public static void OnStart()
    {
        TransformUbo = new UboHandle<TransformData>(BufferUsageHint.StreamDraw);
        TransformUbo.BufferData(new TransformData() { doTrans = 0, transform = Matrix4.Identity });

        UboRegistry.SetUbo("guiTransforms", TransformUbo.handle);

        guiItemShader = MareShaderRegistry.AddShader("nutslib:itemgui", "nutslib:itemgui", "itemgui");

        GuiTransformStack.Push(Matrix4.Identity);
    }

    public static void OnStop()
    {
        TransformUbo = null!;
        guiItemShader = null!;

        GuiTransformStack.Clear();
    }

    public static Stack<ScissorBounds> ScissorStack { get; } = new();

    /// <summary>
    /// Make a triangle suitable for rendering a fullscreen quad.
    /// </summary>
    public static MeshHandle GetFullscreenTriangle()
    {
        MeshInfo<StandardVertex> meshInfo = new(3, 3);

        meshInfo.AddVertex(new StandardVertex(new Vector3(0, 0, 0), new Vector2(0, 0), Vector3.Zero, Vector4.One));
        meshInfo.AddVertex(new StandardVertex(new Vector3(0, 2, 0), new Vector2(0, 2), Vector3.Zero, Vector4.One));
        meshInfo.AddVertex(new StandardVertex(new Vector3(2, 0, 0), new Vector2(2, 0), Vector3.Zero, Vector4.One));

        meshInfo.AddIndex(0);
        meshInfo.AddIndex(1);
        meshInfo.AddIndex(2);

        return UploadMesh(meshInfo);
    }

    /// <summary>
    /// Get a framebuffer from the client platform.
    /// Be sure to rebind it.
    /// </summary>
    public static FrameBufferRef GetFramebuffer(EnumFrameBuffer buffer)
    {
        return MainAPI.Client.Platform.FrameBuffers[(int)buffer];
    }

    /// <summary>
    /// Uses the current origin view matrix and perspective matrix to convert a world position to pixel coordinates.
    /// </summary>
    public static void WorldPosToPixelCoords(Vector3d worldPos, out float x, out float y, out double distance, out bool isBehind)
    {
        worldPos -= MainAPI.OriginOffset;
        Vector4 floatPos = new((float)worldPos.X, (float)worldPos.Y, (float)worldPos.Z, 1f);

        floatPos *= MainAPI.OriginViewMatrix;

        isBehind = floatPos.Z > 0;

        floatPos *= MainAPI.PerspectiveMatrix;
        floatPos /= floatPos.W;

        x = (floatPos.X + 1f) / 2f * MainAPI.RenderWidth;
        y = (1f - floatPos.Y) / 2f * MainAPI.RenderHeight;
        distance = worldPos.Length;
    }

    /// <summary>
    /// Everything is rendered relative to the camera to avoid precision errors.
    /// </summary>
    public static Vector3 CameraRelativePosition(Vector3d position)
    {
        position -= MainAPI.OriginOffset;
        return (Vector3)position;
    }

    public static Vector3 PlayerRelativePosition(Vector3d position)
    {
        position -= MainAPI.Capi.World.Player.Entity.Pos.ToVector();
        return (Vector3)position;
    }

    /// <summary>
    /// Everything is rendered relative to the camera to avoid precision errors.
    /// </summary>
    public static Matrix4 CameraRelativeTranslation(Vector3d position)
    {
        position -= MainAPI.OriginOffset;
        return Matrix4.CreateTranslation((Vector3)position);
    }

    /// <summary>
    /// Everything is rendered relative to the camera to avoid precision errors.
    /// </summary>
    public static Matrix4 CameraRelativeTranslation(double x, double y, double z)
    {
        return Matrix4.CreateTranslation((float)(x - MainAPI.OriginOffset.X), (float)(y - MainAPI.OriginOffset.Y), (float)(z - MainAPI.OriginOffset.Z));
    }

    public static Vector4 GetIncandescenceColor(int temperature)
    {
        return temperature < 500
            ? new Vector4(0)
            : new Vector4(Math.Max(0f, Math.Min(1f, (temperature - 500) / 400f)),
            Math.Max(0f, Math.Min(1f, (temperature - 900) / 200f)),
            Math.Max(0f, Math.Min(1f, (temperature - 1100) / 200f)),
            Math.Max(0f, Math.Min(1f, (temperature - 525) / 2f))
        );
    }

    /// <summary>
    /// Use the vanilla pipeline for rendering items.
    /// Removes depth after rendering.
    /// </summary>
    public static void RenderItemStackToGui(ItemSlot slot, MareShader originalGuiShader, float x, float y, float scale, float dt, bool rotate = false)
    {
        ItemStack itemStack = slot.Itemstack;
        if (itemStack == null) return;

        ClientMain game = (ClientMain)MainAPI.Capi.World;

        ItemRenderInfo itemStackRenderInfo = InventoryItemRenderer.GetItemStackRenderInfo(game, slot, EnumItemRenderTarget.Gui, dt);
        if (itemStackRenderInfo.ModelRef == null) return;

        ModelTransform transform = itemStackRenderInfo.Transform;
        if (transform == null) return;

        guiItemShader.Use();

        itemStack.Collectible.InGuiIdle(game, itemStack);

        bool isBlock = itemStack.Class == EnumItemClass.Block;
        bool canRotate = rotate && itemStackRenderInfo.Transform.Rotate;

        int newX = (int)x - (!isBlock ? 3 : 0);
        int newY = (int)y - (!isBlock ? 1 : 0);

        Matrix4 modelMat = Matrix4.CreateTranslation(newX, newY, 0);

        modelMat = Matrix4.CreateTranslation(transform.Origin.X, transform.Origin.Y, transform.Origin.Z) * modelMat;

        // Only use 1 scale since this should always be flat at 0.
        modelMat = Matrix4.CreateScale(transform.ScaleXYZ.X * scale, transform.ScaleXYZ.Y * scale, 1) * modelMat;

        modelMat = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(transform.Rotation.X + (isBlock ? 180 : 0))) * modelMat;
        modelMat = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(transform.Rotation.Y - ((!isBlock ? 1 : -1) * (canRotate ? MainAPI.Capi.World.ElapsedMilliseconds / 50f : 0)))) * modelMat;
        modelMat = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(transform.Rotation.Z)) * modelMat;

        modelMat = Matrix4.CreateTranslation(-transform.Origin.X, -transform.Origin.Y, -transform.Origin.Z) * modelMat;

        int temperatureInt = (int)itemStack.Collectible.GetTemperature(game, itemStack);
        Vector4 incandescenceColor = GetIncandescenceColor(temperatureInt);
        int clampedTemperature = GameMath.Clamp((temperatureInt - 550) / 2, 0, 255);
        incandescenceColor.W = clampedTemperature / 255f;

        guiItemShader.Uniform("extraGlow", clampedTemperature);

        bool hasTemperature = itemStack.Attributes.HasAttribute("temperature");
        guiItemShader.Uniform("tempGlowMode", hasTemperature ? 1 : 0);

        guiItemShader.Uniform("rgbaGlowIn", hasTemperature ? incandescenceColor : new Vector4(1, 1, 1, clampedTemperature / 255f));
        guiItemShader.Uniform("rgbaIn", new Vector4(1));

        guiItemShader.Uniform("normalShaded", itemStackRenderInfo.NormalShaded ? 1 : 0);
        guiItemShader.Uniform("applyColor", itemStackRenderInfo.ApplyColor ? 1 : 0);
        guiItemShader.Uniform("alphaTest", itemStackRenderInfo.AlphaTest);
        guiItemShader.Uniform("overlayOpacity", itemStackRenderInfo.OverlayOpacity);

        // Light position for rendering this item.
        guiItemShader.Uniform("lightPosition", new Vector3(1, -1, 0).Normalized());

        if (itemStackRenderInfo.OverlayTexture != null && itemStackRenderInfo.OverlayOpacity > 0f)
        {
            guiItemShader.BindTexture(itemStackRenderInfo.OverlayTexture.TextureId, "tex2dOverlay");
            guiItemShader.Uniform("overlayTextureSize", new Vector2(itemStackRenderInfo.OverlayTexture.Width, itemStackRenderInfo.OverlayTexture.Height));
            guiItemShader.Uniform("baseTextureSize", new Vector2(itemStackRenderInfo.TextureSize.Width, itemStackRenderInfo.TextureSize.Height));
            TextureAtlasPosition textureAtlasPosition = MainAPI.Capi.Render.GetTextureAtlasPosition(itemStack);
            guiItemShader.Uniform("baseUvOrigin", new Vector2(textureAtlasPosition.x1, textureAtlasPosition.y1));
        }

        guiItemShader.Uniform("modelMatrix", modelMat);

        guiItemShader.Uniform("applyModelMat", 1);

        guiItemShader.Uniform("damageEffect", itemStackRenderInfo.DamageEffect);

        EnableDepthTest();

        RenderMultiTextureMesh(guiItemShader, itemStackRenderInfo.ModelRef, "tex2d");

        SetDepthFunc(DepthFunction.Always);
        guiItemShader.Uniform("removeDepth", 1);
        RenderMultiTextureMesh(guiItemShader, itemStackRenderInfo.ModelRef, "tex2d");
        SetDepthFunc(DepthFunction.Lequal);
        guiItemShader.Uniform("removeDepth", 0);

        DisableDepthTest();

        guiItemShader.Uniform("applyModelMat", 0);
        guiItemShader.Uniform("normalShaded", 0);
        guiItemShader.Uniform("tempGlowMode", 0);
        guiItemShader.Uniform("damageEffect", 0f);

        guiItemShader.Uniform("alphaTest", 0f);
        guiItemShader.Uniform("rgbaGlowIn", new Vector4(0));

        // RENDER NUMBERS HERE.

        originalGuiShader.Use();
    }

    public static void RenderQuadInstanced(MareShader guiShader, float x, float y, float width, float height, int instances)
    {
        // Round everything to prevent sub-pixel rendering.
        x = (int)x;
        y = (int)y;
        width = (int)width;
        height = (int)height;

        Matrix4 translation = Matrix4.CreateScale(width, height, 1) * Matrix4.CreateTranslation(x, y, 0);
        guiShader.Uniform("modelMatrix", translation);
        RenderMeshInstanced(MainAPI.GuiQuad, instances);
    }

    /// <summary>
    /// Renders a nine-slice texture. Scale with scale the size of the texture that is repeated and the border.
    /// This should be an integer amount for the texture to render correctly, like the gui scale (1-4x).
    /// </summary>
    public static void RenderNineSlice(NineSliceTexture texture, MareShader guiShader, float x, float y, float width, float height, float scale = 1f)
    {
        // Round everything to prevent sub-pixel rendering.
        x = (int)x;
        y = (int)y;
        width = (int)width;
        height = (int)height;

        guiShader.BindTexture(texture.texture.Handle, "tex2d", 0);

        guiShader.Uniform("border", texture.Border);
        guiShader.Uniform("dimensions", texture.GetDimensions(width / scale, height / scale));
        guiShader.Uniform("centerScale", texture.GetCenterScale(width / scale, height / scale));

        guiShader.Uniform("shaderType", 1);

        Matrix4 translation = Matrix4.CreateScale(width, height, 1) * Matrix4.CreateTranslation(x, y, 0);

        guiShader.Uniform("modelMatrix", translation);

        RenderMesh(MainAPI.GuiQuad);

        guiShader.Uniform("shaderType", 0);
    }

    /// <summary>
    /// Renders a nine-slice texture. Scale with scale the size of the texture that is repeated and the border.
    /// This should be an integer amount for the texture to render correctly, like the gui scale (1-4x).
    /// </summary>
    public static void RenderNineSliceSplit(NineSliceTexture texture, MareShader guiShader, float x, float y, float width, float height, float scaleX = 1f, float scaleY = 1f)
    {
        // Round everything to prevent sub-pixel rendering.
        x = (int)x;
        y = (int)y;
        width = (int)width;
        height = (int)height;

        guiShader.BindTexture(texture.texture.Handle, "tex2d", 0);

        guiShader.Uniform("border", texture.Border);
        guiShader.Uniform("dimensions", texture.GetDimensions(width / scaleX, height / scaleY));
        guiShader.Uniform("centerScale", texture.GetCenterScale(width / scaleX, height / scaleY));

        guiShader.Uniform("shaderType", 1);

        Matrix4 translation = Matrix4.CreateScale(width, height, 1) * Matrix4.CreateTranslation(x, y, 0);

        guiShader.Uniform("modelMatrix", translation);

        RenderMesh(MainAPI.GuiQuad);

        guiShader.Uniform("shaderType", 0);
    }

    /// <summary>
    /// Render a 0-1 quad at a pixel position.
    /// Coordinates are rounded.
    /// </summary>
    public static void RenderQuad(MareShader guiShader, float x, float y, float width, float height)
    {
        // Round everything to prevent sub-pixel rendering.
        x = (int)x;
        y = (int)y;
        width = (int)width;
        height = (int)height;

        Matrix4 translation = Matrix4.CreateScale(width, height, 1) * Matrix4.CreateTranslation(x, y, 0);
        guiShader.Uniform("modelMatrix", translation);

        RenderMesh(MainAPI.GuiQuad);
    }

    /// <summary>
    /// Render a 0-1 quad at a pixel position.
    /// Coordinates are rounded.
    /// </summary>
    public static void RenderElement(MareShader guiShader, float x, float y, float width, float height, MeshHandle handle)
    {
        // Round everything to prevent sub-pixel rendering.
        x = (int)x;
        y = (int)y;
        width = (int)width;
        height = (int)height;

        Matrix4 translation = Matrix4.CreateScale(width, height, 1) * Matrix4.CreateTranslation(x, y, 0);
        guiShader.Uniform("modelMatrix", translation);

        RenderMesh(handle);
    }

    /// <summary>
    /// Render a mesh.
    /// </summary>
    public static void RenderMesh(MeshHandle meshHandle)
    {
        GL.BindVertexArray(meshHandle.vaoId);
        GL.DrawElements(meshHandle.drawMode, meshHandle.indexAmount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0); // Somewhere, someone is binding an element buffer without binding a VAO and breaking everything.
    }

    /// <summary>
    /// Renders many instances of a mesh.
    /// </summary>
    public static void RenderMeshInstanced(MeshHandle meshHandle, int instances)
    {
        GL.BindVertexArray(meshHandle.vaoId);
        GL.DrawElementsInstanced(meshHandle.drawMode, meshHandle.indexAmount, DrawElementsType.UnsignedInt, IntPtr.Zero, instances);
        GL.BindVertexArray(0); // Somewhere, someone is binding an element buffer without binding a VAO and breaking everything.
    }

    /// <summary>
    /// For fonts.
    /// </summary>
    public static void RenderSquareVao(int vaoId)
    {
        GL.BindVertexArray(vaoId);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0); // Somewhere, someone is binding an element buffer without binding a VAO and breaking everything.
    }

    /// <summary>
    /// Render a mesh.
    /// </summary>
    public static void RenderMesh(MeshRef meshRef)
    {
        MainAPI.Capi.Render.RenderMesh(meshRef);
    }

    /// <summary>
    /// Render a mesh with multiple meshes and textures from the base game.
    /// </summary>
    public static void RenderMultiTextureMesh(MareShader shader, MultiTextureMeshRef mmr, string samplerName, int textureUnit = 0)
    {
        for (int i = 0; i < mmr.meshrefs.Length; i++)
        {
            shader.BindTexture(mmr.textureids[i], samplerName, textureUnit);
            MeshRef meshRef = mmr.meshrefs[i];
            RenderMesh(meshRef);
        }
    }

    public static void PushScissor(Widget widget)
    {
        PushScissor(widget.X, widget.Y, widget.Width, widget.Height);
    }

    public static void PushScissor(int x, int y, int width, int height)
    {
        if (GuiTransformStack.Count > 1)
        {
            Matrix4 transform = GuiTransformStack.Peek();

            Vector4 start = new Vector4(x, y, 0, 1) * transform;
            Vector4 end = new Vector4(x + width, y + height, 0, 1) * transform;

            Vector4 min = Vector4.ComponentMin(start, end);
            Vector4 max = Vector4.ComponentMax(start, end);

            x = (int)min.X;
            y = (int)min.Y;

            width = (int)(max.X - min.X);
            height = (int)(max.Y - min.Y);
        }

        if (ScissorStack.Count == 0)
        {
            ScissorStack.Push(new ScissorBounds()
            {
                x = x,
                y = y,
                width = width,
                height = height
            });
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(x, MainAPI.RenderHeight - y - height, width, height);
        }
        else
        {
            ScissorBounds peekedBounds = ScissorStack.Peek();

            ScissorBounds scissor = new()
            {
                x = Math.Max(x, peekedBounds.x),
                y = Math.Max(y, peekedBounds.y)
            };
            scissor.width = Math.Min(x + width, peekedBounds.x + peekedBounds.width) - scissor.x;
            scissor.height = Math.Min(y + height, peekedBounds.y + peekedBounds.height) - scissor.y;

            if (scissor.width < 0) scissor.width = 0;
            if (scissor.height < 0) scissor.height = 0;

            ScissorStack.Push(scissor);

            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(scissor.x, MainAPI.RenderHeight - scissor.y - scissor.height, scissor.width, scissor.height);
        }
    }

    public static void PopScissor()
    {
        ScissorStack.Pop();
        if (ScissorStack.Count == 0)
        {
            GL.Disable(EnableCap.ScissorTest);
            GL.Scissor(0, 0, MainAPI.RenderWidth, MainAPI.RenderHeight); // Test.
        }
        else
        {
            ScissorBounds bounds = ScissorStack.Peek();
            GL.Scissor(bounds.x, MainAPI.RenderHeight - bounds.y - bounds.height, bounds.width, bounds.height);
        }
    }

    /// <summary>
    /// Is a screen coordinate inside the current scissor?
    /// </summary>
    public static bool IsPointInsideScissor(int x, int y)
    {
        if (ScissorStack.Count == 0) return true;

        ScissorBounds bounds = ScissorStack.Peek();
        return x >= bounds.x && x <= bounds.x + bounds.width && y >= bounds.y && y <= bounds.y + bounds.height;
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

        handle.drawMode = meshData.drawMode;
        handle.usageType = meshData.usageType;
        handle.indexAmount = meshData.indexAmount;

        GL.BindVertexArray(0);

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

        GL.BindVertexArray(0);

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

        GL.BindVertexArray(0);
    }

    // Depth testing.
    public static void EnableDepthTest()
    {
        GL.Enable(EnableCap.DepthTest);
    }

    public static void DisableDepthTest()
    {
        GL.Disable(EnableCap.DepthTest);
    }

    public static void SetDepthFunc(DepthFunction depthFunc)
    {
        GL.DepthFunc(depthFunc);
    }

    public static void EnableDepthWrite()
    {
        GL.DepthMask(true);
    }

    public static void DisableDepthWrite()
    {
        GL.DepthMask(false);
    }

    // Culling.
    public static void EnableCulling()
    {
        GL.Enable(EnableCap.CullFace);
    }

    public static void DisableCulling()
    {
        GL.Disable(EnableCap.CullFace);
    }

    public static void SetCullMode(CullFaceMode cullMode)
    {
        GL.CullFace(cullMode);
    }

    // Blend.
    public static void EnableBlending()
    {
        GL.Enable(EnableCap.Blend);
    }

    public static void DisableBlending()
    {
        GL.Disable(EnableCap.Blend);
    }

    public static void SetAlphaBlending()
    {
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public static void SetAdditiveBlending()
    {
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
    }

    public static void SetMultiplicativeBlending()
    {
        GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.Zero);
    }
}