using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneManager : UnitySingleton<ChangeSceneManager>
{
    /// <summary>
    /// 轉換場景
    /// </summary>
    /// <param name="scene"></param>
    public void ChangeScene(SceneEnum scene)
    {
        ViewManager.I.OpenPermanentView<RectTransform>(PermanentViewEnum.LoadingView);
        StartCoroutine(ILoadSceneAsync(scene));
    }
    private IEnumerator ILoadSceneAsync(SceneEnum scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync($"{scene}");

        // 等待場景加載
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log($"進入場景:{scene} !");
    }

    /// <summary>
    /// 更換場景_同步
    /// </summary>
    /// <param name="scene"></param>
    public void ChangeScene_Network(SceneEnum scene)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"更換場景_同步: {scene}");
            NetworkManager.Singleton.SceneManager.LoadScene($"{scene}", LoadSceneMode.Single);
        }
    }
}
