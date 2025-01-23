using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class ViewManager : UnitySingleton<ViewManager>
{
    // 已開啟介面
    private Stack<RectTransform> _openedView = new();

    // 紀錄已開啟的介面
    private Dictionary<ViewEnum, RectTransform> _recodeView = new();
    // 紀錄已開啟的常駐介面
    private Dictionary<PermanentViewEnum, RectTransform> _recodePermanetView = new();

    public RectTransform CurrSceneCanvasRt { get; private set; }
    public RectTransform PermanentCanvasRt { get; private set; }

    private void Start()
    {
        PermanentCanvasRt = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 設置當前場景Canvas
    /// </summary>
    private void SetCurrSceneCanvas()
    {
        CurrSceneCanvasRt = GameObject.Find("Canvas").GetComponent<RectTransform>();
    }

    /// <summary>
    /// 重製介面資料
    /// </summary>
    public void ResetViewData()
    {
        SetCurrSceneCanvas();
        _openedView.Clear();
    }

    /// <summary>
    /// 開啟介面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="viewName"></param>
    /// <param name="callback"></param>
    public void OpenView<T>(ViewEnum viewName, UnityAction<T> callback = null) where T : Component
    {
        if (CurrSceneCanvasRt == null)
        {
            SetCurrSceneCanvas();
        }

        if (_recodeView.ContainsKey(viewName))
        {
            RectTransform view = _recodeView[viewName];
            CreateViewHandle(view, CurrSceneCanvasRt, callback);
            _openedView.Push(view);
        }
        else
        {
            GameObject newViewObj = SOManager.I.View_SO.ViewList[(int)viewName];
            RectTransform newView = Instantiate(newViewObj, CurrSceneCanvasRt).GetComponent<RectTransform>();
            CreateViewHandle(newView, CurrSceneCanvasRt, callback);
            _recodeView.Add(viewName, newView);
            _openedView.Push(newView);
        }
    }

    /// <summary>
    /// 關閉當前介面
    /// </summary>
    public void CloseCurrView()
    {
        _openedView.Pop().gameObject.SetActive(false);
    }

    /// <summary>
    /// 開啟常駐介面
    /// </summary>
    /// <param name="permanentView"></param>
    /// <returns></returns>
    public void OpenPermanentView<T>(PermanentViewEnum permanentView, UnityAction<T> callback = null) where T : Component
    {
        if (_recodePermanetView.ContainsKey(permanentView))
        {
            _recodePermanetView[permanentView].gameObject.SetActive(true);
        }
        else
        {
            GameObject viewObj = SOManager.I.View_SO.PermanentViewList[(int)permanentView];
            RectTransform view = Instantiate(viewObj, PermanentCanvasRt).GetComponent<RectTransform>();
            CreateViewHandle<RectTransform>(view, PermanentCanvasRt);

            _recodePermanetView.Add(permanentView, view);
        }
    }

    /// <summary>
    /// 關閉常駐介面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="permanentView"></param>
    /// <param name="callback"></param>
    public void ClosePermanentView<T>(PermanentViewEnum permanentView, UnityAction<T> callback = null) where T : Component
    {
        if (_recodePermanetView.ContainsKey(permanentView))
        {
            _recodePermanetView[permanentView].gameObject.SetActive(false);
            ActionCallback(_recodePermanetView[permanentView], callback);
            _recodePermanetView.Remove(permanentView);
        }
    }

    /// <summary>
    /// 產生介面處理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="view"></param>
    /// <param name="canvasRt"></param>
    /// <param name="callback"></param>
    public void CreateViewHandle<T>(RectTransform view, RectTransform canvasRt, UnityAction<T> callback = null) where T : Component
    {
        view.gameObject.SetActive(true);
        view.offsetMax = Vector2.zero;
        view.offsetMin = Vector2.zero;
        view.anchoredPosition = Vector2.zero;
        view.eulerAngles = Vector3.zero;
        view.localScale = Vector3.one;
        view.name = view.name.Replace("(Clone)", "");
        view.SetSiblingIndex(canvasRt.childCount + 1);

        ActionCallback(view, callback);
    }

    /// <summary>
    /// 執行回傳方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="view"></param>
    /// <param name="callback"></param>
    private void ActionCallback<T>(RectTransform view, UnityAction<T> callback) where T : Component
    {
        if (callback != null)
        {
            T component = view.GetComponent<T>();
            if (component != null)
            {
                callback?.Invoke(component);
            }
            else
            {
                Debug.LogError($"{view.name}: 介面不存在 Component");
            }
        }
    }
}
