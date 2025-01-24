#region 遊戲
/// <summary>
/// Layer名稱列表
/// </summary>
public enum LayerNameEnum
{
    // 角色
    Character = 0,
    // 地板
    Ground,
    // 不碰撞物
    NotCollision,
    // 障礙物
    Obstacle,
    // 可擊破物
    BreakObstacle,
    // 爆炸
    Explosion,
    // 炸彈
    Bomb,
}

/// <summary>
/// 掉落道具列表
/// </summary>
public enum DropPropsEnum
{
    // 炸彈數量增加道具
    BombIncreaseProps = 0,
    // 爆炸等級強化道具
    PowerProps,
    // 移動速度強化道具
    SpeedProps,
}
#endregion

#region 場景
/// <summary>
/// 場景列表
/// </summary>
public enum SceneEnum
{
    // 入口
    Entry = 0,
    // 大廳
    Lobby,
    // 遊戲
    Game,
}
#endregion

#region 介面
// 一般介面
public enum ViewEnum
{
    // 大廳
    LobbyView,
}

/// <summary>
/// 常駐介面列表
/// </summary>
public enum PermanentViewEnum
{
    // 載入介面
    LoadingView = 0,
}
#endregion

#region Lobby
/// <summary>
/// Lobby玩家資料Kry
/// </summary>
public enum LobbyPlayerDataKey
{
    // Relay加入代碼
    RelayJoinCode = 0,
}

#endregion

#region 語言
/// <summary>
/// 語言配置表列表
/// </summary>
public enum LocalizationTableEnum
{
    Entry_Table = 0,                // 入口
    Lobby_Table,                    // 大廳
}
#endregion