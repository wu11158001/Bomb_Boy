using UnityEngine;
using Unity.Netcode;

public class LobbyRpcManager : NetworkBehaviour
{
    private static LobbyRpcManager _instance;
    public static LobbyRpcManager I { get { return _instance; } }

    public NetworkList<PlayerData> PlayerData_List { get; private set; }
        = new(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        PlayerData_List.OnListChanged -= OnLobbyPlayerDataChange;
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            PlayerData_List.Clear();
        }

        // 事件註冊
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        PlayerData_List.OnListChanged += OnLobbyPlayerDataChange;

        // 新增大廳玩家
        AddLobbyPlayerServerRpc(new()
        {
            NetworkClientId = NetworkManager.Singleton.LocalClientId,
            Nickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY),
            IsGameHost = NetworkManager.Singleton.IsHost,
            IsPrepare = false,
            IsInGameScene = false
        });
    }

    /// <summary>
    /// 玩家斷線事件
    /// </summary>
    /// <param name="networkClientId"></param>
    private void OnClientDisconnect(ulong networkClientId)
    {
        Debug.Log($"有玩家斷線: {networkClientId}");
        if (NetworkManager.Singleton.IsServer)
        {
            RemoveLobbyPlayerServerRpc(networkClientId);
        }

        if (NetworkManager.Singleton.LocalClientId == networkClientId)
        {
            /*離開的是本地端*/
            return;
        }
    }

    /// <summary>
    /// 大廳玩家資料變更事件
    /// </summary>
    /// <param name="changeEvent"></param>
    private void OnLobbyPlayerDataChange(NetworkListEvent<PlayerData> changeEvent)
    {
        CheckLobbyView();
        if (_lobbyView != null && _lobbyView.gameObject.activeSelf)
        {
            _lobbyView.UpdateListPlayerItem();
        }
    }

    /// <summary>
    /// 新增大廳玩家
    /// </summary>
    /// <param name="newPlayer"></param>
    [ServerRpc(RequireOwnership = false)]
    public void AddLobbyPlayerServerRpc(PlayerData newPlayer)
    {
        Debug.Log($"新增房間玩家: {newPlayer.NetworkClientId}");
        PlayerData_List.Add(newPlayer);
    }

    /// <summary>
    /// 移除大廳玩家
    /// </summary>
    /// <param name="networkClientId"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RemoveLobbyPlayerServerRpc(ulong networkClientId)
    {
        for (int i = PlayerData_List.Count; i >= 0; i--)
        {
            if (PlayerData_List[i].NetworkClientId == networkClientId)
            {
                Debug.Log($"移除大廳玩家 {networkClientId}");
                PlayerData_List.Remove(PlayerData_List[i]);
                return;
            }
        }

        Debug.LogError($"移除大廳玩家錯誤: {networkClientId}");
    }

    /// <summary>
    /// 更新大廳玩家資料
    /// </summary>
    /// <param name="updatePlayerData"></param>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateLobbyPlayerServerRpc(PlayerData updatePlayerData)
    {
        for (int i = 0; i < PlayerData_List.Count; i++)
        {
            if (PlayerData_List[i].NetworkClientId == updatePlayerData.NetworkClientId)
            {
                PlayerData_List[i] = updatePlayerData;
                return;
            }
        }

        Debug.LogError($"玩家: {updatePlayerData.NetworkClientId} 更新房間玩家資料錯誤");
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
    /// 獲取本地大廳玩家資料
    /// </summary>
    /// <param name="networkClientId"></param>
    /// <returns></returns>
    public PlayerData GetLocalLobbyPlayerData(ulong networkClientId)
    {
        for (int i = 0; i < PlayerData_List.Count; i++)
        {
            if (PlayerData_List[i].NetworkClientId == networkClientId)
            {
                return PlayerData_List[i];
            }
        }

        Debug.LogError($"玩家: {networkClientId} 獲取本地大廳玩家資料錯誤");
        return new();
    }

    /// <summary>
    /// 準備進入遊戲
    /// </summary>
    [ServerRpc]
    public void ReadyInGameServerRpc()
    {
        ReadyInGameClientRpc();
    }
    [ClientRpc]
    private void ReadyInGameClientRpc()
    {
        CheckLobbyView();
        if (_lobbyView != null && _lobbyView.gameObject.activeSelf)
        {
            _lobbyView.ReadyInGame();
        }
    }
}
