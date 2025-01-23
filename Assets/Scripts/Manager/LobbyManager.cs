using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Unity.Netcode;

public class LobbyManager : UnitySingleton<LobbyManager>
{
    // 主大廳
    public Lobby JoinedMainLobby { get; private set; }
    // 加入的房間
    public Lobby JoinedRoomLobby { get; private set; }

    private void Start()
    {
        InvokeRepeating(nameof(HandleMainLobbyHeartbeat), 10, 10);
    }

    #region 主大廳
    /// <summary>
    /// 查詢已加入的主大廳資料
    /// </summary>
    /// <param name="joinedMainLobbyId"></param>
    /// <returns></returns>
    public async Task<Lobby> QueryLobbiesAsync(string joinedMainLobbyId)
    {
        try
        {
            // 篩選排序
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Order = new()
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            foreach (var mainLobby in queryResponse.Results)
            {
                if (mainLobby.Id == joinedMainLobbyId)
                {
                    return mainLobby;
                }
            }

            Debug.LogError("查詢已加入的主大廳資料錯誤");
            return null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"查詢已加入的主大廳資料錯誤: {e}");
            return null;
        }
    }

    /// <summary>
    /// 查詢可加入的主大廳列表
    /// </summary>
    public async Task<QueryResponse> QueryLobbiesAsync()
    {
        try
        {
            // 篩選排序
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Filters = new List<QueryFilter>()
                {
                    // 大廳類型=主大廳
                    new QueryFilter(QueryFilter.FieldOptions.S1, $"{LobbyTypeEnum.MainLobby}", QueryFilter.OpOptions.EQ),
                    // 剩餘空位數 > 10
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "10", QueryFilter.OpOptions.GT),
                },
                Order = new()
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            // 查詢房間
            return await LobbyService.Instance.QueryLobbiesAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"查詢可加入的主大廳列表錯誤: {e}");
            return null;
        }
    }

    /// <summary>
    /// 創建主大廳
    /// </summary>
    public async Task CreateMainLobby()
    {
        try
        {
            // 最大玩家人數
            int maxPlayer = 100;
            // 本地玩家暱稱
            string recodeNickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY);

            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Player = new Player()
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        // 玩家暱稱
                        { $"{LobbyPlayerDataKey.PlayerNickname}", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, recodeNickname)},
                        // 遊戲狀態
                        { $"{LobbyPlayerDataKey.GameState}", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, $"{LobbyPlayerDataKey.Online}")},
                    },
                },
                Data = new Dictionary<string, DataObject>()
                {
                    { "LobbyType", new DataObject(DataObject.VisibilityOptions.Public, $"{LobbyTypeEnum.MainLobby}")},
                },
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("MainLobby", maxPlayer, createLobbyOptions);
            JoinedMainLobby = lobby;

            Debug.Log($"創建主大廳, LobbyId: {lobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"創建主大廳錯誤: {e}");
        }
    }

    /// <summary>
    /// 加入主大廳
    /// </summary>
    /// <param name="joinLobby"></param>
    public async Task JoinMainLobby(Lobby joinLobby)
    {
        try
        {
            // 本地玩家暱稱
            string recodeNickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY);

            JoinLobbyByIdOptions joinLobbyByIdOptions = new()
            {
                Player = new Player()
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        // 玩家暱稱
                        { $"{LobbyPlayerDataKey.PlayerNickname}", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, recodeNickname)},
                        // 遊戲狀態
                        { $"{LobbyPlayerDataKey.GameState}", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, $"{LobbyPlayerDataKey.Online}")},
                    },
                },
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(joinLobby.Id, joinLobbyByIdOptions);
            JoinedMainLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"加入主大廳錯誤: {e}");
        }
    }
    /// <summary>
    /// 處理主大廳心跳
    /// </summary>
    public async void HandleMainLobbyHeartbeat()
    {
        if (JoinedMainLobby != null &&
            JoinedMainLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(JoinedMainLobby.Id);
        }
    }
    #endregion
}
