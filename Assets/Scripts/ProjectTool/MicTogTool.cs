using UnityEngine;
using UnityEngine.UI;

public class MicTogTool : MonoBehaviour
{
    [SerializeField] Color BgColor;
    [SerializeField] Color OpenColor;
    [SerializeField] Color CloseColor;
    [SerializeField] Toggle Mic_Tog;

    private void Start()
    {
        // 麥克風Tog
        Mic_Tog.onValueChanged.AddListener((isOn) =>
        {
            Mic_Tog.targetGraphic.color =
                isOn ?
                BgColor :
                OpenColor;

            Mic_Tog.graphic.color = CloseColor;
        });
    }
}
