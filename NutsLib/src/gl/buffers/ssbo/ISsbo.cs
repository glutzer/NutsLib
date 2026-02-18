namespace NutsLib;

public interface ISsbo
{
    int Handle { get; }

    /// <summary>
    /// Bind buffer to an index.
    /// Example: readonly restrict layout(std430, binding = index) buffer matrices.
    /// Readonly is used for ssbos that do not need their data set by compute shaders.
    /// Restrict means only one shader may access it at a time (always use).
    /// </summary>
    void Bind(uint bindingPoint);
}