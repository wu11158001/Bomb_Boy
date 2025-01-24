using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GameView : MonoBehaviour
{
    private void Awake()
    {
        ViewManager.I.ResetViewData();
    }

    private void Start()
    {
        GameRpcManager.I.InGameSceneServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// 遊戲開始
    /// </summary>
    public void GameStart()
    {
        ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.LoadingView);
    }
}
