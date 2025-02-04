using OpenTK.Mathematics;

namespace Equimancy;

public static class MatrixExtension
{
    /// <summary>
    /// I don't think this returns the right column major.
    /// </summary>
    public static Matrix4 ToMatrix4(this float[] originalMat)
    {
        Matrix4 newMat = new()
        {
            Column0 = new Vector4(originalMat[0], originalMat[1], originalMat[2], originalMat[3]),
            Column1 = new Vector4(originalMat[4], originalMat[5], originalMat[6], originalMat[7]),
            Column2 = new Vector4(originalMat[8], originalMat[9], originalMat[10], originalMat[11]),
            Column3 = new Vector4(originalMat[12], originalMat[13], originalMat[14], originalMat[15])
        };

        return newMat;
    }
}