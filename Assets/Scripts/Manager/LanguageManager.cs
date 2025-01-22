using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

/// <summary>
/// 語言配置表列表
/// </summary>
public enum LocalizationTableEnum
{
    Common_Table,                   // 共通
    Entry_Table,                    // 入口
    Lobby_Table,                    // 大廳
    Room_Table,                     // 房間
}

/*
 * 0 = 繁體中文
 * 1 = 英文
 */
public class LanguageManager : UnitySingleton<LanguageManager>
{
    public int CurrLanguage { get; private set; }                                           // 當前語言

    /// <summary>
    /// 初始化語言管理
    /// </summary>
    public void InitializeLanguageManager()
    {
        int localLanguage = PlayerPrefs.GetInt(LocalDataKeyManager.LOCAL_LANGUAGE_KEY);
        ChangeLanguage(localLanguage);
    }

    /// <summary>
    /// 設置文字
    /// </summary>
    /// <param name="table"></param>
    /// <param name="txt"></param>
    /// <param name="key"></param>
    public void SetText(TextMeshProUGUI txt, LocalizationTableEnum table, string key, string otherStr = "")
    {
        StartCoroutine(ISetText(txt, table, key, otherStr));
    }
    private IEnumerator ISetText(TextMeshProUGUI txt, LocalizationTableEnum table, string key, string otherStr = "")
    {
        var loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync($"{table}");
        yield return loadingOperation;

        if (loadingOperation.Status == AsyncOperationStatus.Succeeded)
        {
            var stringTable = loadingOperation.Result;
            txt.text = $"{stringTable.GetEntry(key).GetLocalizedString()}{otherStr}";
        }
        else
        {
            Debug.LogError($"無法載入語言表:{table} , 錯誤:{loadingOperation.OperationException}");
        }
    }

    /// <summary>
    /// 更換語言
    /// </summary>
    /// <param name="index"></param>
    public void ChangeLanguage(int index)
    {
        AsyncOperationHandle handle = LocalizationSettings.SelectedLocaleAsync;
        if (handle.IsDone)
        {
            SetLanguage(index);
        }
        else
        {
            handle.Completed += (OperationHandle) =>
            {
                SetLanguage(index);
            };
        }
    }

    /// <summary>
    /// 設置語言
    /// </summary>
    /// <param name="index"></param>
    private void SetLanguage(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt(LocalDataKeyManager.LOCAL_LANGUAGE_KEY, index);
        CurrLanguage = index;

        Debug.Log($"當前語言: {index}");
    }
}
