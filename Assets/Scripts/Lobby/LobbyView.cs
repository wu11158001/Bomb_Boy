using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Vivox;

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

    [Space(30)]
    [Header("聊天區域")]
    [SerializeField] LobbyChatArea _lobbyChatArea;

    [Space(30)]
    [Header("測試")]
    [SerializeField] bool isSinglePlay;

    // 大廳玩家項目
    private LobbyPlayerItem[] _lobbyPlayerItem_Array = new LobbyPlayerItem[4];

    private void Awake()
    {
        ViewManager.I.ResetViewData();
    }

    private void Start()
    {
        AudioManager.I.PlayBGM(BGNEnum.EntryAndLobby);
        PlayerPrefs.SetString(LocalDataKeyManager.LOCAL_JOIN_LOBBY_ID, "");

        // 產生大廳玩家項目
        for (int i = 0; i < 4; i++)
        {
            int playerIndex = i;

            GameObject itemObj = Instantiate(LobbyPlayerItemSample, LobbyPlayerArea);
            itemObj.SetActive(true);
            LobbyPlayerItem lobbyPlayerItem = itemObj.GetComponent<LobbyPlayerItem>();
            lobbyPlayerItem.InitializeLobbyPlayerItem(playerIndex);
            _lobbyPlayerItem_Array[i] = lobbyPlayerItem;
        }
        LobbyPlayerItemSample.SetActive(false);

        EventListener();
        UpdateListPlayerItem();

        if (VivoxService.Instance.IsLoggedIn)
        {
            ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.LoadingView);
        }
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 離開按鈕
        Leave_Btn.onClick.AddListener(() =>
        {
            AudioManager.I.PlaySound(SoundEnum.Cancel);
            if (LobbyManager.I.IsLobbyHost())
            {
                LobbyRpcManager.I.HostLeaveLobbyServerRpc();
            }
            LobbyRpcManager.I.LeaveLobby();
        });

        // 準備 / 開始按鈕
        PrepareOrStart_Btn.onClick.AddListener(async () =>
        {           
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                if (LobbyManager.I.IsLobbyHost())
                {
                    /*是Host*/

                    AudioManager.I.PlaySound(SoundEnum.Confirm);

                    if (!isSinglePlay && NetworkManager.Singleton.ConnectedClients.Count < 2)
                    {
                        Debug.Log("遊戲人數未滿2人");
                        LanguageManager.I.GetString(LocalizationTableEnum.TipMessage_Table, "The number of players is less than 2", (text) =>
                        {
                            ViewManager.I.OpenPermanentView<TipMessageView>(PermanentViewEnum.TipMessageView, (view) =>
                            {
                                view.ShowTipMessage(text);
                            });
                        });
                        
                        return;
                    }

                    bool isAllPrepare = true;
                    foreach (var playerData in LobbyRpcManager.I.LobbyPlayerData_List)
                    {
                        if (playerData.IsPrepare == false &&
                            LobbyManager.I.JoinedLobby.HostId != playerData.AuthenticationPlayerId)
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
                        LanguageManager.I.GetString(LocalizationTableEnum.TipMessage_Table, "Some players are not prepared", (text) =>
                        {
                            ViewManager.I.OpenPermanentView<TipMessageView>(PermanentViewEnum.TipMessageView, (view) =>
                            {
                                view.ShowTipMessage(text);
                            });
                        });
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
            LobbyManager.I.IsLobbyHost() ?
            "Start" :
            "Prepare";
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, prepareOrStartBtnText, (text) =>
        {
            PrepareOrStartBtn_Txt.text = text;
        });

        // 大廳玩家項目初始化
        int playerIndex = 0;
        foreach (var lobbyPlayerItem in _lobbyPlayerItem_Array)
        {
            lobbyPlayerItem.InitializeLobbyPlayerItem(playerIndex);
            playerIndex++;
        }

        // 設置大廳玩家項目資料
        NetworkList<LobbyPlayerData> playerData_List = LobbyRpcManager.I.LobbyPlayerData_List;
        for (int i = 0; i < playerData_List.Count; i++)
        {
            _lobbyPlayerItem_Array[i].UpdateLobbyPlayerItem(playerData_List[i]);
        }

        ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.ReconnectView);
    }
    
    /// <summary>
    /// 準備進入遊戲
    /// </summary>
    public void ReadyInGame()
    {
        ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
    }

    /// <summary>
    /// 聊天訊息接收
    /// </summary>
    /// <param name="ChatData"></param>
    public void ChatMessageReceived(ChatData chatData)
    {
        _lobbyChatArea.ShowChatMessage(chatData);
    }
}
