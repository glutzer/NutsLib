namespace MareLib;

public static class TimeUtility
{
    public static float ElapsedClientSeconds()
    {
        return MainAPI.Client.ElapsedMilliseconds / 1000f;
    }
}