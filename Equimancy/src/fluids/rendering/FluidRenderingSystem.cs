using MareLib;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Equimancy;

/// <summary>
/// Handles Item stack rendering, instanced block fluid rendering.
/// </summary>
[GameSystem(forSide = EnumAppSide.Client)]
public class FluidRenderingSystem : GameSystem, IRenderer
{
    /// <summary>
    /// Keep a dictionary of cached models. It's cleared/disposed periodically.
    /// </summary>
    private readonly Dictionary<string, MultiTextureMeshRef> fluidModelCache = new();
    private const int MAX_ELEMENTS = 1024;

    public FluidRenderingSystem(bool isServer, ICoreAPI api) : base(isServer, api)
    {
    }

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {

    }

    public double RenderOrder => 0.5;
    public int RenderRange => 0;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public MultiTextureMeshRef GetFluidItemModel(FluidStorageItem item, ItemStack stack)
    {
        FluidContainer container = item.GetContainer(stack);
        FluidStack? fluidStack = container.HeldStack;

        if (fluidStack == null)
        {
            return GetOrCreate($"{item.Code}-empty", () =>
            {
                return CreateFluidItemModel(item, Vector4.Zero, 0, 0);
            });
        }

        Fluid fluid = fluidStack.fluid;

        Vector4 color = fluid.GetColor(fluidStack);
        float glow = fluid.GetGlowLevel(fluidStack);
        float fill = (float)fluidStack.Units / container.Capacity;

        return GetOrCreate($"{item.Code}-{color.X}-{color.Y}-{color.Z}-{color.W}-{glow}-{fill}", () =>
        {
            return CreateFluidItemModel(item, color, glow, fill);
        });
    }

    private MultiTextureMeshRef GetOrCreate(ReadOnlySpan<char> code, Func<MultiTextureMeshRef> meshRef)
    {
        if (fluidModelCache.Count > MAX_ELEMENTS)
        {
            foreach (MultiTextureMeshRef value in fluidModelCache.Values)
            {
                value.Dispose();
            }
            fluidModelCache.Clear();
        }

        if (fluidModelCache.TryGetValue(code.ToString(), out MultiTextureMeshRef? mesh))
        {
            return mesh;
        }

        mesh = meshRef();
        fluidModelCache.Add(code.ToString(), mesh);
        return mesh;
    }

    /// <summary>
    /// Create a fluid item model.
    /// For every element that starts with "Contents", it is altered.
    /// Must be facing up.
    /// </summary>
    public static MultiTextureMeshRef CreateFluidItemModel(Item item, Vector4 color, float glowLevel, float fillLevel)
    {
        ICoreClientAPI capi = MainAPI.Capi;

        // Get the shape this item uses.
        IAsset shapeAsset = capi.Assets.TryGet(item.Shape.Base);
        Shape shape = shapeAsset.ToObject<Shape>();

        // Add every texture for this item.
        shape.Textures.Clear();
        foreach (KeyValuePair<string, CompositeTexture> texture in item.Textures)
        {
            shape.Textures.Add(texture.Key, new AssetLocation(texture.Value.ToString().Split('@')[0]));
        }

        fillLevel = MathF.Round(fillLevel, 2);

        // Alter all "Contents" shapes.
        void RecursivelyAlter(ShapeElement element)
        {
            if (element.Children != null)
            {
                foreach (ShapeElement child in element.Children)
                {
                    RecursivelyAlter(element);
                }
            }

            if (!element.Name.StartsWith("Contents")) return;

            double height = element.To[1] - element.From[1];
            element.To[1] = element.From[1] + (height * fillLevel);

            if (element.FacesResolved != null)
            {
                foreach (ShapeElementFace face in element.FacesResolved)
                {
                    if (face != null)
                    {
                        // Minimum 1.
                        face.Glow = Math.Max((int)(glowLevel * 255), 1);
                    }
                }
            }
        }

        foreach (ShapeElement shapeElement in shape.Elements)
        {
            RecursivelyAlter(shapeElement);
        }

        // Tessellate, change every vertex with glow level > 0 to the color.
        ShapeTextureSource textureSource = new(capi, shape, "");
        capi.Tesselator.TesselateShape("", shape, out MeshData meshData, textureSource);

        for (int i = 0; i < meshData.Flags.Length; i++)
        {
            // Get value of bits 0-7.
            int glow = meshData.Flags[i] & 0x7F;

            if (glow > 0)
            {
                meshData.Rgba[i * 4] = (byte)(color.X * 255);
                meshData.Rgba[(i * 4) + 1] = (byte)(color.Y * 255);
                meshData.Rgba[(i * 4) + 2] = (byte)(color.Z * 255);
                meshData.Rgba[(i * 4) + 3] = (byte)(color.W * 255);
            }
        }

        return capi.Render.UploadMultiTextureMesh(meshData);
    }
}