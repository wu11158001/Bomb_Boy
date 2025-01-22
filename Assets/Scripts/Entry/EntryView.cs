using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class EntryView : MonoBehaviour
{
    [Header("Debug 工具")]
    [SerializeField] GameObject DebugTool;
    [SerializeField] bool IsUsingDebugTool;

    [SerializeField] GameObject Loading_Obj;

    public void Awake()
    {
        DebugTool.SetActive(IsUsingDebugTool);
    }

    private IEnumerator Start()
    {
        Loading_Obj.SetActive(true);

        LanguageManager.I.InitializeLanguageManager();
        yield return UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"登入ID:{AuthenticationService.Instance.PlayerId}");
        };
        yield return AuthenticationService.Instance.SignInAnonymouslyAsync();

        Loading_Obj.SetActive(false);
    }
}
