using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicSwitchTool : MonoBehaviour
{
    [SerializeField] MusicTypeEnum musicType;
    [SerializeField] TextMeshProUGUI State_Txt;

    private UISwitcher.UINullableToggle _thisTog;

    // 音樂類型
    private enum MusicTypeEnum
    {
        BGM,
        Sound
    }

    private void Start()
    {
        _thisTog = GetComponent<UISwitcher.UINullableToggle>();
        _thisTog.isOn =
            musicType == MusicTypeEnum.BGM ?
            AudioManager.I.IsBGMOpen() :
            AudioManager.I.IsSoundOpen();
        _thisTog.onValueChanged.AddListener((isOn) =>
        {
            string stateKey = isOn ? "Open" : "Close";
            LanguageManager.I.GetString(LocalizationTableEnum.Options_Table, stateKey, (text) =>
            {
                State_Txt.text = text;
            });

            switch (musicType)
            {
                // BGM
                case MusicTypeEnum.BGM:
                    AudioManager.I.BGMSwitch(isOn);
                    break;

                // 音效
                case MusicTypeEnum.Sound:
                    AudioManager.I.SoundSwitch(isOn);
                    break;
            }
        });
    }
}
