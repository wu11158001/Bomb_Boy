using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerItem : MonoBehaviour
{
    [SerializeField] Button Kick_Btn;
    [SerializeField] Button Mute_Btn;
    [SerializeField] Button MigrateHost_Btn;
    [SerializeField] GameObject Host_Obj;
    [SerializeField] TextMeshProUGUI Nickname_Txt;
    [SerializeField] TextMeshProUGUI Prepare_Obj;

    /// <summary>
    /// 初始化大廳玩家項目
    /// </summary>
    public void InitializeLobbyPlayerItem()
    {
        Kick_Btn.gameObject.SetActive(false);
        Mute_Btn.gameObject.SetActive(false);
        MigrateHost_Btn.gameObject.SetActive(false);
        Host_Obj.SetActive(false);

        Nickname_Txt.text = "";
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, "Waiting to join", (text) =>
        {
            Prepare_Obj.text = text;
        });
    }
}
