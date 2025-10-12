namespace NutsLib;

public static class TimeUtility
{
    /// <summary>
    /// Get time for shaders.
    /// </summary>
    public static float ElapsedClientSeconds()
    {
        return MainAPI.Client.ElapsedMilliseconds / 1000f;
    }
}