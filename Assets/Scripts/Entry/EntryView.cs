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
using Unity.Netcode;
using Unity.Services.Vivox;
using Unity.Multiplayer.Tools.NetStatsMonitor;

public class EntryView : MonoBehaviour
{
    [Header("Debug 工具")]
    [SerializeField] Canvas DebugToolCanvas;
    [SerializeField] bool IsUsingDebugTool;

    [Space(30)]
    [Header("流量檢測")]
    [SerializeField] RuntimeNetStatsMonitor RuntimeNetStatsMonitor;

    [SerializeField] GameObject Loading_Obj;
    [SerializeField] InputField Nickname_If;
    [SerializeField] TextMeshProUGUI NicknameErrorTip_Txt;
    [SerializeField] Button JoinLobby_Btn;

    [Space(30)]
    [Header("語言")]
    [SerializeField] Toggle Chinese_Tog;
    [SerializeField] Toggle English_Tog;

    private Coroutine _nicknameError_coroutine;
    private Vector2 _initNicknameErrorTxtPos;

    private void OnDestroy()
    {
        AuthenticationService.Instance.SignedIn -= OnSignedIn;        
    }

    public void Awake()
    {
        DebugToolCanvas = GameObject.Find("IngameDebugConsole").GetComponent<Canvas>(); 
        DebugToolCanvas.enabled = false;
    }

    private IEnumerator Start()
    {
        Loading_Obj.SetActive(true);
        NicknameErrorTip_Txt.gameObject.SetActive(false);
        _initNicknameErrorTxtPos = NicknameErrorTip_Txt.rectTransform.anchoredPosition;

        // 等待NGO斷線
        while (NetworkManager.Singleton.IsListening)
        {
            yield return null;
        }

        if (!GameDataManager.I.IsLogined)
        {
            // 初始化
            LanguageManager.I.InitializeLanguageManager();
            yield return UnityServices.InitializeAsync();

            // 用戶登入
            AuthenticationService.Instance.SignedIn += OnSignedIn;
            yield return AuthenticationService.Instance.SignInAnonymouslyAsync();

            // Vivox初始化
            yield return VivoxService.Instance.InitializeAsync();
            VivoxManager.I.BindVivoxEvents();
        }       

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

        AudioManager.I.PlayBGM(BGNEnum.EntryAndLobby);

        string recodeNickname = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY);

        Nickname_If.ActivateInputField();
        Nickname_If.text = recodeNickname;

        Loading_Obj.SetActive(false);
        ViewManager.I.ClosePermanentView<RectTransform>(PermanentViewEnum.LoadingView);
        EventListener();

        GameDataManager.I.IsLogined = true;
    }

    private void Update()
    {
        // 開啟Debug工具
        if (Input.GetKeyDown(KeyCode.RightAlt))
        {
            IsUsingDebugTool = !IsUsingDebugTool;
            DebugToolCanvas.enabled = IsUsingDebugTool;
        }

        // 開啟流量檢測
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            RuntimeNetStatsMonitor.Visible = !RuntimeNetStatsMonitor.Visible;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            LobbyManager.I.Query();
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
                AudioManager.I.PlaySound(SoundEnum.Click);
                LanguageManager.I.ChangeLanguage(0);
            }
        });

        // 語言_英文
        English_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                AudioManager.I.PlaySound(SoundEnum.Click);
                LanguageManager.I.ChangeLanguage(1);
            }
        });

        // 暱稱輸入框
        Nickname_If.onValueChanged.AddListener((value) =>
        {
            NicknameErrorTip_Txt.gameObject.SetActive(false);
        });

        // 加入大廳按鈕
        JoinLobby_Btn.onClick.AddListener(() =>
        {
            AudioManager.I.PlaySound(SoundEnum.Confirm);
            JudgeEnterGameData();
        });
    }

    /// <summary>
    /// 登入完成事件
    /// </summary>
    private void OnSignedIn()
    {
        Debug.Log($"登入ID:{AuthenticationService.Instance.PlayerId}");        
        ReconnectHandle();
    }

    /// <summary>
    /// 重新連線處理
    /// </summary>
    private async void ReconnectHandle()
    {
        string lobbyJoinId = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_JOIN_LOBBY_ID);
        PlayerPrefs.SetString(LocalDataKeyManager.LOCAL_JOIN_LOBBY_ID, "");

        // 嘗試斷線重連
        if (!string.IsNullOrEmpty(lobbyJoinId))
        {
            Debug.Log("嘗試斷線重連");
            Lobby lobby = await LobbyManager.I.ReconnectQueryLobby(lobbyJoinId);

            if (lobby != null)
            {
                LanguageManager.I.GetString(LocalizationTableEnum.Ask_Table, "There is an unfinished game, would you like to enter it?", (text) =>
                {
                    ViewManager.I.OpenView<AskView>(ViewEnum.AskView, (view) =>
                    {
                        view.SetAskView(text, async () =>
                        {
                            ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
                            bool isJoin = await LobbyManager.I.JoinLobby(lobby);

                            // 加入失敗
                            if (!isJoin)
                            {
                                Debug.Log("重新連線加入大廳失敗!");
                                NetworkManager.Singleton.Shutdown(true);
                                EnterLobby();
                            }
                        });
                    });
                });
            }           
        }
    }

    /// <summary>
    /// 暱稱錯誤
    /// </summary>
    private void OnNicknameError()
    {
        if (_nicknameError_coroutine != null) StopCoroutine(_nicknameError_coroutine);
        _nicknameError_coroutine = StartCoroutine(INicknameErrorEffect());
    }

    /// <summary>
    /// 暱稱錯誤效果
    /// </summary>
    /// <returns></returns>
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

        EnterLobby();
    }

    /// <summary>
    /// 進入大廳
    /// </summary>
    /// <returns></returns>
    private async void EnterLobby()
    {
        ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
        PlayerPrefs.SetString(LocalDataKeyManager.LOCAL_NICKNAME_KEY, Nickname_If.text);

        await LobbyManager.I.QuickJoinLobby();
    }
}
