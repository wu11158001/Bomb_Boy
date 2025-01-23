using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyView : MonoBehaviour
{
    [Header("按鈕")]
    [SerializeField] Button Create_Btn;
    [SerializeField] Button QuickJoin_Btn;

    [Space(30)]
    [Header("房間列表")]
    [SerializeField] RectTransform ListRoomItemNode;
    [SerializeField] GameObject ListRoomItemSample;

    [Space(30)]
    [Header("大廳玩家列表")]
    [SerializeField] RectTransform ListLobbyPlayersNode;
    [SerializeField] GameObject ListLobbyPlayerItemSample;

    private ObjPool _objPool;

    private void OnDestroy()
    {
        CancelInvoke(nameof(RefreshMainLobby));
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(RefreshMainLobby));
    }

    private void Awake()
    {
        _objPool = new ObjPool(transform);
        ListLobbyPlayerItemSample.gameObject.SetActive(false);
    }

    private void Start()
    {
        EventListener();
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(RefreshMainLobby), 1.5f, 5);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 創建房間按鈕
        Create_Btn.onClick.AddListener(() =>
        {
        });

        // 快速加入按鈕
        QuickJoin_Btn.onClick.AddListener(() =>
        {
        });
    }

    /// <summary>
    /// 刷新主大廳
    /// </summary>
    private async void RefreshMainLobby()
    {
        if (LobbyManager.I.JoinedMainLobby == null)
        {
            Debug.LogError($"刷新主大廳錯誤, 未加入!");
            return;
        }

        Lobby mainLobby = await LobbyManager.I.QueryLobbiesAsync(LobbyManager.I.JoinedMainLobby.Id);

        if (mainLobby != null && 
            gameObject.activeSelf)
        {
            List<GameObject> roomItems = _objPool.GetObjList(ListLobbyPlayerItemSample);
            foreach (var item in roomItems)
            {
                item.SetActive(false);
            }

            foreach (var player in mainLobby.Players)
            {
                ListLobbyPlayerItem listLobbyPlayerItem = _objPool.CreateObj<ListLobbyPlayerItem>(ListLobbyPlayerItemSample, ListLobbyPlayersNode);
                listLobbyPlayerItem.SetListLobbyPlayerItem(player);
            }
            Utils.I.SetGridLayoutSize(ListLobbyPlayersNode, false, 1);

            ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.LoadingView);
        }       
    }
}
