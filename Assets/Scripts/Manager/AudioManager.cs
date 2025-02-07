using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class AudioManager : UnitySingleton<AudioManager>
{
    private AudioSource _bgmAudioSource;

    // 音效池
    private readonly Dictionary<GameObject, AudioSource> _soundDic = new();
    // 限制音效紀錄
    private readonly Dictionary<SoundEnum, DateTime> _limitSoundDic = new();

    // 音效清理數
    private int _clear = 10;

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void Start()
    {
        _bgmAudioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 播放BGM
    /// </summary>
    /// <param name="bgm"></param>
    /// <param name="volume"></param>
    public void PlayBGM(BGNEnum bgm, float volume = 0.3f)
    {
        AudioClip clip = SOManager.I.AudioClip_SO.BGMList[(int)bgm];
        _bgmAudioSource.clip = clip;
        _bgmAudioSource.loop = true;
        _bgmAudioSource.volume = volume;

        if (!IsBGMOpen()) return;
        _bgmAudioSource.Play();
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="isLimitSound">限制音效</param>
    /// <param name="volume"></param>
    public void PlaySound(SoundEnum sound, bool isLimitSound = false, float volume = 0.5f)
    {
        if (!IsSoundOpen()) return;

        AudioClip clip = SOManager.I.AudioClip_SO.SoundList[(int)sound];

        // 限制音效判斷
        if (isLimitSound)
        {
            if (_limitSoundDic.ContainsKey(sound))
            {
                if ((DateTime.Now - _limitSoundDic[sound]).TotalSeconds < 0.1f)
                {                    
                    return;
                }
                else
                {
                    _limitSoundDic[sound] = DateTime.Now;
                }
            }
            else
            {
                _limitSoundDic.Add(sound, DateTime.Now);
            }
        }

        // 播放/產生音效
        foreach (var audio in _soundDic)
        {
            if (!audio.Key.activeSelf)
            {
                audio.Key.SetActive(true);
                audio.Key.name = $"{sound}";
                audio.Value.clip = clip;
                audio.Value.volume = volume;
                audio.Value.Play();

                //清理音效物件池
                if (_soundDic.Count >= _clear)
                {
                    List<GameObject> cleanList = new();
                    foreach (var usingObj in _soundDic)
                    {
                        if (!usingObj.Key.activeSelf)
                        {
                            cleanList.Add(usingObj.Key);
                        }
                    }

                    foreach (var cleanObj in cleanList)
                    {
                        _soundDic.Remove(cleanObj);
                        Destroy(cleanObj);
                    }
                }

                // 播放完畢協程
                StartCoroutine(ISoundFinished(audio.Value));
                return;
            }
        }

        GameObject newAduidObj = new();
        newAduidObj.transform.SetParent(transform);
        newAduidObj.name = $"{sound}";
        AudioSource audioSource = newAduidObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.Play();

        _soundDic.Add(newAduidObj, audioSource);
        StartCoroutine(ISoundFinished(audioSource));
    }

    /// <summary>
    /// 播放完畢協程
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private IEnumerator ISoundFinished(AudioSource source)
    {
        yield return new WaitForSecondsRealtime(source.clip.length);
        source.gameObject.SetActive(false);
    }

    /// <summary>
    /// BGM是否開啟
    /// </summary>
    /// <returns></returns>
    public bool IsBGMOpen()
    {
        string bgm = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_BGM);
        bool isOpen =
            string.IsNullOrEmpty(bgm) ?
            true :
            bgm == "true" ? true : false;

        return isOpen;
    }

    /// <summary>
    /// 音效是否開啟
    /// </summary>
    /// <returns></returns>
    public bool IsSoundOpen()
    {
        string sound = PlayerPrefs.GetString(LocalDataKeyManager.LOCAL_SOUND);
        bool isOpen =
            string.IsNullOrEmpty(sound) ?
            true :
            sound == "true" ? true : false;

        return isOpen;
    }

    /// <summary>
    /// BGM開關
    /// </summary>
    /// <param name="isOpen"></param>
    public void BGMSwitch(bool isOpen)
    {
        if (isOpen) _bgmAudioSource.Play();
        else _bgmAudioSource.Pause();

        PlayerPrefs.SetString(LocalDataKeyManager.LOCAL_BGM, isOpen ? "true" : "false");
    }

    /// <summary>
    /// 音效開關
    /// </summary>
    /// <param name="isOpen"></param>
    public void SoundSwitch(bool isOpen)
    {
        PlayerPrefs.SetString(LocalDataKeyManager.LOCAL_SOUND, isOpen ? "true" : "false");
    }
}
