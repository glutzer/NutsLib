using ProtoBuf;
using Vintagestory.API.Util;

namespace Equimancy;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class SpellPacket
{
    public long instanceId;
    public int packetId;
    public byte[]? data;

    /// <summary>
    /// Return value or default.
    /// </summary>
    public T? GetData<T>()
    {
        return SerializerUtil.Deserialize<T>(data);
    }
}