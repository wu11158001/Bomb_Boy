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

    // 場景中可擊破物件
    private GameObject[] _breakObstacle;
    // 紀錄掉落道具可擊破物件
    private List<Transform> _recodeDropPropsIndexList;

    // 場景中可擊破物數量
    private const int _dropPropsCount = 2;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        InitializeGameSceneManager();
    }

    /// <summary>
    /// 初始化遊戲場景管理中心
    /// </summary>
    public void InitializeGameSceneManager()
    {
        // 場景中可擊破物件
        _breakObstacle = GameObject.FindGameObjectsWithTag($"{LayerNameEnum.BreakObstacle}");

        if (NetworkManager.Singleton.IsServer)
        {
            // 設置掉落道具位置            
            _recodeDropPropsIndexList = new();
            List<Transform> breakObstacleTransform = _breakObstacle.Select(x => x.transform).ToList();
            List<Transform> ShuffleBreakObstacle = Utils.I.Shuffle<Transform>(breakObstacleTransform);
            _recodeDropPropsIndexList = ShuffleBreakObstacle.Take(_dropPropsCount).ToList();
        }
    }

    /// <summary>
    /// 消除可擊破物件
    /// </summary>
    /// <param name="obj"></param>
    public void DespawnBreakObstacle(GameObject obj)
    {
        if (!IsServer) return;

        GameObject breakObj = _breakObstacle.Where(x => x == obj).FirstOrDefault();
        if (_recodeDropPropsIndexList.Contains(breakObj.transform))
        {
            // 產生掉落道具
            /*DropPropsEnum dropPropsType = (DropPropsEnum)UnityEngine.Random.Range(0, Enum.GetValues(typeof(DropPropsEnum)).Length);
            DropProps dropProps = Instantiate(SOManager.I.NetworkObject_SO.NetworkObjectList[2]).GetComponent<DropProps>();
            Vector3 offset = GameDataManager.I.CreateSceneObjectOffset;
            dropProps.gameObject.transform.position = breakObj.transform.position + offset;
            dropProps.SetDropPropsType(dropPropsType);*/
        }

        GameRpcManager.I.DespawnObjectServerRpc(breakObj.GetComponent<NetworkObject>().NetworkObjectId);
    }
}
