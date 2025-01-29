using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;
using System;

public class GameView : MonoBehaviour
{
    [SerializeField] Button ExitGame_Btn;
    [SerializeField] TextMeshProUGUI GameResult_Txt;
    [SerializeField] TextMeshProUGUI Winner_Txt;
    [SerializeField] TextMeshProUGUI GameStartCD_Txt;
    [SerializeField] TextMeshProUGUI ReturnLobbyCD_Txt;

    private void Awake()
    {
        ViewManager.I.ResetViewData();
    }

    private void Start()
    {
        GameRpcManager.I.InGameSceneServerRpc(NetworkManager.Singleton.LocalClientId);

        ExitGame_Btn.gameObject.SetActive(false);
        GameResult_Txt.gameObject.SetActive(false);
        Winner_Txt.gameObject.SetActive(false);
        GameStartCD_Txt.gameObject.SetActive(false);
        ReturnLobbyCD_Txt.gameObject.SetActive(false);

        EventListener();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 退出遊戲按鈕
        ExitGame_Btn.onClick.AddListener(async () =>
        {
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

            LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, "Draw", (text) =>
            {
                GameResult_Txt.text = text;
            });
        }
        else
        {
            /*有玩家獲勝*/

            bool isVictory = winnerData.NetworkClientId == NetworkManager.Singleton.LocalClientId;
            string resultStr =
                isVictory ?
                $"Victory" :
                $"Game Over";

            LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, resultStr, (text) =>
            {
                GameResult_Txt.text = text;
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
        GameStartCD_Txt.gameObject.SetActive(true);

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
        LanguageManager.I.GetString(LocalizationTableEnum.Game_Table, "GO!", (text) =>
        {
            GameStartCD_Txt.text = $"{text}";
        });

        StartCoroutine(ICloseGameStartTet());
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
