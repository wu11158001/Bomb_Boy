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
    // 加入的Lobby
    public Lobby JoinedLobby { get; private set; }
    // 當前加入Lobby Host Id
    public string CurrLobbyHostId { get; private set; }
    // 當前加入Relay Join Code
    public string CurrRelayJoinCode { get; private set; }

    // 是否是手動退出大廳
    public bool IsSelfLeaveLobby { get; set; }
    // 是否正在交換房主
    public bool IsMigrateLobbyHost { get; set; }

    private void OnDestroy()
    {
        CancelInvoke(nameof(HandleLobbyHeartbeat));
        CancelInvoke(nameof(RefreshRoom));
    }

    private void Start()
    {
        InvokeRepeating(nameof(HandleLobbyHeartbeat), 15, 15);
    }

    /// <summary>
    /// 斷線重連查詢大廳
    /// </summary>
    /// <param name="lobbyId"></param>
    /// <returns></returns>
    public async Task<Lobby> ReconnectQueryLobby(string lobbyId)
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Filters = new List<QueryFilter>()
                {
                    {new QueryFilter( QueryFilter.FieldOptions.S1, $"{LobbyDataKey.In_Game}", QueryFilter.OpOptions.EQ) },
                },
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            for (int i = 0; i < queryResponse.Results.Count; i++)
            {
                if (queryResponse.Results[i].Id == lobbyId)
                {
                    return queryResponse.Results[i];
                }
            }

            return null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"查詢大廳錯誤: {e}");
            return null;
        }
    }

    /// <summary>
    /// 創建大廳
    /// </summary>
    /// <returns></returns>
    public async Task CreateLobby()
    {
        try
        {
            // 本地玩家暱稱
            string recodeNickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY);
            // 本地玩家Id
            string id = AuthenticationService.Instance.PlayerId;

            // 創建Relay
            string relayJoinCode = await RelayManager.I.CreateRelay(GameDataManager.MaxPlayer - 1);
            CurrRelayJoinCode = relayJoinCode;

            // 創建Lobby
            CreateLobbyOptions createLobbyOptions = new()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    // Relay加入代碼
                    { $"{LobbyPlayerDataKey.RelayJoinCode}", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode)},
                    // 房間狀態
                    { $"{LobbyDataKey.State}", new DataObject(DataObject.VisibilityOptions.Public, $"{LobbyDataKey.In_Team}", DataObject.IndexOptions.S1)},
                },
            };
            
            JoinedLobby = await LobbyService.Instance.CreateLobbyAsync(id, GameDataManager.MaxPlayer, createLobbyOptions);
            CurrLobbyHostId = JoinedLobby.HostId;

            await JoinVivox();

            InvokeRepeating(nameof(RefreshRoom), 1.1f, 1.1f);
            Debug.Log($"創建大廳, LobbyId: {JoinedLobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"創建大廳錯誤: {e}");
        }
    }

    /// <summary>
    /// 加入大廳
    /// </summary>
    /// <param name="joinLobby"></param>
    public async Task JoinLobby(Lobby joinLobby)
    {
        try
        {
            // 本地玩家暱稱
            string recodeNickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY);
            // Relay加入代碼
            string relayJoinCode = joinLobby.Data[$"{LobbyPlayerDataKey.RelayJoinCode}"].Value;

            // 加入Relay
            await RelayManager.I.JoinRelay(relayJoinCode);
            CurrRelayJoinCode = relayJoinCode;

            // 加入Lobby
            JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(joinLobby.Id);
            CurrLobbyHostId = JoinedLobby.HostId;

            await JoinVivox();

            InvokeRepeating(nameof(RefreshRoom), 1.1f, 1.1f);
            Debug.Log($"加入大廳, LobbyId: {JoinedLobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"加入大廳錯誤: {e}");
        }
    }

    /// <summary>
    /// 快速加入大廳
    /// </summary>
    /// <returns></returns>
    public async Task QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new()
            {
                Filter = new List<QueryFilter>()
                {
                    {new QueryFilter( QueryFilter.FieldOptions.S1, $"{LobbyDataKey.In_Team}", QueryFilter.OpOptions.EQ) },
                },
            };

            // 快速加入Lobby
            JoinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            CurrLobbyHostId = JoinedLobby.HostId;

            // 加入Relay
            string relayJoinCode = JoinedLobby.Data[$"{LobbyPlayerDataKey.RelayJoinCode}"].Value;
            await RelayManager.I.JoinRelay(relayJoinCode);
            CurrRelayJoinCode = relayJoinCode;

            await JoinVivox();

            InvokeRepeating(nameof(RefreshRoom), 1.1f, 1.1f);
            Debug.Log($"快速加入大廳, LobbyId: {JoinedLobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"快速加入大廳失敗: {e}");

            await CreateLobby();
        }
    }

    /// <summary>
    /// 離開大廳
    /// </summary>
    public async Task LeaveLobby()
    {
        CancelInvoke(nameof(RefreshRoom));

        try
        {
            if (JoinedLobby != null)
            {
                NetworkManager.Singleton.Shutdown(false);
                await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);

                await VivoxManager.I.LeaveEchoChannelAsync();

                JoinedLobby = null;

                Debug.Log("離開大廳");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"離開大廳錯誤: {e}");
        }
    }

    /// <summary>
    /// 處理房間心跳
    /// </summary>
    public async void HandleLobbyHeartbeat()
    {
        if (JoinedLobby != null &&
            JoinedLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
        }
    }

    /// <summary>
    /// 更新大廳資料
    /// </summary>
    /// <param name="dataDic"></param>
    public async Task UpdateLobbyData(Dictionary<string, DataObject> dataDic)
    {
        try
        {
            if (!IsLobbyHost()) return;

            JoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions()
            {
                Data = dataDic,
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"更新大廳資料: {e}");
        }
    }

    /// <summary>
    /// 是否是Lobby Host
    /// </summary>
    /// <returns></returns>
    public bool IsLobbyHost()
    {
        if (JoinedLobby != null)
        {
            return JoinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        return false;
    }

    /// <summary>
    /// 轉讓Lobby Host
    /// </summary>
    /// <param name="playerId"></param>
    public async void MigrateLobbyHost(string playerId)
    {
        try
        {
            if (!IsLobbyHost()) return;

            JoinedLobby = await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions()
            {
                HostId = playerId,
            });

            Debug.Log($"轉讓Lobby Host給: {playerId}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"更換 Lobby Host 錯誤: {e}");
        }
    }

    /// <summary>
    /// 刷新房間
    /// </summary>
    public async void RefreshRoom()
    {
        if (JoinedLobby == null) return;

        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
            JoinedLobby = lobby;
            string relayJoinCode = JoinedLobby.Data[$"{LobbyPlayerDataKey.RelayJoinCode}"].Value;

            if (JoinedLobby == null) return;

            // Lobby Host更改 Relay重新連接
            if (CurrLobbyHostId != lobby.HostId)
            {
                Debug.Log("房主更換!");
                CurrLobbyHostId = lobby.HostId;

                if (IsLobbyHost())
                {
                    ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.ReconnectView);
                    CancelInvoke(nameof(RefreshRoom));

                    NetworkManager.Singleton.Shutdown(true);
                    relayJoinCode = await RelayManager.I.CreateRelay(GameDataManager.MaxPlayer - 1);
                    CurrRelayJoinCode = relayJoinCode;

                    await UpdateLobbyData(new Dictionary<string, DataObject>()
                    {
                        { $"{LobbyPlayerDataKey.RelayJoinCode}", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode)},
                    });

                    Debug.Log($"更新Lobyy資料:Relay Join Code 更換 {relayJoinCode}");

                    InvokeRepeating(nameof(RefreshRoom), 1.1f, 1.1f);
                    Debug.Log($"接收房主, 創建Relay: {relayJoinCode}");
                }               
            }

            if (CurrRelayJoinCode != relayJoinCode)
            {
                Debug.Log($"Relay更換, {CurrRelayJoinCode} => {relayJoinCode}");
                ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.ReconnectView);
                CancelInvoke(nameof(RefreshRoom));

                NetworkManager.Singleton.Shutdown(true);
                await RelayManager.I.JoinRelay(relayJoinCode);
                CurrRelayJoinCode = relayJoinCode;

                InvokeRepeating(nameof(RefreshRoom), 1.1f, 1.1f);
                Debug.Log($"退出Relay重新連接: {relayJoinCode}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"刷新房間錯誤:{e}");
        }
    }

    /// <summary>
    /// 加入Vivox
    /// </summary>
    /// <returns></returns>
    private async Task JoinVivox()
    {
        await VivoxManager.I.LoginToVivoxAsync();
        await VivoxManager.I.JoinGroupChannelAsync(JoinedLobby.Id);
    }
}
