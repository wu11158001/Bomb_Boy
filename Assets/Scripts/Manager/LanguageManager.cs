using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

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
    /// 獲取文字
    /// </summary>
    /// <param name="table"></param>
    /// <param name="key"></param>
    /// <param name="callback"></param>
    public void GetString(LocalizationTableEnum table, string key, UnityAction<string> callback)
    {
        StartCoroutine(ISetText(table, key, callback));
    }
    private IEnumerator ISetText(LocalizationTableEnum table, string key, UnityAction<string> callback)
    {
        var loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync($"{table}");
        yield return loadingOperation;

        if (loadingOperation.Status == AsyncOperationStatus.Succeeded)
        {
            var stringTable = loadingOperation.Result;
            callback?.Invoke(stringTable.GetEntry(key).GetLocalizedString());
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
