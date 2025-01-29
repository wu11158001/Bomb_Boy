using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class LobbyView : MonoBehaviour
{
    [Header("按鈕")]
    [SerializeField] Button Leave_Btn;
    [SerializeField] Button PrepareOrStart_Btn;
    [SerializeField] TextMeshProUGUI PrepareOrStartBtn_Txt;

    [Space(30)]
    [Header("玩家項目")]
    [SerializeField] RectTransform LobbyPlayerArea;
    [SerializeField] GameObject LobbyPlayerItemSample;

    // 大廳玩家項目
    private LobbyPlayerItem[] _lobbyPlayerItem_Array = new LobbyPlayerItem[4];

    // 是否首次進入
    private bool _isFirstUpdated;

    private void Awake()
    {
        ViewManager.I.ResetViewData();
    }

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
        UpdateListPlayerItem();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 離開按鈕
        Leave_Btn.onClick.AddListener(async () =>
        {
            ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
            await LobbyManager.I.LeaveLobby();
            ChangeSceneManager.I.ChangeScene(SceneEnum.Entry);
        });

        // 準備 / 開始按鈕
        PrepareOrStart_Btn.onClick.AddListener(async () =>
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    /*是Host*/

                    /*if (NetworkManager.Singleton.ConnectedClients.Count < 2)
                    {
                        Debug.Log("遊戲人數未滿2人");
                        return;
                    }*/

                    bool isAllPrepare = true;
                    foreach (var playerData in LobbyRpcManager.I.LobbyPlayerData_List)
                    {
                        if (playerData.IsPrepare == false &&
                            !playerData.IsGameHost)
                        {
                            isAllPrepare = false;
                            break;
                        }
                    }

                    if (isAllPrepare)
                    {
                        LobbyRpcManager.I.ReadyInGameServerRpc();

                        // 更新房間狀態
                        await LobbyManager.I.UpdateLobbyData(new Dictionary<string, DataObject>()
                        {
                            {$"{LobbyDataKey.State}", new DataObject(DataObject.VisibilityOptions.Public, $"{LobbyDataKey.In_Game}", DataObject.IndexOptions.S1) },
                        });
                        ChangeSceneManager.I.ChangeScene_Network(SceneEnum.Game);
                    }
                    else
                    {
                        Debug.Log("有玩家未準備");
                    }
                }
                else
                {
                    /*一般玩家*/

                    LobbyPlayerData playerData = LobbyRpcManager.I.GetLobbyPlayerData(NetworkManager.Singleton.LocalClientId);
                    playerData.IsPrepare = !playerData.IsPrepare;
                    LobbyRpcManager.I.UpdateLobbyPlayerServerRpc(playerData);
                }
            }
        });
    }

    /// <summary>
    /// 更新玩家項目列表
    /// </summary>
    public void UpdateListPlayerItem()
    {
        // 準備/開始按鈕文字
        string prepareOrStartBtnText =
            NetworkManager.Singleton.IsHost ?
            "Start" :
            "Prepare";
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, prepareOrStartBtnText, (text) =>
        {
            PrepareOrStartBtn_Txt.text = text;
        });

        // 大廳玩家項目初始化
        foreach (var lobbyPlayerItem in _lobbyPlayerItem_Array)
        {
            lobbyPlayerItem.InitializeLobbyPlayerItem();
        }

        // 設置大廳玩家項目資料
        NetworkList<LobbyPlayerData> playerData_List = LobbyRpcManager.I.LobbyPlayerData_List;
        for (int i = 0; i < playerData_List.Count; i++)
        {
            _lobbyPlayerItem_Array[i].UpdateLobbyPlayerItem(playerData_List[i]);
        }

        if (!_isFirstUpdated && playerData_List.Count > 0)
        {
            /*首次更新*/

            _isFirstUpdated = true;
            ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.LoadingView);
        }        
    }
    
    /// <summary>
    /// 準備進入遊戲
    /// </summary>
    public void ReadyInGame()
    {
        ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
    }
}
