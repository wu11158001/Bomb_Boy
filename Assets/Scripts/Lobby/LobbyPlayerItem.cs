using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class LobbyPlayerItem : MonoBehaviour
{
    [SerializeField] Button Kick_Btn;
    [SerializeField] Toggle Mute_Tog;
    [SerializeField] Button MigrateHost_Btn;
    [SerializeField] GameObject Host_Obj;
    [SerializeField] TextMeshProUGUI Nickname_Txt;
    [SerializeField] TextMeshProUGUI Prepare_Txt;

    /// <summary>
    /// 初始化大廳玩家項目
    /// </summary>
    public void InitializeLobbyPlayerItem()
    {
        Kick_Btn.gameObject.SetActive(false);
        Mute_Tog.gameObject.SetActive(false);
        MigrateHost_Btn.gameObject.SetActive(false);
        Host_Obj.SetActive(false);

        Nickname_Txt.text = "";
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, "Waiting to join", (text) =>
        {
            Prepare_Txt.text = text;
        });
    }

    /// <summary>
    /// 更新大廳玩家列表
    /// </summary>
    /// <param name="lobbyPlayerData"></param>
    public void UpdateLobbyPlayerItem(PlayerData lobbyPlayerData)
    {
        bool isGameHost = lobbyPlayerData.IsGameHost;
        bool isLocalHost = NetworkManager.Singleton.IsHost;
        bool isLocal = lobbyPlayerData.NetworkClientId == NetworkManager.Singleton.LocalClientId;

        // 踢除按鈕
        Kick_Btn.gameObject.SetActive(isLocalHost && !isLocal);

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
        });

        // 房主標示
        Host_Obj.SetActive(isGameHost);

        // 暱稱文字
        Nickname_Txt.text = $"{lobbyPlayerData.Nickname}";

        // 準備文字
        if (lobbyPlayerData.IsPrepare)
        {
            LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, "Prepare", (text) =>
            {
                Prepare_Txt.gameObject.SetActive(true);
                Prepare_Txt.text = text;
            });
        }
        else
        {
            Prepare_Txt.gameObject.SetActive(false);
        }
    }
}
