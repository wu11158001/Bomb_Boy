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
    public Lobby JoinedLobby { get; private set; }

    private void OnDestroy()
    {
        CancelInvoke(nameof(HandleLobbyHeartbeat));
    }

    private void Start()
    {
        InvokeRepeating(nameof(HandleLobbyHeartbeat), 15, 15);
    }

    /// <summary>
    /// 創建大廳
    /// </summary>
    /// <returns></returns>
    public async Task CreateLobby()
    {
        try
        {
            // 大廳人數
            int maxPlayer = 4;
            // 本地玩家暱稱
            string recodeNickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY);
            // 本地玩家Id
            string id = AuthenticationService.Instance.PlayerId;

            // 創建Relay
            string relayJoinCode = await RelayManager.I.CreateRelay(maxPlayer - 1);

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

            JoinedLobby = await LobbyService.Instance.CreateLobbyAsync(id, maxPlayer, createLobbyOptions);
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

            // 加入Lobby
            JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(joinLobby.Id);
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

            // 加入Relay
            string relayJoinCode = JoinedLobby.Data[$"{LobbyPlayerDataKey.RelayJoinCode}"].Value;
            await RelayManager.I.JoinRelay(relayJoinCode);

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
        try
        {
            if (JoinedLobby != null)
            {
                NetworkManager.Singleton.Shutdown(true);
                await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);

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
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"更換 Lobby Host 錯誤: {e}");
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
}
