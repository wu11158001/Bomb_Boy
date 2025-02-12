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

    private void OnDestroy()
    {
        if (_vivoxParticipant != null)
        {
            _vivoxParticipant.ParticipantMuteStateChanged -= UpdateVivoxUI;
            _vivoxParticipant.ParticipantSpeechDetected -= UpdateVivoxUI;
        }
    }

    private void OnDisable()
    {
        if (_vivoxParticipant != null)
        {
            _vivoxParticipant.ParticipantMuteStateChanged -= UpdateVivoxUI;
            _vivoxParticipant.ParticipantSpeechDetected -= UpdateVivoxUI;
        }
    }

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
            if (_vivoxParticipant == null)
            {
                Node_Obj.SetActive(true);
                _vivoxParticipant = VivoxManager.I.VivoxParticipantList[_itemIndex];

                // 暱稱文字
                string takeNickname =
                    _vivoxParticipant.DisplayName.Length > 6 ?
                    $"{new string(_vivoxParticipant.DisplayName.ToArray().Take(6).ToArray())}..." :
                    _vivoxParticipant.DisplayName;
                Nickname_Txt.text = takeNickname;

                // 靜音按鈕
                Mute_Tog.isOn = _vivoxParticipant.IsMuted;
                Mute_Tog.onValueChanged.RemoveAllListeners();
                Mute_Tog.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        /*靜音*/
                        _vivoxParticipant.MutePlayerLocally();
                    }
                    else
                    {
                        /*解除靜音*/
                        _vivoxParticipant.UnmutePlayerLocally();
                    }
                });

                // 語音偵測
                SpeechDetected_Obj.SetActive(false);

                _vivoxParticipant.ParticipantMuteStateChanged -= UpdateVivoxUI;
                _vivoxParticipant.ParticipantSpeechDetected -= UpdateVivoxUI;

                _vivoxParticipant.ParticipantMuteStateChanged += UpdateVivoxUI;
                _vivoxParticipant.ParticipantSpeechDetected += UpdateVivoxUI;
            } 
        }
        else
        {
            if (Node_Obj.activeSelf)
            {
                _vivoxParticipant = null;
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

    /// <summary>
    /// 更新Vivox UI
    /// </summary>
    private void UpdateVivoxUI()
    {
        if (_vivoxParticipant != null)
        {          
            // 語音偵測
            if (!_vivoxParticipant.IsMuted)
            {
                SpeechDetected_Obj.SetActive(_vivoxParticipant.SpeechDetected);
            }
        }
    }
}
