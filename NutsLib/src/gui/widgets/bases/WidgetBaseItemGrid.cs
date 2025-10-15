using OpenTK.Mathematics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace NutsLib;

/// <summary>
/// Base class for item slots, only handles logic.
/// Does not have individual bounds per slot.
/// 
/// Needs implementation: rendering, textures.
/// </summary>
public class WidgetBaseItemGrid : Widget
{
    protected ItemSlot[] slots;
    protected int width;
    protected int height;

    private readonly int slotSize;
    protected virtual int SlotSize => slotSize * Scale;

    // Index of currently moused over slot, or -1 if no slot.
    public int MousedSlotIndex { get; private set; }

    protected virtual bool IsEnabled => true;

    public WidgetBaseItemGrid(ItemSlot[] slots, int width, int height, int slotSize, Widget? parent, Gui gui) : base(parent, gui)
    {
        this.slots = slots;
        this.width = width;
        this.height = height;
        this.slotSize = slotSize;
    }

    public override void OnRender(float dt, NuttyShader shader)
    {
        if (!IsEnabled) return;

        int slotRenderSize = SlotSize;

        for (int i = 0; i < slots.Length; i++)
        {
            Vector2 offset = GetOffsetOfSlot(i % width, i / width, SlotSize);

            offset.X += X;
            offset.Y += Y;
            offset.X = (int)offset.X;
            offset.Y = (int)offset.Y;

            ItemSlot slot = slots[i];

            RenderBackground(offset, slotRenderSize, dt, shader, slot, i);
            if (slot.Itemstack != null) RenderItem(offset, slotRenderSize, dt, shader, slot, i);
            RenderOverlay(offset, slotRenderSize, dt, shader, slot, i);
        }
    }

    public virtual void OnSlotActivated(int slotIndex, ItemSlot slot)
    {

    }

    public virtual void RenderBackground(Vector2 start, int size, float dt, NuttyShader shader, ItemSlot slot, int slotIndex)
    {

    }

    public virtual void RenderItem(Vector2 offset, int size, float dt, NuttyShader shader, ItemSlot slot, int slotIndex)
    {
        RenderTools.PushScissor((int)offset.X, (int)offset.Y, size, size);
        RenderTools.RenderItemStackToGui(slot, shader, offset.X + (size / 2), offset.Y + (size / 2), size / 2, dt, false);
        RenderTools.PopScissor();
    }

    public virtual void RenderOverlay(Vector2 start, int size, float dt, NuttyShader shader, ItemSlot slot, int slotIndex)
    {

    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseMove += GuiEvents_MouseMove;
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
        guiEvents.MouseWheel += GuiEvents_MouseWheel;
    }

    private void GuiEvents_MouseWheel(MouseWheelEventArgs obj)
    {
        if (!IsEnabled) return;

        if (MousedSlotIndex == -1 || obj.IsHandled) return;
        WheelSlot(MousedSlotIndex, obj);
        obj.SetHandled();
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        if (!IsEnabled) return;

        if (IsInAllBounds(obj) && MousedSlotIndex != -1)
        {
            obj.Handled = true;
        }
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!IsEnabled) return;

        if (!IsInAllBounds(obj) || obj.Handled) return;

        MousedSlotIndex = GetMousedIndex(obj.X, obj.Y);

        if (MousedSlotIndex != -1)
        {
            ClickSlot(MousedSlotIndex, obj.Button);
            obj.Handled = true;
        }
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (!IsEnabled) return;

        int newIndex = GetMousedIndex(obj.X, obj.Y);
        if (newIndex == -1)
        {
            if (MousedSlotIndex != -1)
            {
                MainAPI.Capi.Input.TriggerOnMouseLeaveSlot(slots[MousedSlotIndex]);
            }

            MousedSlotIndex = -1;
            return;
        }

        if (obj.Handled) return;
        obj.Handled = true;

        int oldIndex = MousedSlotIndex;
        //mousedSlotIndex = GetMousedIndex(obj.X, obj.Y);
        MousedSlotIndex = newIndex;

        if (MousedSlotIndex != -1 && MousedSlotIndex != oldIndex)
        {
            MainAPI.Capi.Event.EnqueueMainThreadTask(() =>
            {
                MainAPI.Capi.Input.TriggerOnMouseEnterSlot(slots[MousedSlotIndex]);
            }, "");
        }

        if (MousedSlotIndex == -1 && oldIndex != -1)
        {
            MainAPI.Capi.Input.TriggerOnMouseLeaveSlot(slots[oldIndex]);
        }
    }

    /// <summary>
    /// Equally divides the slots among the bounds, trying to center them.
    /// </summary>
    protected Vector2 GetOffsetOfSlot(int indexX, int indexY, int size)
    {
        int maxWidth = Width - size;
        int maxHeight = Height - size;

        float xRatio = width == 1 ? 0.5f : (float)indexX / (width - 1);
        float yRatio = height == 1 ? 0.5f : (float)indexY / (height - 1);

        return new Vector2(maxWidth * xRatio, maxHeight * yRatio);
    }

    /// <summary>
    /// Returns index, or -1 if not found.
    /// </summary>
    protected int GetMousedIndex(int x, int y)
    {
        int size = SlotSize;

        for (int i = 0; i < slots.Length; i++)
        {
            Vector2 offset = GetOffsetOfSlot(i % width, i / width, size);

            if (IsInAllBounds(x, y, X + (int)offset.X, Y + (int)offset.Y, size, size))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Click a slot, mostly vanilla behavior.
    /// </summary>
    protected void ClickSlot(int index, EnumMouseButton mouseButton)
    {
        ICoreClientAPI capi = MainAPI.Capi;

        IInventory mouseInventory = capi.World.Player.InventoryManager.GetOwnInventory("mouse");

        bool shiftPressed = capi.Input.KeyboardKeyState[(int)GlKeys.ShiftLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.ShiftRight];
        bool ctrlPressed = capi.Input.KeyboardKeyState[(int)GlKeys.ControlLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.ControlRight];
        bool altPressed = capi.Input.KeyboardKeyState[(int)GlKeys.AltLeft] || capi.Input.KeyboardKeyState[(int)GlKeys.AltRight];

        EnumModifierKey modifiers = (shiftPressed ? EnumModifierKey.SHIFT : 0) |
                                    (ctrlPressed ? EnumModifierKey.CTRL : 0) |
                                    (altPressed ? EnumModifierKey.ALT : 0);

        ItemSlot clickedSlot = slots[index];
        IInventory slotInventory = clickedSlot.Inventory;
        int slotId = slotInventory.GetSlotId(clickedSlot);

        object packet;

        ItemStackMoveOperation op = new(capi.World, mouseButton, modifiers, EnumMergePriority.AutoMerge)
        {
            ActingPlayer = capi.World.Player
        };

        if (shiftPressed)
        {
            op.RequestedQuantity = clickedSlot.StackSize;
            packet = slotInventory.ActivateSlot(slotId, clickedSlot, ref op);
        }
        else
        {
            op.CurrentPriority = EnumMergePriority.DirectMerge;
            packet = slotInventory.ActivateSlot(slotId, mouseInventory[0], ref op);
        }

        OnSlotActivated(index, clickedSlot);

        if (packet != null)
        {
            if (packet is object[] packets)
            {
                for (int i = 0; i < packets.Length; i++)
                {
                    capi.Network.SendPacketClient(packets[i]);
                }
            }
            else
            {
                capi.Network.SendPacketClient(packet);
            }
        }
    }

    protected void WheelSlot(int index, MouseWheelEventArgs args)
    {
        ICoreClientAPI capi = MainAPI.Capi;

        ItemStackMoveOperation op = new(capi.World, EnumMouseButton.Wheel, 0, EnumMergePriority.AutoMerge, 1)
        {
            WheelDir = (args.delta > 0) ? 1 : (-1),
            ActingPlayer = capi.World.Player
        };

        IInventory ownInventory = capi.World.Player.InventoryManager.GetOwnInventory("mouse");
        ItemSlot sourceSlot = ownInventory[0];

        ItemSlot clickedSlot = slots[index];
        IInventory slotInventory = clickedSlot.Inventory;

        object packet = slotInventory.ActivateSlot(slotInventory.GetSlotId(clickedSlot), sourceSlot, ref op);
        OnSlotActivated(index, clickedSlot);

        if (packet == null) return;

        if (packet is object[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                capi.Network.SendPacketClient(packet);
            }
        }
        else
        {
            capi.Network.SendPacketClient(packet);
        }
    }

    public override void Dispose()
    {
        // Leave slot.
        if (MousedSlotIndex != -1)
        {
            MainAPI.Capi.Input.TriggerOnMouseLeaveSlot(slots[MousedSlotIndex]);
        }
    }
}