using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Services.Vivox;
using System.Linq;
using System.Collections;

public class LobbyPlayerItem : MonoBehaviour
{
    [SerializeField] Button Kick_Btn;
    [SerializeField] Toggle Mute_Tog;
    [SerializeField] Button MigrateHost_Btn;
    [SerializeField] GameObject Host_Obj;
    [SerializeField] TextMeshProUGUI Nickname_Txt;
    [SerializeField] TextMeshProUGUI Prepare_Txt;
    [SerializeField] GameObject SpeechDetected_Obj;
    [SerializeField] GameObject SelfIcon_Obj;

    private LobbyPlayerData _previousLobbyPlayerData;
    private VivoxParticipant _vivoxParticipant;

    private void OnDestroy()
    {
        if (_vivoxParticipant != null)
        {
            _vivoxParticipant.ParticipantMuteStateChanged -= UpdateVivoxUI;
            _vivoxParticipant.ParticipantSpeechDetected -= UpdateVivoxUI;
        }
    }

    /// <summary>
    /// 初始化大廳玩家項目
    /// </summary>
    public void InitializeLobbyPlayerItem(int playerIndex)
    {
        Kick_Btn.gameObject.SetActive(false);
        Mute_Tog.gameObject.SetActive(false);
        MigrateHost_Btn.gameObject.SetActive(false);
        Host_Obj.SetActive(false);
        SpeechDetected_Obj.SetActive(false);
        SelfIcon_Obj.SetActive(false);

        Nickname_Txt.text = "";
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, "Waiting to join", (text) =>
        {
            Prepare_Txt.text = $"<color=#908C84>{text}</color>";
        });

        _vivoxParticipant = null;
    }

    /// <summary>
    /// 更新大廳玩家列表
    /// </summary>
    /// <param name="lobbyPlayerData"></param>
    public void UpdateLobbyPlayerItem(LobbyPlayerData lobbyPlayerData)
    {
        if (LobbyManager.I.JoinedLobby == null)
        {
            return;
        }

        _vivoxParticipant = VivoxManager.I.VivoxParticipantList.Where(x => x.PlayerId == lobbyPlayerData.AuthenticationPlayerId).FirstOrDefault();
        if (_vivoxParticipant == null)
        {
            StartCoroutine(IGetVivoxParticipant(lobbyPlayerData));
        }
        else
        {
            VivoxParticipantEvent();
        }

        bool isLobbyHost = LobbyManager.I.JoinedLobby.HostId == lobbyPlayerData.AuthenticationPlayerId;
        bool isLocalHost = LobbyManager.I.IsLobbyHost();
        bool isLocal = lobbyPlayerData.NetworkClientId == NetworkManager.Singleton.LocalClientId;

        SelfIcon_Obj.SetActive(isLocal);

        // 踢除按鈕
        Kick_Btn.gameObject.SetActive(isLocalHost && !isLocal);
        Kick_Btn.onClick.RemoveAllListeners();
        Kick_Btn.onClick.AddListener(() =>
        {
            AudioManager.I.PlaySound(SoundEnum.Click);
            LobbyRpcManager.I.KickLobbyPlayerServerRpc(lobbyPlayerData.NetworkClientId);
            Kick_Btn.onClick.RemoveAllListeners();
        });

        // 靜音按鈕
        Mute_Tog.gameObject.SetActive(!isLocal);

        // 交換房主按鈕
        /*MigrateHost_Btn.gameObject.SetActive(isLocalHost && !isLocal);
        MigrateHost_Btn.onClick.RemoveAllListeners();
        MigrateHost_Btn.onClick.AddListener(() =>
        {
            AudioManager.I.PlaySound(SoundEnum.Click);
            LobbyRpcManager.I.MigrateHostNotifyServerRpc(lobbyPlayerData.AuthenticationPlayerId);
        });*/

        // 房主標示
        Host_Obj.SetActive(isLobbyHost);

        // 暱稱文字
        Nickname_Txt.text = $"{lobbyPlayerData.Nickname}";

        // 準備文字
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, "Prepare", (text) =>
        {
            if (lobbyPlayerData.IsPrepare)
            {
                Prepare_Txt.text = $"<color=#FFB93C>{text}</color>";
            }
            else
            {
                Prepare_Txt.text = "";
            }

            // 音效
            if (_previousLobbyPlayerData.IsPrepare != lobbyPlayerData.IsPrepare &&
                _previousLobbyPlayerData.Nickname == lobbyPlayerData.Nickname)
            {
                if (lobbyPlayerData.IsPrepare)
                {
                    AudioManager.I.PlaySound(SoundEnum.Click);
                }
                else
                {
                    AudioManager.I.PlaySound(SoundEnum.Cancel);
                }
            }
        });

        _previousLobbyPlayerData = lobbyPlayerData;
    }

    /// <summary>
    /// 獲取Vivox參與者
    /// </summary>
    /// <param name="lobbyPlayerData"></param>
    /// <returns></returns>
    private IEnumerator IGetVivoxParticipant(LobbyPlayerData lobbyPlayerData)
    {
        yield return new WaitForSeconds(1);
        _vivoxParticipant = VivoxManager.I.VivoxParticipantList.Where(x => x.PlayerId == lobbyPlayerData.AuthenticationPlayerId).FirstOrDefault();

        if (_vivoxParticipant == null)
        {
            yield return IGetVivoxParticipant(lobbyPlayerData);
        }
        else
        {
            VivoxParticipantEvent();
        }
    }

    /// <summary>
    /// Vivox事件註冊
    /// </summary>
    private void VivoxParticipantEvent()
    {
        _vivoxParticipant.ParticipantMuteStateChanged -= UpdateVivoxUI;
        _vivoxParticipant.ParticipantSpeechDetected -= UpdateVivoxUI;

        _vivoxParticipant.ParticipantMuteStateChanged += UpdateVivoxUI;
        _vivoxParticipant.ParticipantSpeechDetected += UpdateVivoxUI;
    }

    /// <summary>
    /// 更新Vivox UI
    /// </summary>
    private void UpdateVivoxUI()
    {
        if (_vivoxParticipant != null)
        {
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
            if (!_vivoxParticipant.IsMuted)
            {
                SpeechDetected_Obj.SetActive(_vivoxParticipant.SpeechDetected);
            }
        }
    }
}
