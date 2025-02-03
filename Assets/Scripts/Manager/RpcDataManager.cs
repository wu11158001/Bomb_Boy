using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 大廳玩家資料
/// </summary>
[Serializable]
public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    // Network Id
    public ulong NetworkClientId;
    // 玩家登入Id
    public FixedString64Bytes AuthenticationPlayerId;
    // Join Lobby Id
    public FixedString64Bytes JoinLobbyId;
    // 暱稱
    public FixedString64Bytes Nickname;
    // 準備狀態
    public bool IsPrepare;
   
    public bool Equals(LobbyPlayerData other)
    {
        return NetworkClientId.Equals(other.NetworkClientId) &&
               AuthenticationPlayerId.Equals(other.AuthenticationPlayerId) &&
               JoinLobbyId.Equals(other.JoinLobbyId) &&
               Nickname.Equals(other.Nickname) &&
               IsPrepare == other.IsPrepare;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            NetworkClientId,
            AuthenticationPlayerId,
            JoinLobbyId,
            Nickname,
            IsPrepare);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref NetworkClientId);
        serializer.SerializeValue(ref AuthenticationPlayerId);
        serializer.SerializeValue(ref JoinLobbyId);
        serializer.SerializeValue(ref Nickname);
        serializer.SerializeValue(ref IsPrepare);
    }
}

/// <summary>
/// 遊戲玩家資料
/// </summary>
public struct GamePlayerData : INetworkSerializable, IEquatable<GamePlayerData>
{
    // 玩家登入Id
    public FixedString64Bytes AuthenticationPlayerId;
    // 角色物件Id
    public ulong CharacterId;
    // 角色暱稱
    public FixedString64Bytes Nickname;
    // 炸彈數量
    public int BombCount;
    // 爆炸等級
    public int ExplotionLevel;
    // 移動速度
    public float MoveSpeed;
    // 死亡狀態
    public bool IsDie;
    // 停止行為
    public bool IsStopAction;

    public bool Equals(GamePlayerData other)
    {
        return AuthenticationPlayerId.Equals(other.AuthenticationPlayerId) &&
               CharacterId == other.CharacterId &&
               Nickname.Equals(other.Nickname) &&
               BombCount == other.BombCount &&
               ExplotionLevel == other.ExplotionLevel &&
               MoveSpeed == other.MoveSpeed &&
               IsDie == other.IsDie &&
               IsStopAction == other.IsStopAction;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            AuthenticationPlayerId,
            CharacterId,
            Nickname,
            BombCount,
            ExplotionLevel,
            MoveSpeed,
            IsDie,
            IsStopAction);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref AuthenticationPlayerId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref Nickname);
        serializer.SerializeValue(ref BombCount);
        serializer.SerializeValue(ref ExplotionLevel);
        serializer.SerializeValue(ref MoveSpeed);
        serializer.SerializeValue(ref IsDie);
        serializer.SerializeValue(ref IsStopAction);
    }
}

/// <summary>
/// 遊戲地形資料
/// </summary>
public struct GameTerrainData : INetworkSerializable, IEquatable<GameTerrainData>
{
    // 已移除的可擊破物件Id
    public ulong RemoveBreakObstacleId;

    public bool Equals(GameTerrainData other)
    {
        return RemoveBreakObstacleId.Equals(other.RemoveBreakObstacleId);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            RemoveBreakObstacleId);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref RemoveBreakObstacleId);
    }
}