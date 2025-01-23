using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjPool
{
    /// <summary>
    /// 物件池(主物件, (父物件, 已創建物件)
    /// </summary>
    private Dictionary<GameObject, (Transform, List<GameObject>)> _poolDic;

    private Transform _mainParent;       // 掛載父物件
    private int _cleanNum;               // 達到數量清理未使用物件

    public ObjPool(Transform parent, int cleanNum = 10)
    {
        _mainParent = parent;
        _cleanNum = cleanNum;
        _poolDic = new Dictionary<GameObject, (Transform, List<GameObject>)>();
    }

    /// <summary>
    /// 獲取物件列表
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public List<GameObject> GetObjList(GameObject obj)
    {
        if (_poolDic.ContainsKey(obj))
        {
            return _poolDic[obj].Item2;
        }
        else
        {
            return new List<GameObject>();
        }
    }

    /// <summary>
    /// 物件池創建物件
    /// </summary>
    /// <param name="obj">創建的物件</param>
    /// <param name="parentObj">父物件</param>
    /// <returns></returns>
    public T CreateObj<T>(GameObject obj, Transform parentObj = null) where T : Component
    {
        if (_poolDic.ContainsKey(obj))
        {
            //物件池已有物件
            foreach (var item in _poolDic[obj].Item2)
            {
                //有物件未使用
                if (!item.activeSelf)
                {
                    item.SetActive(true);

                    //清理物件池
                    if (_poolDic[obj].Item2.Count >= _cleanNum)
                    {
                        List<GameObject> cleanList = new List<GameObject>();
                        foreach (var usingObj in _poolDic[obj].Item2)
                        {
                            if (!usingObj.activeSelf)
                            {
                                cleanList.Add(usingObj);
                            }
                        }

                        foreach (var cleanObj in cleanList)
                        {
                            _poolDic[obj].Item2.Remove(cleanObj);
                            GameObject.Destroy(cleanObj);
                        }
                    }

                    return item.GetComponent<T>();
                }
            }

            GameObject addObj = GameObject.Instantiate(obj, _poolDic[obj].Item1);
            addObj.SetActive(true);
            _poolDic[obj].Item2.Add(addObj);
            return addObj.GetComponent<T>();
        }

        //創建新物件池
        GameObject parent = null;
        if (parentObj == null)
        {
            parent = new GameObject();
            parent.name = $"{obj.name}Pool";
            parent.transform.SetParent(this._mainParent.transform);
        }
        else
        {
            parent = parentObj.gameObject;
        }

        GameObject newObj = GameObject.Instantiate(obj, parent.transform);
        newObj.SetActive(true);
        _poolDic.Add(obj, (parent.transform, new List<GameObject>() { newObj }));
        return newObj.GetComponent<T>();
    }
}