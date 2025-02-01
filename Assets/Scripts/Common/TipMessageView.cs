using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class TipMessageView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Message_Txt;

    private Coroutine _fadeOut_Coroutine;

    /// <summary>
    /// 顯示提示訊息
    /// </summary>
    /// <param name="msg"></param>
    public void ShowTipMessage(string msg)
    {
        Message_Txt.text = msg;

        if (_fadeOut_Coroutine != null) StopCoroutine(_fadeOut_Coroutine);
        _fadeOut_Coroutine = StartCoroutine(IFadeOut());
    }

    /// <summary>
    /// 淡出效果
    /// </summary>
    /// <returns></returns>
    private IEnumerator IFadeOut()
    {
        // 淡出時間
        float fadeoutDuring = 1;

        Color color = Message_Txt.color;
        color.a = 1;
        Message_Txt.color = color;

        yield return new WaitForSeconds(2);

        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < fadeoutDuring)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / fadeoutDuring;
            float a = Mathf.Lerp(1, 0, progress);
            color.a = a;
            Message_Txt.color = color;

            yield return null;
        }

        ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.TipMessageView);
    }
}
