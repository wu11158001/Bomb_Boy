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

    // 可擊破物件列表
    private List<NetworkObject> _breakObstacleList;
    // 遮蔽物件列表
    private List<NetworkObject> _maskingList;

    // 紀錄已移除的可擊破物件Id
    private NetworkList<GameTerrainData> _removeBreakObstacleId_List = 
        new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // 紀錄已移除的躲藏物件Id
    private NetworkList<GameTerrainData> _removeHideObjectId_List =
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
        _removeBreakObstacleId_List.OnListChanged += BreakObstacleChange;
        _removeHideObjectId_List.OnListChanged += MaskingChange;
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
    /// 遮蔽物件變更物件
    /// </summary>
    /// <param name="networkListEvent"></param>
    private void MaskingChange(NetworkListEvent<GameTerrainData> networkListEvent)
    {
        SynchronousScene();
    }

    /// <summary>
    /// 同步場景
    /// </summary>
    public void SynchronousScene()
    {
        // 可擊破物件
        foreach (var breakObstacle in _removeBreakObstacleId_List)
        {
            NetworkObject networkObject = _breakObstacleList.Where(x => x.NetworkObjectId == breakObstacle.RemoveBreakObstacleId).FirstOrDefault();
            if (networkObject != null)
            {
                networkObject.gameObject.SetActive(false);
            }
        }

        // 遮蔽物件
        foreach (var masking in _removeHideObjectId_List)
        {
            NetworkObject networkObject = _maskingList.Where(x => x.NetworkObjectId == masking.RemoveHideObjectId).FirstOrDefault();
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
        // 可擊破物件
        _breakObstacleList = new();
        List<GameObject>  breakObstacleList = GameObject.FindGameObjectsWithTag($"{LayerNameEnum.BreakObstacle}").ToList();
        foreach (var breakObstacle in breakObstacleList)
        {
            NetworkObject networkObject = breakObstacle.GetComponent<NetworkObject>();
            _breakObstacleList.Add(networkObject);
        }

        // 遮蔽物件
        _maskingList = new();
        List<GameObject> maskingList = GameObject.FindGameObjectsWithTag($"{LayerNameEnum.HideObject}").ToList();
        foreach (var masking in maskingList)
        {
            NetworkObject networkObject = masking.GetComponent<NetworkObject>();
            _maskingList.Add(networkObject);
        }


        if (IsServer)
        {
            _removeBreakObstacleId_List.Clear();
            _removeHideObjectId_List.Clear();

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
        _removeBreakObstacleId_List.Add(gameTerrainData);
    }

    /// <summary>
    /// 消除遮蔽物件
    /// </summary>
    /// <param name="networkObjectId"></param>
    public void DespawnMasking(ulong networkObjectId)
    {
        if (!IsServer) return;

        GameTerrainData gameTerrainData = new()
        {
            RemoveHideObjectId = networkObjectId,
        };
        _removeHideObjectId_List.Add(gameTerrainData);
    }
}
