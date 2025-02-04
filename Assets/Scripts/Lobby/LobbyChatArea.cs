using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Vivox;
using Unity.Services.Authentication;

public class LobbyChatArea : MonoBehaviour
{
    [SerializeField] Toggle SelfMute_Tog;
    [SerializeField] TMP_InputField Chat_If;
    [SerializeField] RectTransform ChatNode;
    [SerializeField] LobbyChatItem LobbyChatItemSample;

    // 本地玩家VivoxParticipant
    private VivoxParticipant _localVivoxParticipant;

    private void Start()
    {
        LobbyChatItemSample.gameObject.SetActive(false);

        // 初始本地靜音
        VivoxManager.I.LocalMute(true);
        // 獲取本地玩家VivoxParticipant
        foreach (var participant in VivoxManager.I.VivoxParticipantList)
        {
            if (participant.IsSelf)
            {
                _localVivoxParticipant = participant;
                break;
            }
        }
        SelfMute_Tog.isOn = !_localVivoxParticipant.IsMuted;
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
            VivoxManager.I.LocalMute(isOn);
        });
    }

    /// <summary>
    /// 顯示聊天訊息
    /// </summary>
    /// <param name="chatData"></param>
    public void ShowChatMessage(ChatData chatData)
    {
        bool isLocal = chatData.AuthenticationPlayerId == AuthenticationService.Instance.PlayerId;

        LobbyChatItem lobbyChatItem = Instantiate(LobbyChatItemSample, ChatNode).GetComponent<LobbyChatItem>();
        lobbyChatItem.gameObject.SetActive(true);
        lobbyChatItem.SetLobbyChatItem(chatData, isLocal);
    }
}
