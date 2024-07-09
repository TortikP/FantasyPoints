using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes lobbyPlayerId;
    public int playerId;
    public Faction playerFaction;

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && playerFaction == other.playerFaction && playerId == other.playerId && lobbyPlayerId == other.lobbyPlayerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref lobbyPlayerId);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerFaction);
    }
}
