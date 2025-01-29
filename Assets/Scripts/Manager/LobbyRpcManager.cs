using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyRpcManager : NetworkBehaviour
{
    private static LobbyRpcManager _instance;
    public static LobbyRpcManager I { get { return _instance; } }

    public NetworkList<LobbyPlayerData> LobbyPlayerData_List { get; private set; }
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
        LobbyPlayerData_List.OnListChanged -= OnLobbyPlayerDataChange;
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            LobbyPlayerData_List.Clear();
        }

        // 事件註冊
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        LobbyPlayerData_List.OnListChanged += OnLobbyPlayerDataChange;

        // 新增大廳玩家
        AddLobbyPlayerServerRpc(new()
        {
            NetworkClientId = NetworkManager.Singleton.LocalClientId,
            Nickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY),
            IsGameHost = NetworkManager.Singleton.IsHost,
            IsPrepare = false,
        });
    }

    /// <summary>
    /// 玩家斷線事件
    /// </summary>
    /// <param name="networkClientId"></param>
    private void OnClientDisconnect(ulong networkClientId)
    {
        Debug.Log($"有玩家斷線: {networkClientId}");

        RemoveLobbyPlayerServerRpc(networkClientId);

        if (IsServer)
        {
            if (SceneManager.GetActiveScene().name == $"{SceneEnum.Game}")
            {
                // 判斷遊戲結果
                GameRpcManager.I.JudgeGameResultServerRpc();
            }
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
    private void OnLobbyPlayerDataChange(NetworkListEvent<LobbyPlayerData> changeEvent)
    {
        // 大廳更新
        CheckLobbyView();
        if (_lobbyView != null && _lobbyView.gameObject.activeSelf)
        {
            _lobbyView.UpdateListPlayerItem();
        }
    }

    /// <summary>
    /// (Server)新增大廳玩家
    /// </summary>
    /// <param name="newPlayer"></param>
    [ServerRpc(RequireOwnership = false)]
    public void AddLobbyPlayerServerRpc(LobbyPlayerData newPlayer)
    {
        Debug.Log($"新增房間玩家: {newPlayer.NetworkClientId}");
        LobbyPlayerData_List.Add(newPlayer);
    }

    /// <summary>
    /// (Server)移除大廳玩家
    /// </summary>
    /// <param name="networkClientId"></param>
    [ServerRpc(RequireOwnership = false)]
    public void RemoveLobbyPlayerServerRpc(ulong networkClientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            for (int i = LobbyPlayerData_List.Count - 1; i >= 0; i--)
            {
                if (LobbyPlayerData_List[i].NetworkClientId == networkClientId)
                {
                    Debug.Log($"移除大廳玩家 {networkClientId}");
                    LobbyPlayerData_List.Remove(LobbyPlayerData_List[i]);
                    return;
                }
            }

            Debug.LogError($"移除大廳玩家錯誤: {networkClientId}");
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
}
