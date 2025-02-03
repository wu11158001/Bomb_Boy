using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Netcode;

public class GameSceneManager : NetworkBehaviour
{
    private static GameSceneManager _instance = null;
    public static GameSceneManager I { get { return _instance; } }

    // 紀錄掉落道具可擊破物件Id
    private List<NetworkObject> _recodeDropPropsPosIdList;

    // 可擊破物件Id列表
    private List<NetworkObject> _breakObstacleList;

    // 紀錄已移除的可擊破物件Id
    private NetworkList<GameTerrainData> _gameTerrainDataList = 
        new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    // 場景中可擊破物數量
    private const int _dropPropsCount = 45;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        _gameTerrainDataList.OnListChanged += BreakObstacleChange;
        InitializeGameSceneManager();
    }

    /// <summary>
    /// 可擊破物件變更物件
    /// </summary>
    /// <param name="networkListEvent"></param>
    private void BreakObstacleChange(NetworkListEvent<GameTerrainData> networkListEvent)
    {
        SynchronousScene();
    }

    /// <summary>
    /// 同步場景
    /// </summary>
    public void SynchronousScene()
    {
        foreach (var gameTerrainData in _gameTerrainDataList)
        {
            NetworkObject networkObject = _breakObstacleList.Where(x => x.NetworkObjectId == gameTerrainData.RemoveBreakObstacleId).FirstOrDefault();
            if (networkObject != null)
            {
                networkObject.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 初始化遊戲場景管理中心
    /// </summary>
    public void InitializeGameSceneManager()
    {
        // 場景中可擊破物件
        _breakObstacleList = new();
        List<GameObject>  breakObstacleList = GameObject.FindGameObjectsWithTag($"{LayerNameEnum.BreakObstacle}").ToList();
        foreach (var breakObstacle in breakObstacleList)
        {
            NetworkObject networkObject = breakObstacle.GetComponent<NetworkObject>();
            _breakObstacleList.Add(networkObject);
        }

        if (IsServer)
        {
            _gameTerrainDataList.Clear();

            // 設置掉落道具位置            
            List<NetworkObject> tempBreakObstacle = _breakObstacleList.Select(x => x).ToList();
            List<NetworkObject> ShuffleBreakObstacle = Utils.I.Shuffle(tempBreakObstacle);
            _recodeDropPropsPosIdList = new();
            _recodeDropPropsPosIdList = ShuffleBreakObstacle.Take(_dropPropsCount).ToList();
        }
    }

    /// <summary>
    /// 消除可擊破物件
    /// </summary>
    /// <param name="networkObjectId"></param>
    public void DespawnBreakObstacle(ulong networkObjectId)
    {
        if (!IsServer) return;

        NetworkObject breakObj = _breakObstacleList.Where(x => x.NetworkObjectId == networkObjectId).FirstOrDefault();
        if (breakObj == null) return;

        if (_recodeDropPropsPosIdList.Contains(breakObj))
        {
            // 產生掉落道具  
            Vector3 offset = GameDataManager.I.CreateSceneObjectOffset;
            Vector3 pos = breakObj.transform.position + offset;
            pos.y = 0.5f;
            GameRpcManager.I.SpawnDropPropsServerRpc(pos);
        }

        // 添加已移除的可擊破物件
        GameTerrainData gameTerrainData = new()
        {
            RemoveBreakObstacleId = networkObjectId,
        };
        _gameTerrainDataList.Add(gameTerrainData);
    }
}
