using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

public class BasePopUpView : MonoBehaviour
{
    [Header("彈窗")]
    [SerializeField] Button Mask_Btn;
    [SerializeField] RectTransform SlideArea;
    [SerializeField] bool isMaskBtn;

    protected virtual void OnEnable()
    {
        StartCoroutine(IDisplayView());
    }

    /// <summary>
    /// 介面顯示
    /// </summary>
    /// <returns></returns>
    private IEnumerator IDisplayView()
    {
        float alphaMax = 200;                         // 遮罩透明值最大值
        float maskEffectTime = 0.2f;                  // 遮罩效果完成時間
        float slideViewMaxPosY = 50.0f;               // 滑動介面最頂部位置
        float slideViewReboundTime = 0.1f;            // 滑動介面回彈時間
        float slideViewPauseTime = 0.05f;             // 滑動介面停頓時間

        DateTime startTime = DateTime.Now;

        Color color = Mask_Btn.image.color;
        while ((DateTime.Now - startTime).TotalSeconds <= maskEffectTime)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / maskEffectTime;

            // 遮罩淡入效果
            color.a = Mathf.Lerp(0, alphaMax / 255, progress);
            Mask_Btn.image.color = color;

            // 介面向上滑動效果
            float posY = Mathf.Lerp(-Screen.height, slideViewMaxPosY, progress);
            SlideArea.anchoredPosition = new Vector2(0, posY);

            yield return null;
        }
        color.a = alphaMax / 255;
        Mask_Btn.image.color = color;
        SlideArea.anchoredPosition = new Vector2(0, slideViewMaxPosY);

        yield return new WaitForSecondsRealtime(slideViewPauseTime);

        startTime = DateTime.Now;
        float currViewPosY = SlideArea.anchoredPosition.y;
        while ((DateTime.Now - startTime).TotalSeconds <= slideViewReboundTime)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / slideViewReboundTime;

            // 介面回彈滑動效果
            float posY = Mathf.Lerp(currViewPosY, 0, progress);
            SlideArea.anchoredPosition = new Vector2(0, posY);

            yield return null;
        }
        SlideArea.anchoredPosition = Vector2.zero;

        // 遮罩按鈕
        Mask_Btn.onClick.AddListener(() =>
        {
            if (isMaskBtn)
            {
                Mask_Btn.onClick.RemoveAllListeners();
                StartCoroutine(ICloseView());
            }
        });
    }

    /// <summary>
    /// 介面關閉
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ICloseView()
    {
        float slideViewReboundPosY = 50.0f;             // 滑動介面回彈位置
        float slideViewReboundTime = 0.1f;              // 滑動介面回彈時間
        float waitTime = 0.05f;                         // 等待開始下滑時間
        float slideTime = 0.2f;                         // 下滑完成時間

        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds <= slideViewReboundTime)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / slideViewReboundTime;
            // 介面向上滑動效果
            float posY = Mathf.Lerp(0, slideViewReboundPosY, progress);
            SlideArea.anchoredPosition = new Vector2(0, posY);

            yield return null;
        }

        yield return new WaitForSeconds(waitTime);

        Color color = Mask_Btn.image.color;
        float currMaskAlpha = color.a;
        float currViewPosY = SlideArea.anchoredPosition.y;
        startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds <= slideTime)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / slideTime;

            // 遮罩淡出效果
            color.a = Mathf.Lerp(currMaskAlpha, 0, progress);
            Mask_Btn.image.color = color;

            // 介面向下滑動效果
            float posY = Mathf.Lerp(currViewPosY, -Screen.height, progress);
            SlideArea.anchoredPosition = new Vector2(0, posY);

            yield return null;
        }

        ViewManager.I.CloseCurrView();
    }
}
