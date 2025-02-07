using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Vivox;
using System.Linq;

public class GamePlayerItem : MonoBehaviour
{
    [SerializeField] GameObject Node_Obj;
    [SerializeField] TextMeshProUGUI Nickname_Txt;
    [SerializeField] GameObject SpeechDetected_Obj;
    [SerializeField] Toggle Mute_Tog;

    private int _itemIndex;
    private VivoxParticipant _vivoxParticipant;

    private void Awake()
    {
        _itemIndex = -1;
    }

    private void Start()
    {
        EventListener();
    }

    private void Update()
    {
        if (_itemIndex < 0) return;

        if (VivoxManager.I.VivoxParticipantList.Count - 1 >= _itemIndex)
        {
            if (!Node_Obj.activeSelf)
            {
                Node_Obj.SetActive(true);
            }

            _vivoxParticipant = VivoxManager.I.VivoxParticipantList[_itemIndex];
            Mute_Tog.isOn = _vivoxParticipant.IsMuted;

            // 暱稱文字
            string takeNickname =
                _vivoxParticipant.DisplayName.Length > 6 ?
                $"{new string(_vivoxParticipant.DisplayName.ToArray().Take(6).ToArray())}..." :
                _vivoxParticipant.DisplayName;
            Nickname_Txt.text = takeNickname;

            // 語音偵測
            SpeechDetected_Obj.SetActive(_vivoxParticipant.SpeechDetected);
        }
        else
        {
            if (Node_Obj.activeSelf)
            {
                Node_Obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 靜音Tog
        Mute_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (_vivoxParticipant != null)
            {
                if (_vivoxParticipant.IsSelf)
                {
                    /*本地靜音*/

                    VivoxManager.I.LocalMute(isOn);
                }
                else
                {
                    /*其他玩家靜音*/

                    if (isOn)
                    {
                        _vivoxParticipant.MutePlayerLocally();
                    }
                    else
                    {
                        _vivoxParticipant.UnmutePlayerLocally();
                    }
                }
            }
        });
    }

    /// <summary>
    /// 獲取項目激活狀態
    /// </summary>
    /// <returns></returns>
    public bool GetNodeObjActive()
    {
        return Node_Obj.activeSelf;
    }

    /// <summary>
    /// 設置遊戲玩家項目編號
    /// </summary>
    /// <param name="index"></param>
    public void SetItemIndex(int index)
    {
        _itemIndex = index;
    }
}
