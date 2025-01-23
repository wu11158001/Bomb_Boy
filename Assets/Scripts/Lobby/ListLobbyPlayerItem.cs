using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class ListLobbyPlayerItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Nickname_Txt;
    [SerializeField] TextMeshProUGUI GameState_Txt;

    /// <summary>
    /// 設置大廳玩家項目
    /// </summary>
    /// <param name="player"></param>
    public void SetListLobbyPlayerItem(Player player)
    {
        Nickname_Txt.text = player.Data[$"{LobbyPlayerDataKey.PlayerNickname}"].Value;

        string gameState = player.Data[$"{LobbyPlayerDataKey.GameState}"].Value;
        LanguageManager.I.GetString(LocalizationTableEnum.Lobby_Table, gameState, (text) =>
        {
            GameState_Txt.text = text;
        });
    }
}
