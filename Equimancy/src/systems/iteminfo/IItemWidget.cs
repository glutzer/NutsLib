using Vintagestory.API.Common;

namespace Equimancy;

public interface IItemWidget
{
    public void SetItemStackData(ItemSlot slot);
    public void OnLeaveSlot(ItemSlot slot);
}