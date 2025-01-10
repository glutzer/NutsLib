using MareLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace Equimancy;

/// <summary>
/// Bars like mana.
/// </summary>
public class HudStats : Gui
{
    public override EnumDialogType DialogType => EnumDialogType.HUD;
    public override bool Focusable => false;
    public override double DrawOrder => 0.8;

    public EquimancyPlayerData playerData;

    public HudStats(ICoreClientAPI capi) : base(capi)
    {
        TryOpen();

        playerData = MainAPI.GetGameSystem<EquimancySaveDataSystem>(EnumAppSide.Client).GetPlayerData(ClientSettings.PlayerName);
    }

    public override void PopulateWidgets(out Bounds mainBounds)
    {
        mainBounds = Bounds.Create().Fixed(-40, -40, 10, 50).Alignment(Align.RightBottom);

        AddWidget(new VerticalStatusWidget(this, mainBounds, () =>
        {
            return playerData.Mana / playerData.MaxMana;
        }));
    }
}