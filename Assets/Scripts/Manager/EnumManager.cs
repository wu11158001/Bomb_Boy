/// <summary>
/// Layer名稱列表
/// </summary>
public enum LayerNameEnum
{
    Character = 0,          // 角色
    Ground,                 // 地板
    NotCollision,           // 不碰撞物
    Obstacle,               // 障礙物
    BreakObstacle,          // 可擊破物
    Explosion,              // 爆炸
    Bomb,                   // 炸彈
}

/// <summary>
/// 掉落道具列表
/// </summary>
public enum DropPropsEnum
{
    BombIncreaseProps = 0,      // 炸彈數量增加道具
    PowerProps,                 // 爆炸等級強化道具
    SpeedProps,                 // 移動速度強化道具
}