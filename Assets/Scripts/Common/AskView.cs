using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class AskView : BasePopUpView
{
    [SerializeField] TextMeshProUGUI Content_Txt;
    [SerializeField] Button Confirm_Btn;
    [SerializeField] Button Cancel_Btn;

    /// <summary>
    /// 設置詢問介面
    /// </summary>
    /// <param name="content"></param>
    /// <param name="confirmCallback"></param>
    /// <param name="cancelCallback"></param>
    public void SetAskView(string content, UnityAction confirmCallback, UnityAction cancelCallback = null)
    {
        // 詢問內容文字
        Content_Txt.text = content;

        // 確認按鈕
        Confirm_Btn.onClick.RemoveAllListeners();
        Confirm_Btn.onClick.AddListener(() =>
        {
            confirmCallback?.Invoke();
            StartCoroutine(ICloseView());
        });

        // 取消按鈕
        Cancel_Btn.onClick.RemoveAllListeners();
        Cancel_Btn.onClick.AddListener(() =>
        {
            cancelCallback?.Invoke();
            StartCoroutine(ICloseView());
        });
    }
}
