using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyView : MonoBehaviour
{
    [Header("按鈕")]
    [SerializeField] Button Leave_Btn;

    [Space(30)]
    [Header("玩家項目")]
    [SerializeField] RectTransform LobbyPlayerArea;
    [SerializeField] GameObject LobbyPlayerItemSample;

    private LobbyPlayerItem[] _lobbyPlayerItem_Array = new LobbyPlayerItem[4];

    private void Start()
    {
        // 產生大廳玩家項目
        for (int i = 0; i < 4; i++)
        {
            GameObject itemObj = Instantiate(LobbyPlayerItemSample, LobbyPlayerArea);
            itemObj.SetActive(true);
            LobbyPlayerItem lobbyPlayerItem = itemObj.GetComponent<LobbyPlayerItem>();
            lobbyPlayerItem.InitializeLobbyPlayerItem();
            _lobbyPlayerItem_Array[i] = lobbyPlayerItem;
        }
        LobbyPlayerItemSample.SetActive(false);

        EventListener();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 離開按鈕
        Leave_Btn.onClick.AddListener(() =>
        {
            ChangeSceneManager.I.ChangeScene(SceneEnum.Entry);
        });
    }
}
