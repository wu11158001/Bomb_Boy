using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    [SerializeField] Vector3 _offset = new Vector3(0, 0, -5.0f);
    [SerializeField] float _cameraHeight = 12.0f;
    [SerializeField] float _cameraAngleX = 65.0f;

    private void LateUpdate()
    {
        if (Target == null) return;

        Vector3 desiredPosition = new Vector3(Target.position.x, _cameraHeight, Target.position.z) + _offset;
        transform.position = desiredPosition;
        transform.rotation = Quaternion.Euler(_cameraAngleX, 0f, 0f);
    }
}
