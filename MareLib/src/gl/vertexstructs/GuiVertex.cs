using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MareLib;

[StructLayout(LayoutKind.Sequential)]
public struct GuiVertex
{
    public Vector3 position;
    public Vector2 uv;
    public Vector4 color;

    public GuiVertex(Vector3 position, Vector2 uv)
    {
        this.position = position;
        this.uv = uv;
        color = new Vector4(1, 1, 1, 1);
    }

    public GuiVertex(Vector3 position, Vector2 uv, Vector4 color)
    {
        this.position = position;
        this.uv = uv;
        this.color = color;
    }
}