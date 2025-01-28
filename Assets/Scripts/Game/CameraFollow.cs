using UnityEngine;
using System.Collections;
using System;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    [SerializeField] Vector3 _offset;
    [SerializeField] float _cameraHeight;
    [SerializeField] float _cameraAngleX;

    private void LateUpdate()
    {
        if (Target == null) return;

        Vector3 desiredPosition = new Vector3(Target.position.x, _cameraHeight, Target.position.z) + _offset;
        transform.position = desiredPosition;
        transform.rotation = Quaternion.Euler(_cameraAngleX, 0f, 0f);
    }

    /// <summary>
    /// 本地玩家死亡
    /// </summary>
    public void OnLoccalDie()
    {
        Target = null;
        StartCoroutine(IMoveToDieViewing());
    }

    /// <summary>
    /// 移動到死亡視角
    /// </summary>
    /// <returns></returns>
    private IEnumerator IMoveToDieViewing()
    {
        Vector3 targetPosition = new Vector3(-10, 24, 11.5f);
        Quaternion targetRotation = Quaternion.Euler(90, 0, 0);
        float during = 1.0f;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < during)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / during;

            Vector3 pos = Vector3.Lerp(startPosition, targetPosition, progress);
            Quaternion rot = Quaternion.Lerp(startRotation, targetRotation, progress);

            transform.position = pos;
            transform.rotation = rot;

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        // 顯示死亡介面
        GameObject gameViewObj = GameObject.Find("GameView");
        if (gameViewObj != null)
        {
            GameView gameView = gameViewObj.GetComponent<GameView>();
            gameView.ShowDieView();
        }
    }
}
