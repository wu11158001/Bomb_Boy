using UnityEngine;

public class GameDataManager : UnitySingleton<GameDataManager>
{
    // 產生場景物件偏移量
    public Vector3 CreateSceneObjectOffset = new(0.4f, -1.0f, -0.4f);
}
