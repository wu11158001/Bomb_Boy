using UnityEngine;
using Unity.Netcode;

public class GameRpcManager : NetworkBehaviour
{
    private static GameRpcManager _instance;
    public static GameRpcManager I { get { return _instance; } }

    private GameView _gameView;

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

    [ServerRpc(RequireOwnership = false)]
    public void InGameSceneServerRpc(ulong networkClientId)
    {
        Debug.Log($"玩家: {networkClientId} 已進入遊戲場景");

        PlayerData playerData = LobbyRpcManager.I.GetLocalLobbyPlayerData(networkClientId);
        playerData.IsInGameScene = true;
        LobbyRpcManager.I.UpdateLobbyPlayerServerRpc(playerData);

        bool isAllPlayerInGame = true;
        foreach (var player in LobbyRpcManager.I.PlayerData_List)
        {
            if (player.IsInGameScene == false)
            {
                isAllPlayerInGame = false;
                break;
            }
        }

        if (isAllPlayerInGame)
        {
            Debug.Log("所有玩家進入遊戲場景");
            GameStartClientRpc();
        }
    }

    [ClientRpc]
    private void GameStartClientRpc()
    {
        CheckGameView();
        if (_gameView != null && _gameView.gameObject.activeSelf)
        {
            _gameView.GameStart();
        }
    }

    /// <summary>
    /// 檢測遊戲介面
    /// </summary>
    /// <returns></returns>
    private void CheckGameView()
    {
        if (_gameView == null)
        {
            GameObject lobbyViewObj = GameObject.Find("GameView");
            if (lobbyViewObj != null)
            {
                _gameView = lobbyViewObj.GetComponent<GameView>();
            }
        }
    }
}