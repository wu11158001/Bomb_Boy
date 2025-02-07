using UnityEngine;
using UnityEngine.UI;

public class MicTogTool : MonoBehaviour
{

    private void Start()
    {
        Toggle Mic_Tog = GetComponent<Toggle>();

        // 麥克風Tog
        Mic_Tog.onValueChanged.AddListener((isOn) =>
        {
            Mic_Tog.targetGraphic.enabled = isOn ? false : true;
        });
    }
}
