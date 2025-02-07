using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Vivox;
using Unity.Services.Authentication;

public class LobbyChatArea : MonoBehaviour
{
    [SerializeField] Toggle SelfMute_Tog;
    [SerializeField] TMP_InputField Chat_If;
    [SerializeField] ScrollRect Chat_Sr;
    [SerializeField] RectTransform ChatNode;
    [SerializeField] VerticalLayoutGroup ChatNode_VLayout;
    [SerializeField] LobbyChatItem LobbyChatItemSample;
    [SerializeField] Button NewMsg_Btn;

    // 聊天區域大小Y
    private float ChatAreaSizeY = 1020;

    private void Start()
    {
        LobbyChatItemSample.gameObject.SetActive(false);
        ChatNode.sizeDelta = new Vector2(ChatNode.sizeDelta.x, 0);
        NewMsg_Btn.gameObject.SetActive(false);

        // 初始本地靜音
        VivoxManager.I.LocalMute(false);
        SelfMute_Tog.isOn = false;

        EventListener();
    }

    private void Update()
    {
        // 發送聊天訊息
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!string.IsNullOrEmpty(Chat_If.text))
            {
                VivoxManager.I.SendMessageAsync(Chat_If.text);
                Chat_If.text = "";
                Chat_If.ActivateInputField();
            }
            else
            {
                Chat_If.ActivateInputField();
            }
        }
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 本地玩家靜音Tog
        SelfMute_Tog.onValueChanged.AddListener((isOn) =>
        {
            AudioManager.I.PlaySound(SoundEnum.Click);
            VivoxManager.I.LocalMute(isOn);
        });

        // 移動至新訊息按鈕
        NewMsg_Btn.onClick.AddListener(() =>
        {
            AudioManager.I.PlaySound(SoundEnum.Click);
            MoveToNewMsg();
        });

        // 滾動條事件
        Chat_Sr.onValueChanged.AddListener((position) =>
        {
            if (IsAtBottom())
            {
                NewMsg_Btn.gameObject.SetActive(false);
            }
        });
    }

    /// <summary>
    /// 判斷滾動條是否在最底部
    /// </summary>
    /// <returns></returns>
    private bool IsAtBottom()
    {
        return Chat_Sr.verticalNormalizedPosition <= 0.01f;
    }

    /// <summary>
    /// 顯示聊天訊息
    /// </summary>
    /// <param name="chatData"></param>
    public void ShowChatMessage(ChatData chatData)
    {
        AudioManager.I.PlaySound(SoundEnum.ChatMsg);

        bool isLocal = chatData.AuthenticationPlayerId == AuthenticationService.Instance.PlayerId;
        bool isBotton = IsAtBottom();

        LobbyChatItem lobbyChatItem = Instantiate(LobbyChatItemSample, ChatNode).GetComponent<LobbyChatItem>();
        lobbyChatItem.gameObject.SetActive(true);
        Vector2 itemSizeDelta = lobbyChatItem.SetLobbyChatItem(chatData, isLocal);

        // 設置顯示區域大小
        ChatNode.sizeDelta = new Vector2(
            ChatNode.sizeDelta.x,
            ChatNode.sizeDelta.y + itemSizeDelta.y + ChatNode_VLayout.spacing);

        // 判斷是否移動至新訊息
        if (isLocal)
        {
            MoveToNewMsg();
        }
        else
        {
            if (isBotton)
            {
                MoveToNewMsg();
            }
            else
            {
                if (Mathf.Abs(ChatNode.sizeDelta.y) > ChatAreaSizeY)
                {
                    NewMsg_Btn.gameObject.SetActive(true);
                }                
            }
        }
    }

    /// <summary>
    /// 移動至新訊息
    /// </summary>
    private void MoveToNewMsg()
    {
        float posY = Mathf.Max(0, ChatNode.sizeDelta.y - ChatAreaSizeY);
        ChatNode.anchoredPosition = new Vector2(0, posY);

        NewMsg_Btn.gameObject.SetActive(false);
    }
}
