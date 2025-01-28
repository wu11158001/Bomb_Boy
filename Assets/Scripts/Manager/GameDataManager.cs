using UnityEngine;

public class GameDataManager : UnitySingleton<GameDataManager>
{
    // 產生場景物件偏移量
    public readonly Vector3 CreateSceneObjectOffset = new(0.4f, 0, -0.4f);
    // 射線Size
    public readonly Vector3 PhysicsSize = new(0.5f, 1.5f, 0.5f);
    // 下個地板位置距離
    public const float NextGroundDistance = 1.6f;
}
