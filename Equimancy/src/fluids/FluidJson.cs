using System.Collections.Generic;

namespace Equimancy;

public class FluidJson
{
    public required string Code { get; set; }

    public string Class { get; set; } = "Fluid";

    /// <summary>
    /// 0-1 glow level of the fluid.
    /// </summary>
    public float GlowLevel { get; set; } = 0;

    /// <summary>
    /// RGBA color of the fluid.
    /// </summary>
    public float[] Color { get; set; } = new float[] { 1, 1, 1, 1 };

    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
}