using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class GameView : MonoBehaviour
{
    [SerializeField] Button ExitGame_Btn;
    [SerializeField] TextMeshProUGUI GameResult_Txt;
    [SerializeField] TextMeshProUGUI GameStartCD_Txt;
    [SerializeField] TextMeshProUGUI BackLobbyCD_Txt;

    private void Awake()
    {
        ViewManager.I.ResetViewData();
    }

    private void Start()
    {
        GameRpcManager.I.InGameSceneServerRpc(NetworkManager.Singleton.LocalClientId);

        ExitGame_Btn.gameObject.SetActive(false);
        GameResult_Txt.gameObject.SetActive(false);
        GameStartCD_Txt.gameObject.SetActive(false);
        BackLobbyCD_Txt.gameObject.SetActive(false);

        EventListener();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 退出遊戲按鈕
        ExitGame_Btn.onClick.AddListener(async () =>
        {
            ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
            await LobbyManager.I.LeaveLobby();
            ChangeSceneManager.I.ChangeScene(SceneEnum.Entry);
        });
    }

    /// <summary>
    /// 遊戲開始
    /// </summary>
    public void GameStart()
    {
        ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.LoadingView);
    }

    /// <summary>
    /// 顯示死亡介面
    /// </summary>
    public void ShowDieView()
    {
        ExitGame_Btn.gameObject.SetActive(true);
    }
}
