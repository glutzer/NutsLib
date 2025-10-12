using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace NutsLib;

/// <summary>
/// Add this interface to an item to hook into rendering at the beginning of render held item.
/// </summary>
public interface IRenderableItem
{
    /// <summary>
    /// Returns if the base item should be rendered.
    /// This is an opaque pass, model matrix should already be setup.
    /// </summary>
    bool OnItemRender(EntityShapeRenderer instance, float dt, bool isShadowPass, ItemStack stack, AttachmentPointAndPose apap, ItemRenderInfo renderInfo);
}