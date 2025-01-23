using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class EntryView : MonoBehaviour
{
    [Header("Debug 工具")]
    [SerializeField] GameObject DebugTool;
    [SerializeField] bool IsUsingDebugTool;

    [SerializeField] GameObject Loading_Obj;
    [SerializeField] TMP_InputField Nickname_If;
    [SerializeField] TextMeshProUGUI NicknameErrorTip_Txt;
    [SerializeField] Button EnterGame_Btn;

    [Space(30)]
    [Header("語言")]
    [SerializeField] Toggle Chinese_Tog;
    [SerializeField] Toggle English_Tog;

    private Coroutine _nicknameError_coroutine;
    private Vector2 _initNicknameErrorTxtPos;

    public void Awake()
    {
        DebugTool.SetActive(IsUsingDebugTool);
    }

    private IEnumerator Start()
    {
        Loading_Obj.SetActive(true);
        NicknameErrorTip_Txt.gameObject.SetActive(false);
        _initNicknameErrorTxtPos = NicknameErrorTip_Txt.rectTransform.anchoredPosition;

        LanguageManager.I.InitializeLanguageManager();
        yield return UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"登入ID:{AuthenticationService.Instance.PlayerId}");
        };
        yield return AuthenticationService.Instance.SignInAnonymouslyAsync();

        Loading_Obj.SetActive(false);
        switch (LanguageManager.I.CurrLanguage)
        {
            // 中文
            case 0:
                Chinese_Tog.isOn = true;
                break;

            // 英文
            case 1:
                English_Tog.isOn = true;
                break;

            // 預設中文
            default:
                Chinese_Tog.isOn = true;
                break;
        }

        string recodeNickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY);
        Nickname_If.Select();
        Nickname_If.text = recodeNickname;

        EventListener();
    }

    private void Update()
    {
        // 發送進入遊戲
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            JudgeEnterGameData();
        }
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void EventListener()
    {
        // 語言_中文
        Chinese_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                LanguageManager.I.ChangeLanguage(0);
            }
        });

        // 語言_英文
        English_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                LanguageManager.I.ChangeLanguage(1);
            }
        });

        // 暱稱輸入框
        Nickname_If.onValueChanged.AddListener((value) =>
        {
            NicknameErrorTip_Txt.gameObject.SetActive(false);
        });

        // 進入遊戲按鈕
        EnterGame_Btn.onClick.AddListener(() =>
        {
            JudgeEnterGameData();
        });
    }

    /// <summary>
    /// 暱稱錯誤
    /// </summary>
    private void OnNicknameError()
    {
        if (_nicknameError_coroutine != null) StopCoroutine(_nicknameError_coroutine);
        _nicknameError_coroutine = StartCoroutine(INicknameErrorEffect());
    }
    private IEnumerator INicknameErrorEffect()
    {
        // 抖動震幅
        float shakeAmount = 5.0f;
        // 震動次數
        int amount = 2;
        // 震動速率
        float rate = 0.05f;

        NicknameErrorTip_Txt.gameObject.SetActive(true);

        for (int i = 0; i < amount; i++)
        {
            Vector2 pos = NicknameErrorTip_Txt.rectTransform.anchoredPosition;
            pos.x = _initNicknameErrorTxtPos.x + shakeAmount;
            NicknameErrorTip_Txt.rectTransform.anchoredPosition = pos;
            yield return new WaitForSeconds(rate);

            pos.x = _initNicknameErrorTxtPos.x;
            NicknameErrorTip_Txt.rectTransform.anchoredPosition = pos;
            yield return new WaitForSeconds(rate);

            pos.x = _initNicknameErrorTxtPos.x - shakeAmount;
            NicknameErrorTip_Txt.rectTransform.anchoredPosition = pos;
            yield return new WaitForSeconds(rate);

            pos.x = _initNicknameErrorTxtPos.x;
            NicknameErrorTip_Txt.rectTransform.anchoredPosition = pos;
            yield return new WaitForSeconds(rate);
        }

        NicknameErrorTip_Txt.rectTransform.anchoredPosition = _initNicknameErrorTxtPos;
    }

    /// <summary>
    /// 進入遊戲前資料判斷
    /// </summary>
    private void JudgeEnterGameData()
    {
        // 暱稱格式錯誤
        if (Nickname_If.text.Trim().Length == 0)
        {
            OnNicknameError();
            return;
        }

        IEnterGame();
    }

    /// <summary>
    /// 進入遊戲
    /// </summary>
    /// <returns></returns>
    private async void IEnterGame()
    {
        ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
        PlayerPrefs.SetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY, Nickname_If.text);

        // 查詢主大廳
        QueryResponse queryResponse = await LobbyManager.I.QueryLobbiesAsync();
        if (queryResponse.Results.Count == 0)
        {
            /*未有主大廳*/

            await LobbyManager.I.CreateMainLobby();
        }
        else
        {
            /*加入主大廳*/

            await LobbyManager.I.JoinMainLobby(queryResponse.Results[0]);
        }

        ChangeSceneManager.I.ChangeScene(SceneEnum.Lobby);
    }
}
