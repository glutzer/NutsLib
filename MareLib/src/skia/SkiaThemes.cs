using SkiaSharp;

namespace MareLib;

public static partial class SkiaThemes
{
    public static readonly SKColor White = new(255, 255, 255);
    public static readonly SKColor Black = new(0, 0, 0);
    public static readonly SKColor Gray = new(128, 128, 128);
    public static readonly SKColor DarkGray = new(64, 64, 64);

    // 3 primary colors.
    public static readonly SKColor Red = new(255, 0, 0);
    public static readonly SKColor Green = new(0, 255, 0);
    public static readonly SKColor Blue = new(0, 0, 255);

    // 3 secondary colors.
    public static readonly SKColor Yellow = new(255, 255, 0);
    public static readonly SKColor Cyan = new(0, 255, 255);
    public static readonly SKColor Magenta = new(255, 0, 255);

    // Colors of rarity.
    public static readonly SKColor Poor = new(150, 150, 150);
    public static readonly SKColor Common = new(255, 255, 255);
    public static readonly SKColor Uncommon = new(30, 255, 0);
    public static readonly SKColor Rare = new(20, 110, 220);
    public static readonly SKColor Epic = new(160, 50, 230);
    public static readonly SKColor Legendary = new(255, 128, 0);

    // Special colors.
    public static readonly SKColor Teal = new(50, 255, 200);
    public static readonly SKColor Beige = new(255, 255, 200);
    public static readonly SKColor Pink = new(255, 200, 255);
}