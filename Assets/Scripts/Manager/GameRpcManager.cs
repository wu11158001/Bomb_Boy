using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using Unity.Collections;
using Unity.Services.Lobbies.Models;

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

    // 遊戲倒數時間
    public NetworkVariable<int> GameTimeCd_NV = new(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private GameView _gameView;
    private bool _isGameOver;

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
        Debug.Log("退出 Game Rpc");
        GamePlayerData_List.OnListChanged -= OnGamePlayerChange;
        GameTimeCd_NV.OnValueChanged -= OnGameTimeChange;
    }

    public override void OnNetworkSpawn()
    {
        GamePlayerData_List.OnListChanged += OnGamePlayerChange;
        GameTimeCd_NV.OnValueChanged += OnGameTimeChange;
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
    /// 遊戲時間變更
    /// </summary>
    /// <param name="previousValue"></param>
    /// <param name="newValue"></param>
    private void OnGameTimeChange(int previousValue, int newValue)
    {
        CheckGameView();
        if (_gameView != null && _gameView.gameObject.activeSelf)
        {
            _gameView.DisplayGameTime();
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
    /// (Server)斷線重連
    /// </summary>
    /// <param name="gamePlayerData"></param>
    /// <param name="networkClientId"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ReconnectServerRpc(GamePlayerData gamePlayerData, ulong networkClientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(gamePlayerData.CharacterId, out NetworkObject networkObject))
        {
            Debug.Log("斷線重連，角色存在");
            networkObject.ChangeOwnership(networkClientId);
        }
        else
        {
            Debug.Log("斷線重連，角色不存在");
        }
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
        float rotY = shuffleSpawnPos[index].z > 0 ? 180 : 0;
        Vector3 rot = new(0, rotY, 0);

        GameObject createObj = SOManager.I.NetworkObject_SO.GameObjectList[3].gameObject;
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
            AuthenticationPlayerId = LobbyRpcManager.I.LobbyPlayerData_List[index].AuthenticationPlayerId,
            CharacterId = networkObject.NetworkObjectId,
            Nickname = LobbyRpcManager.I.LobbyPlayerData_List[index].Nickname,
            BombCount = 2,
            ExplotionLevel = 1,
            MoveSpeed = 5,
            IsDie = false,
            IsStopAction = true,
        };
        GamePlayerData_List.Add(gamePlayerData);

        // 所有玩家進入遊戲場景
        if (GamePlayerData_List.Count == LobbyRpcManager.I.LobbyPlayerData_List.Count)
        {
            Debug.Log("所有玩家進入遊戲場景");
            _isGameOver = false;

            // 初始化遊戲時間
            GameTimeCd_NV.Value = GameDataManager.GameTime;

            ShowGameSceneClientRpc();
            StartCoroutine(IStartGameCD());
        }
    }

    /// <summary>
    /// (Client)顯示遊戲場景
    /// </summary>
    [ClientRpc]
    private void ShowGameSceneClientRpc()
    {
        CheckGameView();
        if (_gameView != null && _gameView.gameObject.activeSelf)
        {
            _gameView.ShowGameScene();
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
        GameObject createObj = SOManager.I.NetworkObject_SO.GameObjectList[listIndex].gameObject;
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
            Debug.LogError($"消除物件 未找到 NetworkObjectId {networkObjectId}。");
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
        GameObject createObj = SOManager.I.NetworkObject_SO.GameObjectList[0].gameObject;
        GameObject obj = Instantiate(createObj, pos, Quaternion.identity);
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        BombControl bombControl = obj.GetComponent<BombControl>();
        bombControl.CharacterObjectId_NV.Value = networkObjectId;
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
        GameObject createObj = SOManager.I.NetworkObject_SO.GameObjectList[1].gameObject;
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

        GameObject createObj = SOManager.I.NetworkObject_SO.GameObjectList[2];
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
    /// (Server)角色躲藏
    /// </summary>
    /// <param name="networkObjectId"></param>
    /// <param name="isMasking">進入/離開</param>
    [ServerRpc(RequireOwnership = false)]
    public void CharacverHideServerRpc(ulong networkObjectId, bool isMasking)
    {
        CharacverHideClientRpc(networkObjectId, isMasking);
    }

    /// <summary>
    /// (Client)角色躲藏
    /// </summary>
    /// <param name="networkObjectId"></param>
    /// <param name="isMasking">進入/離開</param>
    [ClientRpc]
    public void CharacverHideClientRpc(ulong networkObjectId, bool isMasking)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            CharacterControl characterControl = networkObject.GetComponent<CharacterControl>();
            characterControl.CharacterHide(isMasking);
        }
    }

    /// <summary>
    /// (Server)角色死亡
    /// </summary>
    /// <param name="networkObjectId"></param>
    [ServerRpc]
    public void CharacterDieServerRpc(ulong networkObjectId)
    {
        GamePlayerData gamePlayerData = GetGamePlayerData(networkObjectId);
        if (gamePlayerData.IsDie == false)
        {
            gamePlayerData.IsDie = true;
            UpdateLobbyPlayerServerRpc(gamePlayerData);
            CharacterDieClientRpc(networkObjectId);
            JudgeGameResultServerRpc();
        }
    }

    /// <summary>
    /// (Client)角色死亡
    /// </summary>
    /// <param name="networkObjectId"></param>
    [ClientRpc]
    public void CharacterDieClientRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            networkObject.GetComponent<CharacterControl>().OnDie();
        }
        else
        {
            Debug.LogError($"角色死亡 未找到 NetworkObjectId {networkObjectId}。");
        }
    }

    /// <summary>
    /// (Server)判斷遊戲結果
    /// </summary>
    [ServerRpc(RequireOwnership =false)]
    public void JudgeGameResultServerRpc()
    {
        if (_isGameOver) return;

        int survival = 0;
        GamePlayerData winnerData = new();

        foreach (var gamePlayerData in GamePlayerData_List)
        {
            if (!gamePlayerData.IsDie)
            {
                survival++;
                winnerData = gamePlayerData;
            }
        }

        // 遊戲結束
        if (survival <= 1)
        {
            _isGameOver = true;

            OnGameOver();
            OnGameResultClientRpc(survival == 0, winnerData);
            StartCoroutine(IReturnToLobbyCD());
        }
    }

    /// <summary>
    /// 遊戲時間倒數
    /// </summary>
    [ServerRpc]
    public void GameTimeCdServerRpc()
    {
        GameTimeCd_NV.Value -= 1;

        if (GameTimeCd_NV.Value <= 0)
        {
            /*時間結束平手*/

            _isGameOver = true;

            OnGameOver();
            OnGameResultClientRpc(true, new GamePlayerData());
            StartCoroutine(IReturnToLobbyCD());
        }
    }

    /// <summary>
    /// (Client)遊戲結果
    /// </summary>
    /// <param name="isDraw">是否平手</param>
    /// <param name="winnerData">獲勝玩家資料</param>
    [ClientRpc]
    private void OnGameResultClientRpc(bool isDraw, GamePlayerData winnerData)
    {
        CheckGameView();
        if (_gameView != null && _gameView.gameObject.activeSelf)
        {
            _gameView.ShowGameResult(isDraw, winnerData);
        }
    }

    /// <summary>
    /// (Client)回到大廳倒數
    /// </summary>
    /// <param name="num">倒數數字</param>
    [ClientRpc]
    private void ReturnToLobbyCDClientRpc(int num)
    {
        CheckGameView();
        if (_gameView != null && _gameView.gameObject.activeSelf)
        {
            _gameView.ShowReturnToLobbyCD(num);
        }
    }

    /// <summary>
    /// 回到大廳倒數
    /// </summary>
    /// <returns></returns>
    private IEnumerator IReturnToLobbyCD()
    {
        if (!IsServer) yield break;

        yield return new WaitForSeconds(2);

        for (int i = 4; i >= 0; i--)
        {
            ReturnToLobbyCDClientRpc(i);
            yield return new WaitForSeconds(1);
        }

        // 更新房間狀態
        yield return LobbyManager.I.UpdateLobbyData(new Dictionary<string, DataObject>()
        {
            {$"{LobbyDataKey.State}", new DataObject(DataObject.VisibilityOptions.Public, $"{LobbyDataKey.In_Team}", DataObject.IndexOptions.S1) },
        });

        // 重製房間玩家
        LobbyRpcManager.I.ResetLobbyPlayerDataServerRpc();

        yield return new WaitForSeconds(0.5f);

        // 返回大廳
        ChangeSceneManager.I.ChangeScene_Network(SceneEnum.Lobby);
    }

    /// <summary>
    /// (Client)開始遊戲倒數
    /// </summary>
    /// <param name="num"></param>
    [ClientRpc]
    private void StartGameClientRpc()
    {
        CheckGameView();
        if (_gameView != null && _gameView.gameObject.activeSelf)
        {
            _gameView.ShowStartGameCD();
        }
    }

    /// <summary>
    /// (Client)開始遊戲
    /// </summary>
    [ClientRpc]
    private void GameStartClientRpc()
    {
        CheckGameView();
        if (_gameView != null && _gameView.gameObject.activeSelf)
        {
            _gameView.ShowGameStart();
        }
    }

    /// <summary>
    /// 開始遊戲倒數
    /// </summary>
    /// <returns></returns>
    private IEnumerator IStartGameCD()
    {
        if (!IsServer) yield break;

        yield return new WaitForSeconds(1);

        StartGameClientRpc();

        yield return new WaitForSeconds(2);

        // 玩家角色開始動作
        for (int i = 0; i < GamePlayerData_List.Count; i++)
        {
            GamePlayerData gamePlayerData = GetGamePlayerData(GamePlayerData_List[i].CharacterId);
            gamePlayerData.IsStopAction = false;
            UpdateLobbyPlayerServerRpc(gamePlayerData);
        }

        InvokeRepeating(nameof(GameTimeCdServerRpc), 0, 1);
        GameStartClientRpc();
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

        Debug.Log($"沒有找到 玩家: {networkObjectId} 玩家資料!!!");
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
            GameObject gameViewObj = GameObject.Find("GameView");
            if (gameViewObj != null)
            {
                _gameView = gameViewObj.GetComponent<GameView>();
            }
        }
    }

    /// <summary>
    /// 遊戲結束
    /// </summary>
    private void OnGameOver()
    {
        // 停止玩家角色動作
        for (int i = 0; i < GamePlayerData_List.Count; i++)
        {
            GamePlayerData gamePlayerData = GetGamePlayerData(GamePlayerData_List[i].CharacterId);
            gamePlayerData.IsStopAction = true;
            UpdateLobbyPlayerServerRpc(gamePlayerData);
        }

        // 停止遊戲時間倒數
        CancelInvoke(nameof(GameTimeCdServerRpc));
    }
}