using UnityEngine;

public class GameDataManager : UnitySingleton<GameDataManager>
{
    // 最大遊戲人數
    public const int MaxPlayer = 4;
    // 產生場景物件偏移量
    public readonly Vector3 CreateSceneObjectOffset = new(0.4f, 0, -0.4f);
    // 射線Size
    public readonly Vector3 PhysicsSize = new(0.5f, 1.5f, 0.5f);
    // 下個地板位置距離
    public const float NextGroundDistance = 1.6f;
    // 遊戲時間
    public const int GameTime = 180;
}

/// <summary>
/// 聊天資料
/// </summary>
public class ChatData
{
    // 登入Id
    public string AuthenticationPlayerId;
    // 暱稱
    public string Nickname;
    // 聊天訊息
    public string ChatMsg;
}