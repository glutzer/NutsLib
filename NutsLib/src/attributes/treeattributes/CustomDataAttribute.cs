using System.Diagnostics.CodeAnalysis;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace NutsLib;

public static class AttributeExtensions
{
    /// <summary>
    /// Try to get and deserialize a custom object.
    /// </summary>
    public static T? GetObject<T>(this ITreeAttribute instance, string key) where T : class
    {
        if (instance.TryGetAttribute(key, out IAttribute? attribute))
        {
            if (attribute is CustomDataAttribute customDataAttribute)
            {
                object? value = customDataAttribute.GetValue();

                if (value is T generic)
                {
                    return generic;
                }

                if (value is byte[] bytes)
                {
                    T? deserialized = SerializerUtil.Deserialize<T>(bytes);
                    if (deserialized == null)
                    {
                        // Invalid data.
                        instance.RemoveAttribute(key);
                        return null;
                    }
                    customDataAttribute.SetObject(deserialized);
                    return deserialized;
                }

                // Invalid data.
                instance.RemoveAttribute(key);
            }
        }

        return null;
    }

    public static bool TryGetObject<T>(this ITreeAttribute instance, string key, [NotNullWhen(true)] out T? value) where T : class
    {
        value = instance.GetObject<T>(key);
        return value != null;
    }

    public static void SetObject<T>(this ITreeAttribute instance, string key, T obj) where T : class
    {
        CustomDataAttribute customDataAttribute = new(obj);
        instance[key] = customDataAttribute;
    }
}

/// <summary>
/// Stores bytes, deserializes as first thing that gets it.
/// Must be proto-serializable.
/// </summary>
public class CustomDataAttribute : IAttribute
{
    private object? customDataObject;
    private byte[]? objectBytes;

    public CustomDataAttribute(object customDataObject)
    {
        this.customDataObject = customDataObject;
    }

    public CustomDataAttribute(object customDataObject, byte[]? objectBytes)
    {
        this.customDataObject = customDataObject;
        this.objectBytes = objectBytes;
    }

    public CustomDataAttribute(byte[]? objectBytes)
    {
        this.objectBytes = objectBytes;
    }

    public CustomDataAttribute()
    {

    }

    public void SetObject(object obj)
    {
        customDataObject = obj;
    }

    /// <summary>
    /// Attempt to clone the bytes if possible.
    /// </summary>
    public IAttribute Clone()
    {
        if (objectBytes == null && customDataObject != null)
        {
            objectBytes = SerializerUtil.Serialize(customDataObject);
        }

        return new CustomDataAttribute(objectBytes);
    }

    /// <summary>
    /// Check if data matches.
    /// </summary>
    public bool Equals(IWorldAccessor worldForResolve, IAttribute otherAttribute)
    {
        if (otherAttribute is not CustomDataAttribute customAttr) return false;

        byte[]? data = GetBytes();
        byte[]? otherData = customAttr.GetBytes();

        if (data == null && otherData == null) return true;
        if (data == null || otherData == null || data.Length != otherData.Length) return false;

        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != otherData[i]) return false;
        }

        return true;
    }

    public byte[]? GetBytes()
    {
        if (customDataObject != null)
        {
            objectBytes = SerializerUtil.Serialize(customDataObject);
        }

        return objectBytes;
    }

    public int GetAttributeId()
    {
        return 434343;
    }

    public object? GetValue()
    {
        return customDataObject ?? objectBytes;
    }

    public void ToBytes(BinaryWriter stream)
    {
        if (objectBytes == null && customDataObject != null)
        {
            objectBytes = SerializerUtil.Serialize(customDataObject);
        }

        if (objectBytes == null)
        {
            stream.Write(0);
            return;
        }

        stream.Write(objectBytes.Length);
        stream.Write(objectBytes);
    }

    public void FromBytes(BinaryReader stream)
    {
        // Refresh the object when receiving new data.
        customDataObject = null;

        int length = stream.ReadInt32();
        if (length == 0)
        {
            objectBytes = null;
            return;
        }

        objectBytes = stream.ReadBytes(length);
    }

    public string ToJsonToken()
    {
        return "";
    }
}