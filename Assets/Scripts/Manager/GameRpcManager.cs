using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameRpcManager : NetworkBehaviour
{
    private static GameRpcManager _instance;
    public static GameRpcManager I { get { return _instance; } }

    private GameView _gameView;

    // 角色生成位置
    private Vector3[] _spawnCharacterPos_Array = new Vector3[4] 
    {
        new Vector3(1.6f, 0, 0),
        new Vector3(1.6f, 0, 24),
        new Vector3(-22.4f, 0, 0), 
        new Vector3(-22.4f, 0, 24)
    };

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

    /// <summary>
    /// (Server)進入遊戲
    /// </summary>
    /// <param name="networkClientId"></param>
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

            // 生成角色
            List<Vector3> shuffleSpawnPos = Utils.I.Shuffle(_spawnCharacterPos_Array.ToList());
            for (int i = 0; i < LobbyRpcManager.I.PlayerData_List.Count; i++)
            {
                float rotY = shuffleSpawnPos[i].x > 0 ? 0 : 180;
                Vector3 rot = new Vector3(0, rotY, 0);

                SpawnObjectServerRpc(
                    LobbyRpcManager.I.PlayerData_List[i].NetworkClientId,
                    3,
                    shuffleSpawnPos[i] + GameDataManager.I.CreateSceneObjectOffset,
                    rot,
                    $"Character_{LobbyRpcManager.I.PlayerData_List[i].NetworkClientId}");
            }

            GameStartClientRpc();
        }
    }

    /// <summary>
    /// (Client)遊戲開始
    /// </summary>
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
    /// (Server)產生物件
    /// </summary>
    /// <param name="onwerId">擁有者Id</param>
    /// <param name="listIndex">NetworkObjectList</param>
    /// <param name="pos">位置</param>
    /// <param name="rot">旋轉</param>
    /// <param name="objName">物件名稱</param>
    /// <param name="isServerOwner">Server擁有權</param>
    [ServerRpc(RequireOwnership =false)]
    public void SpawnObjectServerRpc(ulong onwerId, int listIndex, Vector3 pos, Vector3 rot = default, string objName = "", bool isServerOwner = false)
    {
        GameObject createObj = SOManager.I.NetworkObject_SO.NetworkObjectList[listIndex].gameObject;
        GameObject obj = Instantiate(createObj);
        if (!string.IsNullOrEmpty(objName)) obj.name = objName;
        obj.transform.position = pos;
        obj.transform.eulerAngles = rot;
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        if (isServerOwner) networkObject.Spawn(true);
        else networkObject.SpawnWithOwnership(onwerId, true);
    }

    /// <summary>
    /// 消除物件
    /// </summary>
    /// <param name="networkObjectId">物件Id</param>
    [ServerRpc(RequireOwnership = false)]
    public void DespawnObjectServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            networkObject.Despawn(true);
        }
        else
        {
            Debug.LogError($"未找到 NetworkObjectId {networkObjectId}。");
        }
    }

    /// <summary>
    /// (Server)產生炸彈
    /// </summary>
    /// <param name="explotionLevel">爆炸等級</param>
    /// <param name="pos">位置</param>
    [ServerRpc(RequireOwnership =false)]
    public void SpawnBombServerRpc(int explotionLevel, Vector3 pos)
    {
        GameObject createObj = SOManager.I.NetworkObject_SO.NetworkObjectList[0].gameObject;
        GameObject obj = Instantiate(createObj, pos, Quaternion.identity);
        BombControl bombControl = obj.GetComponent<BombControl>();
        bombControl.ExplotionLevel = explotionLevel;
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
    }

    /// <summary>
    /// (Server)產生爆炸
    /// </summary>
    /// <param name="explotionLevel">爆炸等級</param>
    /// <param name="pos">位置</param>
    ///  <param name="dir">爆炸方向(0=原地, 1=上, 2=下, 3=左, 4=右)</param>
    /// <param name="isCenterExplosion">爆炸是否為中心點</param>
    [ServerRpc]
    public void SpawnExplosionServerRpc(int explotionLevel, Vector3 pos, int dir, bool isCenterExplosion)
    {
        GameObject createObj = SOManager.I.NetworkObject_SO.NetworkObjectList[1].gameObject;
        GameObject obj = Instantiate(createObj, pos, Quaternion.identity);
        ExplosionControl explosionControl = obj.GetComponent<ExplosionControl>();
        explosionControl.LastCount = explotionLevel;
        explosionControl.IsCenterExplosion = isCenterExplosion;
        explosionControl.ExplosionDirection = dir;
        explosionControl.InitializeExplosion();
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        networkObject.Spawn(true);
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