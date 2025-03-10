using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class Utils : UnitySingleton<Utils>
{
    // 觸碰的UI
    private static GraphicRaycaster graphicRaycaster;
    private static EventSystem eventSystem;
    /// <summary>
    /// 初始化GraphicRaycaster和EventSystem
    /// </summary>
    private static void Initialize()
    {
        if (graphicRaycaster == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster == null)
                {
                    Debug.LogError("Canvas物件中未找到GraphicRaycaster");
                }
            }
            else
            {
                Debug.LogError("未找到Canvas物件");
            }
        }

        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogError("未找到EventSystem");
            }
        }
    }
    /// <summary>
    /// 獲取觸碰的UI物件
    /// </summary>
    /// <returns></returns>
    public static GameObject GetTouchUIObj()
    {
        Initialize();

        if (graphicRaycaster == null || eventSystem == null)
        {
            return null;
        }

        PointerEventData eventData = new PointerEventData(eventSystem)
        {
            pressPosition = Input.mousePosition,
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return result.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// 載入url圖片
    /// </summary>
    /// <param name="url"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static IEnumerator ImageUrlToSprite(string url, UnityAction<Sprite> callback)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Debug.Log($"Loading Texture:{url}");
            Sprite sprite = null;
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 載入成功，將下載的紋理轉換為Sprite
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                callback?.Invoke(sprite);
            }
            else
            {
                Debug.LogError($"載入url圖片失敗:{www.error}");
            }
        }
    }

    /// <summary>
    /// List洗牌
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public List<T> Shuffle<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("List洗牌 傳入值是空");
            return new List<T>();
        }

        List<T> result = list != null ? new List<T>(list) : new();
        System.Random rng = new();
        int n = result.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = result[k];
            result[k] = result[n];
            result[n] = value;
        }

        return result;
    }

    /// <summary>
    /// UI始終呈現在畫面
    /// </summary>
    /// <param name="rt"></param>
    public void UIAlwayOnScreen(RectTransform rt)
    {
        Vector2 anchoredPosition = rt.anchoredPosition;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 halfSize = rt.sizeDelta / 2;
        anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, -screenSize.x / 2 + halfSize.x, screenSize.x / 2 - halfSize.x);
        anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, -screenSize.y / 2 + halfSize.y, screenSize.y / 2 - halfSize.y);
        rt.anchoredPosition = anchoredPosition;
    }

    /// <summary>
    /// 獲取目標物件絕對位置
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public Vector2 GetTargerPosision(RectTransform target)
    {
        Vector2 anchoredPosition;
        RectTransform canvasRectTransform = ViewManager.I.CurrSceneCanvasRt;

        // 獲取屏幕座標下物件的相對位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, target.position, null, out anchoredPosition);

        float targetHeight = target.rect.height;
        float targetWidth = target.rect.width;
        Vector2 targetPivot = target.pivot;

        // 計算基於pivot的偏移
        Vector2 pivotOffset = new Vector2(
            targetWidth * (0.5f - targetPivot.x),
            targetHeight * (0.5f - targetPivot.y)
        );

        return anchoredPosition + pivotOffset;
    }

    /// <summary>
    /// 設置網格大小
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="isHorizontal"></param>
    /// <param name="rowOrColumnCount">平均內容數量</param>
    public void SetGridLayoutSize(RectTransform rt, bool isHorizontal, int rowOrColumnCount)
    {
        GridLayoutGroup gridLayout = rt.GetComponent<GridLayoutGroup>();
        float _layoutSpace = isHorizontal ?
                                 gridLayout.cellSize.x + gridLayout.spacing.x :
                                 gridLayout.cellSize.y + gridLayout.spacing.y;
        Vector2 offset = isHorizontal ?
                         rt.offsetMax :
                         rt.offsetMax;

        int count = 0;
        for (int i = 0; i < rt.childCount; i++)
        {
            if (rt.GetChild(i) != null && rt.GetChild(i).gameObject.activeSelf)
            {
                count++;
            }
        }

        if (isHorizontal)
        {
            offset.x = (Mathf.CeilToInt(count / rowOrColumnCount)) * _layoutSpace;
        }
        else
        {
            offset.y = (Mathf.CeilToInt(count / rowOrColumnCount)) * _layoutSpace;
        }

        rt.sizeDelta = offset;
    }

    /// <summary>
    /// 比較字典內容是否相同
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dict1"></param>
    /// <param name="dict2"></param>
    /// <returns></returns>
    public bool HasDictionaryChanged<T1, T2>(Dictionary<T1, T2> dict1, Dictionary<T1, T2> dict2)
    {
        // 檢查兩者的大小是否不同
        if (dict1.Count != dict2.Count)
        {
            return true;
        }

        // 檢查每個鍵和值是否相同
        foreach (var kvp in dict1)
        {
            if (!dict2.TryGetValue(kvp.Key, out T2 value) || !EqualityComparer<T2>.Default.Equals(kvp.Value, value))
            {
                return true;
            }
        }

        // 如果所有鍵值對都相同，則無變更
        return false;
    }

    /// <summary>
    /// 格式化數字轉成貨幣單位
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public string FormatNumberWithUnit(long num)
    {
        if (num >= 1_000_000_000)
            return (num / 1_000_000_000D).ToString("0.##") + "B";
        else if (num >= 1_000_000)
            return (num / 1_000_000D).ToString("0.##") + "M";
        else if (num >= 1_000)
            return (num / 1_000D).ToString("0.##") + "K";
        else
            return num.ToString("#,0"); // 小於1000顯示原始數字，並加上逗號格式
    }

    /// <summary>
    /// 複製文字
    /// </summary>
    /// <param name="str"></param>
    public void CopyText(string str)
    {
        TextEditor textEditor = new TextEditor()
        {
            text = str
        };
        textEditor.SelectAll();
        textEditor.Copy();

        Debug.Log($"複製文字: {str}");
    }

    /// <summary>
    /// 設置Dropdown項目
    /// </summary>
    /// <param name="dropdown"></param>
    /// <param name="options"></param>
    public void SetOptionsToDropdown(TMP_Dropdown dropdown, List<string> options)
    {
        // 清空當前選項
        dropdown.ClearOptions();

        // 添加新的選項
        dropdown.AddOptions(options);
    }

    /// <summary>
    /// 文字前方圖片跟隨
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="img"></param>
    /// <param name="space">間距</param>
    public void TextInFrontOfImageFollow(TextMeshProUGUI textComponent, Image img, float space = 20)
    {
        float txtWidth = textComponent.preferredWidth;

        textComponent.rectTransform.sizeDelta = new Vector2(txtWidth,
                                                            textComponent.rectTransform.sizeDelta.y);

        Vector2 size = img.rectTransform.sizeDelta;
        img.transform.SetParent(textComponent.transform);
        img.rectTransform.anchorMax = new Vector2(0, 0.5f);
        img.rectTransform.anchorMin = new Vector2(0, 0.5f);
        img.rectTransform.pivot = new Vector2(0, 0.5f);
        img.rectTransform.sizeDelta = size;
        img.rectTransform.anchoredPosition = new Vector2(-size.x - space, 0);
    }

    /// <summary>
    /// 日期轉時間戳
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long ConvertDateTimeToTimestamp(DateTime dateTime)
    {
        // 確保DateTime對象是基於UTC
        DateTime utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        // 使用DateTimeOffset將其轉換為Unix時間戳
        long timestamp = new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();

        return timestamp;
    }

    /// <summary>
    /// 時間戳轉日期
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime ConvertTimestampToDate(long timestamp)
    {
        // 使用DateTimeOffset將Unix時間戳轉換為DateTime
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        DateTime dateTime = dateTimeOffset.DateTime;

        // 如果需要UTC時間，可以使用:
        // DateTime dateTime = dateTimeOffset.UtcDateTime;

        return dateTime;
    }
}
