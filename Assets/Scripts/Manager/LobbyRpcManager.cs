using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Newtonsoft.Json;
using Unity.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Relay;

public class LobbyRpcManager : NetworkBehaviour
{
    private static LobbyRpcManager _instance;
    public static LobbyRpcManager I { get { return _instance; } }

    public NetworkList<LobbyPlayerData> LobbyPlayerData_List { get; private set; }
        = new(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private LobbyView _lobbyView;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this);
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log("退出 Lobby Rpc");

        // 非手動退出大廳 && 非交換房主
        if (!LobbyManager.I.IsSelfLeaveLobby &&
            !LobbyManager.I.IsMigrateLobbyHost)
        {
            LobbyManager.I.LeaveLobby();
        }

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        LobbyPlayerData_List.OnListChanged -= OnLobbyPlayerDataChange;

        LobbyManager.I.IsSelfLeaveLobby = false;
        LobbyManager.I.IsMigrateLobbyHost = false;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("加入 Lobby Rpc");

        // 事件註冊
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        LobbyPlayerData_List.OnListChanged += OnLobbyPlayerDataChange;

        if (IsServer)
        {
            LobbyPlayerData_List.Clear();
        }

        // 新增大廳玩家
        AddLobbyPlayerServerRpc(new LobbyPlayerData()
        {
            NetworkClientId = NetworkManager.Singleton.LocalClientId,
            AuthenticationPlayerId = AuthenticationService.Instance.PlayerId,
            JoinLobbyId = AuthenticationService.Instance.PlayerId,
            Nickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY),
            IsPrepare = false,
        });
    }

    /// <summary>
    /// 玩家斷線事件
    /// </summary>
    /// <param name="networkClientId"></param>
    private void OnClientDisconnect(ulong networkClientId)
    {
        Debug.Log($"有玩家斷線: {networkClientId} / 本地: {NetworkManager.Singleton.LocalClientId}");

        LobbyPlayerData lobbyPlayerData = GetLobbyPlayerData(networkClientId);
        if (lobbyPlayerData.NetworkClientId == NetworkManager.Singleton.LocalClientId)
        {
            LobbyManager.I.IsMigrateLobbyHost = true;
        }        

        if (IsServer)
        {
            RemoveLobbyPlayerServerRpc(networkClientId);

            if (SceneManager.GetActiveScene().name == $"{SceneEnum.Game}")
            {
                // 判斷遊戲結果
                GameRpcManager.I.JudgeGameResultServerRpc();
            }
        }
    }

    /// <summary>
    /// 大廳玩家資料變更事件
    /// </summary>
    /// <param name="changeEvent"></param>
    private void OnLobbyPlayerDataChange(NetworkListEvent<LobbyPlayerData> changeEvent)
    {
        // 更新大廳介面
        UpdateLobbyView();
    }

    /// <summary>
    /// (Server)新增大廳玩家
    /// </summary>
    /// <param name="newPlayer"></param>
    [ServerRpc(RequireOwnership = false)]
    public void AddLobbyPlayerServerRpc(LobbyPlayerData newPlayer)
    {
        Debug.Log($"新增房間玩家: {newPlayer.NetworkClientId} / 暱稱: {newPlayer.Nickname}");
        LobbyPlayerData_List.Add(newPlayer);
    }

    /// <summary>
    /// (Server)移除大廳玩家
    /// </summary>
    /// <param name="networkClientId"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RemoveLobbyPlayerServerRpc(ulong networkClientId)
    {
        if (IsServer)
        {
            LobbyPlayerData lobbyPlayerData = GetLobbyPlayerData(networkClientId);
            if (LobbyPlayerData_List.Contains(lobbyPlayerData))
            {
                LobbyPlayerData_List.Remove(lobbyPlayerData);
                Debug.Log($"移除大廳玩家資料: {networkClientId}");
            }
            else
            {
                Debug.LogError($"移除大廳玩家資料錯誤: {networkClientId}");
            }
        }
    }

    /// <summary>
    /// (Server)踢除玩家
    /// </summary>
    /// <param name="networkClientId"></param>
    [ServerRpc]
    public void KickLobbyPlayerServerRpc(ulong networkClientId)
    {
        KickLobbyPlayerClientRpc(networkClientId);
    }

    /// <summary>
    /// (Client)踢除玩家
    /// </summary>
    [ClientRpc]
    private void KickLobbyPlayerClientRpc(ulong networkClientId)
    {
        if (networkClientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("被踢出大廳");
            OnLeaveLobby(true);
        }
    }

    /// <summary>
    /// (Server)更新大廳玩家資料
    /// </summary>
    /// <param name="updatePlayerData"></param>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateLobbyPlayerServerRpc(LobbyPlayerData updatePlayerData)
    {
        for (int i = 0; i < LobbyPlayerData_List.Count; i++)
        {
            if (LobbyPlayerData_List[i].NetworkClientId == updatePlayerData.NetworkClientId)
            {
                LobbyPlayerData_List[i] = updatePlayerData;
                return;
            }
        }

        Debug.LogError($"玩家: {updatePlayerData.NetworkClientId} 更新大廳玩家資料錯誤");
    }

    /// <summary>
    /// (Server)準備進入遊戲
    /// </summary>
    [ServerRpc]
    public void ReadyInGameServerRpc()
    {
        ReadyInGameClientRpc();
        GameRpcManager.I.InitializeGameRpcManager();
    }

    /// <summary>
    /// (Client)準備進入遊戲
    /// </summary>
    [ClientRpc]
    private void ReadyInGameClientRpc()
    {
        CheckLobbyView();
        if (_lobbyView != null && _lobbyView.gameObject.activeSelf)
        {
            _lobbyView.ReadyInGame();
        }
    }

    /// <summary>
    /// (Server)重製大廳玩家資料
    /// </summary>
    [ServerRpc]
    public void ResetLobbyPlayerDataServerRpc()
    {
        foreach (var lobbyPlayerData in LobbyPlayerData_List)
        {
            LobbyPlayerData data = lobbyPlayerData;
            data.IsPrepare = false;
            UpdateLobbyPlayerServerRpc(data);
        }
    }

    /// <summary>
    /// 獲取大廳玩家資料
    /// </summary>
    /// <param name="networkClientId"></param>
    /// <returns></returns>
    public LobbyPlayerData GetLobbyPlayerData(ulong networkClientId)
    {
        for (int i = 0; i < LobbyPlayerData_List.Count; i++)
        {
            if (LobbyPlayerData_List[i].NetworkClientId == networkClientId)
            {
                return LobbyPlayerData_List[i];
            }
        }

        Debug.LogError($"玩家: {networkClientId} 獲取玩家資料錯誤");
        return new();
    }

    /// <summary>
    /// 檢測大廳介面
    /// </summary>
    /// <returns></returns>
    private void CheckLobbyView()
    {
        if (_lobbyView == null)
        {
            GameObject lobbyViewObj = GameObject.Find("LobbyView");
            if (lobbyViewObj != null)
            {
                _lobbyView = lobbyViewObj.GetComponent<LobbyView>();
            }
        }
    }

    /// <summary>
    /// 更新大廳介面
    /// </summary>
    public void UpdateLobbyView()
    {
        CheckLobbyView();
        if (_lobbyView != null && _lobbyView.gameObject.activeSelf)
        {
            _lobbyView.UpdateListPlayerItem();
        }
    }

    /// <summary>
    /// 離開大廳
    /// </summary>
    /// <param name="isKicked">是否是被踢出</param>
    public async void OnLeaveLobby(bool isKicked = false)
    {       
        ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);

        if (isKicked)
        {
            LanguageManager.I.GetString(LocalizationTableEnum.TipMessage_Table, "Kicked out of the lobby", (text) =>
            {
                ViewManager.I.OpenPermanentView<TipMessageView>(PermanentViewEnum.TipMessageView, (view) =>
                {
                    view.ShowTipMessage(text);
                });
            });
        }

        ChangeSceneManager.I.ChangeScene(SceneEnum.Entry);
        await LobbyManager.I.LeaveLobby();
    }
}
