using ProtoBuf;
using System.Collections.Generic;

namespace Equimancy;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class SpellConfig
{
    private readonly Dictionary<string, string> settings = new();

    public string SetString(string key, string value)
    {
        settings[key] = value;
        return value;
    }

    public int SetInt(string key, int value)
    {
        settings[key] = value.ToString();
        return value;
    }

    public long SetLong(string key, long value)
    {
        settings[key] = value.ToString();
        return value;
    }

    public string? GetString(string key)
    {
        settings.TryGetValue(key, out string? value);
        return value;
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (!settings.TryGetValue(key, out string? value)) return defaultValue;
        int number;
        try
        {
            number = int.Parse(value);
        }
        catch
        {
            return defaultValue;
        }

        return number;
    }

    public long GetLong(string key, long defaultValue = 0)
    {
        if (!settings.TryGetValue(key, out string? value)) return defaultValue;
        long number;
        try
        {
            number = long.Parse(value);
        }
        catch
        {
            return defaultValue;
        }

        return number;
    }
}