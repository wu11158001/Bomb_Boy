using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyChatItem : MonoBehaviour
{
    [SerializeField] RectTransform ThisRt;
    [SerializeField] TextMeshProUGUI Nickname_Txt;
    [SerializeField] RectTransform ChatBg_Rt;
    [SerializeField] TextMeshProUGUI ChatMsg_Txt;

    // 訊息文字最大寬度
    private const float _maxMsgWidth = 550;

    /// <summary>
    /// 設置大廳聊天項目
    /// </summary>
    /// <param name="chatData"></param>
    /// <param name="isLocal">本地玩家/其他玩家</param>
    public Vector2 SetLobbyChatItem(ChatData chatData, bool isLocal)
    {
        if (isLocal)
        {
            /*本地玩家*/

            Nickname_Txt.alignment = TextAlignmentOptions.MidlineRight;

            ChatBg_Rt.anchorMin = new Vector2(1, 1);
            ChatBg_Rt.anchorMax = new Vector2(1, 1);
            ChatBg_Rt.pivot = new Vector2(1, 1);
            ChatBg_Rt.anchoredPosition = new Vector2(
                -Mathf.Abs(ChatBg_Rt.anchoredPosition.x),
                ChatBg_Rt.anchoredPosition.y);

            ChatMsg_Txt.rectTransform.anchorMin = new Vector2(1, 1);
            ChatMsg_Txt.rectTransform.anchorMax = new Vector2(1, 1);
            ChatMsg_Txt.rectTransform.pivot = new Vector2(1, 1);
            ChatMsg_Txt.alignment = TextAlignmentOptions.MidlineLeft;
            ChatMsg_Txt.rectTransform.anchoredPosition = new Vector2(
                -Mathf.Abs(ChatMsg_Txt.rectTransform.anchoredPosition.x),
                ChatMsg_Txt.rectTransform.anchoredPosition.y);
        }
        else
        {
            /*其他玩家*/

            Nickname_Txt.alignment = TextAlignmentOptions.MidlineLeft;

            ChatBg_Rt.anchorMin = new Vector2(0, 1);
            ChatBg_Rt.anchorMax = new Vector2(0, 1);
            ChatBg_Rt.pivot = new Vector2(0, 1);
            ChatBg_Rt.anchoredPosition = new Vector2(
                Mathf.Abs(ChatBg_Rt.anchoredPosition.x), 
                ChatBg_Rt.anchoredPosition.y);

            ChatMsg_Txt.rectTransform.anchorMin = new Vector2(0, 1);
            ChatMsg_Txt.rectTransform.anchorMax = new Vector2(0, 1);
            ChatMsg_Txt.rectTransform.pivot = new Vector2(0, 1);
            ChatMsg_Txt.alignment = TextAlignmentOptions.MidlineLeft;
            ChatMsg_Txt.rectTransform.anchoredPosition = new Vector2(
                Mathf.Abs(ChatMsg_Txt.rectTransform.anchoredPosition.x),
                ChatMsg_Txt.rectTransform.anchoredPosition.y);
        }

        Nickname_Txt.text = chatData.Nickname;
        ChatMsg_Txt.text = chatData.ChatMsg;

        // 訊息文字大小
        float preferredWidth = ChatMsg_Txt.preferredWidth;
        float preferredHeight = ChatMsg_Txt.preferredHeight;
        float width = Mathf.Min(preferredWidth, _maxMsgWidth);
        ChatMsg_Txt.rectTransform.sizeDelta = new Vector2(width, preferredHeight);

        // 被景框大小
        float spaceWidth = ChatMsg_Txt.rectTransform.anchoredPosition.x * 2;
        float spaceHeight = ChatMsg_Txt.rectTransform.anchoredPosition.y * 2;
        ChatBg_Rt.sizeDelta = new Vector2(
            width + Mathf.Abs(spaceWidth),
            preferredHeight + Mathf.Abs(spaceHeight));

        // 項目元件大小
        Vector2 thisSizeDalta = new(
            ThisRt.sizeDelta.x,
            Nickname_Txt.rectTransform.sizeDelta.y + ChatBg_Rt.sizeDelta.y);
        ThisRt.sizeDelta = thisSizeDalta;

        return thisSizeDalta;
    }
}
