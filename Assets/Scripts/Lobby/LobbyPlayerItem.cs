using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Services.Vivox;
using System.Linq;

public class LobbyPlayerItem : MonoBehaviour
{
    [SerializeField] Button Kick_Btn;
    [SerializeField] Toggle Mute_Tog;
    [SerializeField] Button MigrateHost_Btn;
    [SerializeField] GameObject Host_Obj;
    [SerializeField] TextMeshProUGUI Nickname_Txt;
    [SerializeField] TextMeshProUGUI Prepare_Txt;
    [SerializeField] GameObject SpeechDetected_Obj;

    private VivoxParticipant _vivoxParticipant;

    private void Update()
    {
        if (_vivoxParticipant != null)
        {
            SpeechDetected_Obj.SetActive(_vivoxParticipant.SpeechDetected);
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

        Nickname_Txt.text = "";
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, "Waiting to join", (text) =>
        {
            Prepare_Txt.text = text;
        });

        _vivoxParticipant = null;
    }

    /// <summary>
    /// 更新大廳玩家列表
    /// </summary>
    /// <param name="lobbyPlayerData"></param>
    public void UpdateLobbyPlayerItem(LobbyPlayerData lobbyPlayerData)
    {
        bool isLobbyHost = LobbyManager.I.JoinedLobby.HostId == lobbyPlayerData.AuthenticationPlayerId;
        bool isLocalHost = LobbyManager.I.IsLobbyHost();
        bool isLocal = lobbyPlayerData.NetworkClientId == NetworkManager.Singleton.LocalClientId;

        // 踢除按鈕
        Kick_Btn.gameObject.SetActive(isLocalHost && !isLocal);
        Kick_Btn.onClick.RemoveAllListeners();
        Kick_Btn.onClick.AddListener(() =>
        {
            LobbyRpcManager.I.KickLobbyPlayerServerRpc(lobbyPlayerData.NetworkClientId);
            Kick_Btn.onClick.RemoveAllListeners();
        });

        // 靜音按鈕
        Mute_Tog.gameObject.SetActive(!isLocal);
        Mute_Tog.isOn = true;
        Mute_Tog.onValueChanged.RemoveAllListeners();
        Mute_Tog.onValueChanged.AddListener((isOn) =>
        {
        });

        // 交換房主按鈕
        MigrateHost_Btn.gameObject.SetActive(isLocalHost && !isLocal);
        MigrateHost_Btn.onClick.RemoveAllListeners();
        MigrateHost_Btn.onClick.AddListener(() =>
        {
            LobbyRpcManager.I.MigrateHostNotifyServerRpc(lobbyPlayerData.AuthenticationPlayerId);
        });

        // 房主標示
        Host_Obj.SetActive(isLobbyHost);

        // 暱稱文字
        Nickname_Txt.text = $"{lobbyPlayerData.Nickname}";

        // 準備文字
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, "Prepare", (text) =>
        {
            if (lobbyPlayerData.IsPrepare)
            {
                Prepare_Txt.text = text;
            }
            else
            {
                Prepare_Txt.text = "";
            }            
        });

        _vivoxParticipant = VivoxManager.I.VivoxParticipantList.Where(x => x.PlayerId == lobbyPlayerData.AuthenticationPlayerId).FirstOrDefault();
    }
}
