using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public struct NetworkString : INetworkSerializable
{
    private Unity.Collections.FixedString32Bytes info;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }
    public override string ToString()
    {
        return info.ToString();
    }
    public static implicit operator string(NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString() { info = new Unity.Collections.FixedString32Bytes(s) };
}
