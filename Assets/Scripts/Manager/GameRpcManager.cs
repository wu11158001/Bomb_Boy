using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class GameRpcManager : NetworkBehaviour
{
    private static GameRpcManager _instance;
    public static GameRpcManager I { get { return _instance; } }

    // 所有遊戲玩家資料
    public NetworkList<GamePlayerData> GamePlayerData_List { get; private set; }
       = new(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // 掉落道具_炸彈數量增加_剩餘數量
    public NetworkVariable<int> BombAddPropsRemainingQuantity { get; private set; }
        = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // 掉落道具_爆炸等級_剩餘數量
    public NetworkVariable<int> PowerPropsRemainingQuantity { get; private set; }
    = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // 掉落道具_移動速度_剩餘數量
    public NetworkVariable<int> SpeedPropsRemainingQuantity { get; private set; }
    = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private GameView _gameView;

    // 角色生成位置
    private List<Vector3> shuffleSpawnPos;
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

    public override void OnNetworkDespawn()
    {
        GamePlayerData_List.OnListChanged -= OnGamePlayerChange;
    }

    public override void OnNetworkSpawn()
    {
        GamePlayerData_List.OnListChanged += OnGamePlayerChange;
    }

    /// <summary>
    /// 遊戲玩家資料變更
    /// </summary>
    /// <param name="changeEvent"></param>
    private void OnGamePlayerChange(NetworkListEvent<GamePlayerData> changeEvent)
    {
        // 遊戲中角色更新
        if (SceneManager.GetActiveScene().name == $"{SceneEnum.Game}")
        {
            foreach (var gamePlayerData in GamePlayerData_List)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(gamePlayerData.CharacterId, out NetworkObject networkObject))
                {
                    CharacterControl characterControl = networkObject.gameObject.GetComponent<CharacterControl>();
                    characterControl.UpdateCharacterData();
                }
            }
        }
    }

    /// <summary>
    /// 初始化遊戲Rpc管理
    /// </summary>
    public void InitializeGameRpcManager()
    {
        shuffleSpawnPos = Utils.I.Shuffle(_spawnCharacterPos_Array.ToList());
        GamePlayerData_List.Clear();
    }

    /// <summary>
    /// (Server)進入遊戲
    /// </summary>
    /// <param name="networkClientId"></param>
    [ServerRpc(RequireOwnership = false)]
    public void InGameSceneServerRpc(ulong networkClientId)
    {
        Debug.Log($"玩家: {networkClientId} 已進入遊戲場景");

        // 生成角色
        int index = GamePlayerData_List.Count;
        float rotY = shuffleSpawnPos[index].x > 0 ? 0 : 180;
        Vector3 rot = new(0, rotY, 0);

        GameObject createObj = SOManager.I.NetworkObject_SO.NetworkObjectList[3].gameObject;
        GameObject obj = Instantiate(createObj);
        obj.name = $"Character_{LobbyRpcManager.I.LobbyPlayerData_List[index].NetworkClientId}";
        obj.transform.position = shuffleSpawnPos[index] + GameDataManager.I.CreateSceneObjectOffset;
        obj.transform.eulerAngles = rot;
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(LobbyRpcManager.I.LobbyPlayerData_List[index].NetworkClientId, true);

        // 初始化掉落道具數量
        BombAddPropsRemainingQuantity.Value = 12;
        PowerPropsRemainingQuantity.Value = 13;
        SpeedPropsRemainingQuantity.Value = 15;

        // 初始化玩家資料
        GamePlayerData gamePlayerData = new()
        {
            CharacterId = networkObject.NetworkObjectId,
            BombCount = 2,
            ExplotionLevel = 1,
            MoveSpeed = 5,
        };
        GamePlayerData_List.Add(gamePlayerData);

        // 所有玩家進入遊戲場景
        if (GamePlayerData_List.Count == LobbyRpcManager.I.LobbyPlayerData_List.Count)
        {
            Debug.Log("所有玩家進入遊戲場景");
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
    /// (Server)消除物件
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
    /// <param name="networkClientId"></param>
    /// <param name="explotionLevel">爆炸等級</param>
    /// <param name="pos">位置</param>
    [ServerRpc(RequireOwnership = false)]
    public void SpawnBombServerRpc(ulong networkObjectId, int explotionLevel, Vector3 pos)
    {
        GameObject createObj = SOManager.I.NetworkObject_SO.NetworkObjectList[0].gameObject;
        GameObject obj = Instantiate(createObj, pos, Quaternion.identity);
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        BombControl bombControl = obj.GetComponent<BombControl>();
        bombControl.CharacterObjectId = networkObjectId;
        bombControl.ExplotionLevel = explotionLevel;

        // 更新遊戲玩家資料
        GamePlayerData gamePlayerData = GetGamePlayerData(networkObjectId);
        gamePlayerData.BombCount -= 1;
        UpdateLobbyPlayerServerRpc(gamePlayerData);
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
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        ExplosionControl explosionControl = obj.GetComponent<ExplosionControl>();
        explosionControl.LastCount = explotionLevel;
        explosionControl.IsCenterExplosion = isCenterExplosion;
        explosionControl.ExplosionDirection = dir;
        explosionControl.InitializeExplosion();
    }

    /// <summary>
    /// (Server)產生掉落道具
    /// </summary>
    /// <param name="pos">位置</param>
    [ServerRpc]
    public void SpawnDropPropsServerRpc(Vector3 pos)
    {
        List<int> takeProps = new();
        if (BombAddPropsRemainingQuantity.Value > 0) 
            takeProps.Add((int)DropPropsEnum.BombAddProps);
        if (PowerPropsRemainingQuantity.Value > 0)
            takeProps.Add((int)DropPropsEnum.PowerProps);
        if (SpeedPropsRemainingQuantity.Value > 0)
            takeProps.Add((int)DropPropsEnum.SpeedProps);

        if (takeProps.Count == 0) return;

        DropPropsEnum dropPropsType = (DropPropsEnum)Utils.I.Shuffle(takeProps)[0];
        switch (dropPropsType)
        {
            case DropPropsEnum.BombAddProps:
                BombAddPropsRemainingQuantity.Value -= 1;
                break;
            case DropPropsEnum.PowerProps:
                PowerPropsRemainingQuantity.Value -= 1;
                break;
            case DropPropsEnum.SpeedProps:
                SpeedPropsRemainingQuantity.Value -= 1;
                break;
        }

        GameObject createObj = SOManager.I.NetworkObject_SO.NetworkObjectList[2];
        GameObject obj = Instantiate(createObj, pos, Quaternion.identity);
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        DropProps dropProps = obj.GetComponent<DropProps>();
        dropProps.SetDropPropsType(dropPropsType);
    }

    /// <summary>
    /// (Server)獲得掉落道具
    /// </summary>
    /// <param name="networkObjectId"></param>
    /// <param name="propsType">道具類型</param>
    [ServerRpc]
    public void GetDropPropsServerRpc(ulong networkObjectId, DropPropsEnum propsType)
    {
        Debug.Log($"獲得掉落道具: {propsType}");
        GamePlayerData gamePlayerData = GetGamePlayerData(networkObjectId);

        switch (propsType)
        {
            // 炸彈數量增加道具
            case DropPropsEnum.BombAddProps:
                gamePlayerData.BombCount += 1;
                break;

            // 爆炸等級強化道具
            case DropPropsEnum.PowerProps:
                gamePlayerData.ExplotionLevel += 1;
                break;

            // 移動速度強化道具
            case DropPropsEnum.SpeedProps:
                gamePlayerData.MoveSpeed += 1;
                break;
        }

        UpdateLobbyPlayerServerRpc(gamePlayerData);
    }

    /// <summary>
    /// (Server)更新遊戲玩家資料
    /// </summary>
    /// <param name="updatePlayerData"></param>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateLobbyPlayerServerRpc(GamePlayerData updatePlayerData)
    {
        for (int i = 0; i < GamePlayerData_List.Count; i++)
        {
            if (GamePlayerData_List[i].CharacterId == updatePlayerData.CharacterId)
            {
                GamePlayerData_List[i] = updatePlayerData;
                return;
            }
        }

        Debug.LogError($"玩家: {updatePlayerData.CharacterId} 更新遊戲玩家資料錯誤");
    }

    /// <summary>
    /// 獲取遊戲玩家資料
    /// </summary>
    /// <param name="networkObjectId"></param>
    /// <returns></returns>
    public GamePlayerData GetGamePlayerData(ulong networkObjectId)
    {
        for (int i = 0; i < GamePlayerData_List.Count; i++)
        {
            if (GamePlayerData_List[i].CharacterId == networkObjectId)
            {
                return GamePlayerData_List[i];
            }
        }

        Debug.LogError($"玩家: {networkObjectId} 獲取玩家資料錯誤");
        return new();
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