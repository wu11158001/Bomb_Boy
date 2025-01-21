using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager _instance = null;
    public static GameSceneManager I
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameSceneManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<GameSceneManager>();
                    obj.hideFlags = HideFlags.DontSave;
                    obj.name = typeof(GameSceneManager).Name;
                }
            }

            return _instance;
        }
    }

    // 場景中可擊破物數量
    private int _dropPropsCount;
    // 場景中可擊破物件
    private GameObject[] _breakObstacle;
    // 紀錄掉落道具可擊破物件
    private List<Transform> _recodeDropPropsIndexList;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (_instance == null) _instance = this as GameSceneManager;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        InitializeGameSceneManager();
    }

    /// <summary>
    /// 初始化遊戲場景管理中心
    /// </summary>
    public void InitializeGameSceneManager()
    {
        _breakObstacle = GameObject.FindGameObjectsWithTag($"{LayerNameEnum.BreakObstacle}");

        _dropPropsCount = 2;

        // 設置掉落道具位置
        _recodeDropPropsIndexList = new();
        List<Transform> breakObstacleTransform = _breakObstacle.Select(x => x.transform).ToList();
        List<Transform> ShuffleBreakObstacle = Utils.I.Shuffle<Transform>(breakObstacleTransform);
        _recodeDropPropsIndexList = ShuffleBreakObstacle.Take(_dropPropsCount).ToList();
    }

    /// <summary>
    /// 消除可擊破物件
    /// </summary>
    /// <param name="obj"></param>
    public void DespawnBreakObstacle(GameObject obj)
    {
        GameObject breakObj = _breakObstacle.Where(x => x == obj).FirstOrDefault();
        if (_recodeDropPropsIndexList.Contains(breakObj.transform))
        {
            // 產生掉落道具
            DropPropsEnum dropPropsType = (DropPropsEnum)UnityEngine.Random.Range(0, Enum.GetValues(typeof(DropPropsEnum)).Length);
            DropProps dropProps = Instantiate(SOManager.I.NetworkObject_SO.NetworkObjectList[2]).GetComponent<DropProps>();
            Vector3 offset = GameDataManager.I.CreateSceneObjectOffset;
            dropProps.gameObject.transform.position = breakObj.transform.position + offset;
            dropProps.SetDropPropsType(dropPropsType);
        }

        Destroy(breakObj);
    }
}
