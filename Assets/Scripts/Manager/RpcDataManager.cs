using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 玩家資料
/// </summary>
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    // Network Id
    public ulong NetworkClientId;
    // 暱稱
    public FixedString64Bytes Nickname;
    // 準備狀態
    public bool IsPrepare;
    // 是否是室長
    public bool IsGameHost;
    // 是否已進入遊戲場景
    public bool IsInGameScene;

    public bool Equals(PlayerData other)
    {
        return NetworkClientId.Equals(other.NetworkClientId) &&
               Nickname.Equals(other.Nickname) &&
               IsPrepare == other.IsPrepare &&
               IsGameHost == other.IsGameHost &&
               IsInGameScene == other.IsInGameScene;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NetworkClientId, Nickname, IsPrepare, IsGameHost, IsInGameScene);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref NetworkClientId);
        serializer.SerializeValue(ref Nickname);
        serializer.SerializeValue(ref IsPrepare);
        serializer.SerializeValue(ref IsGameHost);
        serializer.SerializeValue(ref IsInGameScene);
    }
}
