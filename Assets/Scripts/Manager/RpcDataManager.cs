using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 大廳玩家資料
/// </summary>
public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    // Network Id
    public ulong NetworkClientId;
    // 暱稱
    public FixedString64Bytes Nickname;
    // 準備狀態
    public bool IsPrepare;
    // 是否是室長
    public bool IsGameHost;
   
    public bool Equals(LobbyPlayerData other)
    {
        return NetworkClientId.Equals(other.NetworkClientId) &&
               Nickname.Equals(other.Nickname) &&
               IsPrepare == other.IsPrepare &&
               IsGameHost == other.IsGameHost;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            NetworkClientId,
            Nickname,
            IsPrepare,
            IsGameHost);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref NetworkClientId);
        serializer.SerializeValue(ref Nickname);
        serializer.SerializeValue(ref IsPrepare);
        serializer.SerializeValue(ref IsGameHost);
    }
}

/// <summary>
/// 遊戲玩家資料
/// </summary>
public struct GamePlayerData : INetworkSerializable, IEquatable<GamePlayerData>
{
    // 角色Id
    public ulong CharacterId;
    // 炸彈數量
    public int BombCount;
    // 爆炸等級
    public int ExplotionLevel;
    // 移動速度
    public float MoveSpeed;

    public bool Equals(GamePlayerData other)
    {
        return CharacterId == other.CharacterId &&
               BombCount == other.BombCount &&
               ExplotionLevel == other.ExplotionLevel &&
               MoveSpeed == other.MoveSpeed;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            CharacterId,
            BombCount,
            ExplotionLevel,
            MoveSpeed);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref BombCount);
        serializer.SerializeValue(ref ExplotionLevel);
        serializer.SerializeValue(ref MoveSpeed);
    }
}