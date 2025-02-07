using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;
using System;
using Unity.Services.Authentication;

public class GameView : MonoBehaviour
{
    [SerializeField] Button ExitGame_Btn;
    [SerializeField] TextMeshProUGUI GameCd_Txt;
    [SerializeField] TextMeshProUGUI GameResult_Txt;
    [SerializeField] TextMeshProUGUI Winner_Txt;
    [SerializeField] TextMeshProUGUI GameStartCD_Txt;
    [SerializeField] TextMeshProUGUI ReturnLobbyCD_Txt;
    [SerializeField] GameObject InputController_Obj;
    [SerializeField] GamePlayerItemArea _gamePlayerItemArea;

    private void Awake()
    {
        ViewManager.I.ResetViewData();
    }

    private void Start()
    {
        bool isHaveedData = false;
        GamePlayerData gamePlayerData = new();
        for (int i = 0; i < GameRpcManager.I.GamePlayerData_List.Count; i++)
        {
            if (GameRpcManager.I.GamePlayerData_List[i].AuthenticationPlayerId == AuthenticationService.Instance.PlayerId)
            {
                isHaveedData = true;
                gamePlayerData = GameRpcManager.I.GamePlayerData_List[i];
                break;
            }
        }

        if (isHaveedData == false)
        {
            /*初入遊戲*/
            GameRpcManager.I.InGameSceneServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            /*斷線重連*/
            GameRpcManager.I.ReconnectServerRpc(gamePlayerData, NetworkManager.Singleton.LocalClientId);
            ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.LoadingView);
        }        

        ExitGame_Btn.gameObject.SetActive(false);
        GameResult_Txt.gameObject.SetActive(false);
        Winner_Txt.gameObject.SetActive(false);
        GameStartCD_Txt.gameObject.SetActive(false);
        ReturnLobbyCD_Txt.gameObject.SetActive(false);
        InputController_Obj.SetActive(true);

        EventListener();

        // 紀錄斷線重連資料
        PlayerPrefs.SetString(LocalDataKeyManager.LOCAL_JOIN_LOBBY_ID, LobbyManager.I.JoinedLobby.Id);

        AudioManager.I.PlayBGM(BGNEnum.Game);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 退出遊戲按鈕
        ExitGame_Btn.onClick.AddListener(async () =>
        {
            AudioManager.I.PlaySound(SoundEnum.Cancel);

            ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
            await LobbyManager.I.LeaveLobby();
            ChangeSceneManager.I.ChangeScene(SceneEnum.Entry);
        });
    }

    /// <summary>
    /// 顯示遊戲場景
    /// </summary>
    public void ShowGameScene()
    {
        ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.LoadingView);
    }

    /// <summary>
    /// 顯示退出遊戲按鈕
    /// </summary>
    public void ShowExitBtn()
    {
        ExitGame_Btn.gameObject.SetActive(true);
    }

    /// <summary>
    /// 顯示遊戲結果
    /// </summary>
    /// <param name="isDraw">是否平手</param>
    /// <param name="winnerData">獲勝玩家資料</param>
    public void ShowGameResult(bool isDraw, GamePlayerData winnerData)
    {
        ShowExitBtn();
        GameResult_Txt.gameObject.SetActive(true);

        if (isDraw)
        {
            /*平手*/

            AudioManager.I.PlaySound(SoundEnum.GameOver);

            LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, "Draw", (text) =>
            {
                GameResult_Txt.text = $"<color=#57E2B3>{text}</color>";
            });
        }
        else
        {
            /*有玩家獲勝*/

            bool isVictory = winnerData.AuthenticationPlayerId == AuthenticationService.Instance.PlayerId;
            string resultStr =
                isVictory ?
                $"Victory" :
                $"Game Over";

            string color =
                isVictory ?
                "#E2C357" :
                "#57BBE2";

            AudioManager.I.PlaySound(isVictory ? SoundEnum.Win :SoundEnum.GameOver);

            LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, resultStr, (text) =>
            {
                GameResult_Txt.text = $"<color={color}>{text}</color>";
            });


            Winner_Txt.gameObject.SetActive(!isVictory);
            LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, "Winning Player", (text) =>
            {
                Winner_Txt.text = $"{text}: {winnerData.Nickname}";
            });
        }
    }

    /// <summary>
    /// 顯示開始遊戲倒數
    /// </summary>
    public void ShowStartGameCD()
    {
        InputController_Obj.SetActive(true);
        GameStartCD_Txt.gameObject.SetActive(true);

        AudioManager.I.PlaySound(SoundEnum.Ready);

        LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, "Ready!", (text) =>
        {
            GameStartCD_Txt.text = $"{text}";
        });
    }

    /// <summary>
    /// 顯示遊戲開始
    /// </summary>
    public void ShowGameStart()
    {
        InputController_Obj.SetActive(false);

        LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, "GO!", (text) =>
        {
            GameStartCD_Txt.text = $"{text}";
        });

        StartCoroutine(ICloseGameStartTet());
        _gamePlayerItemArea.ItemSwitchEffect(false);
    }

    /// <summary>
    /// 顯示倒數
    /// </summary>
    public void DisplayGameTime()
    {
        int minute = GameRpcManager.I.GameTimeCd_NV.Value / 60;
        int second = GameRpcManager.I.GameTimeCd_NV.Value % 60;
        GameCd_Txt.text = $"{minute:D2} : {second:D2}";
    }

    /// <summary>
    /// 開始遊戲文字關閉效果
    /// </summary>
    /// <returns></returns>
    private IEnumerator ICloseGameStartTet()
    {
        float during = 2;

        DateTime startTime = DateTime.Now;
        Color color = GameStartCD_Txt.color;
        while ((DateTime.Now - startTime).TotalSeconds < during)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / during;
            float a = Mathf.Lerp(1, 0, progress);
            color.a = a;
            GameStartCD_Txt.color = color;

            yield return null;
        }

        GameStartCD_Txt.gameObject.SetActive(false);
    }

    /// <summary>
    /// 顯示回到大廳倒數
    /// </summary>
    /// <param name="num"></param>
    public void ShowReturnToLobbyCD(int num)
    {
        ReturnLobbyCD_Txt.gameObject.SetActive(true);

        LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, "Return to the lobby", (text) =>
        {
            ReturnLobbyCD_Txt.text = $"{text}: {num}";
        });
    }
}
