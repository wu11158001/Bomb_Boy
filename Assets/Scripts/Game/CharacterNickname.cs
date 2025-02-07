using UnityEngine;
using TMPro;
using System.Linq;

public class CharacterNickname : MonoBehaviour
{
    [SerializeField] Vector3 _offset;
    [SerializeField] float smoothSpeed;

    private TextMeshProUGUI _thisTextMeshPro;
    private Transform _target;
    private Camera _mainCamera;

    private void Awake()
    {
        _thisTextMeshPro = GetComponent<TextMeshProUGUI>();
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_target != null && _thisTextMeshPro != null)
        {
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(_target.position + _offset);
            _thisTextMeshPro.transform.position = Vector3.Lerp(_thisTextMeshPro.transform.position, screenPos, smoothSpeed);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 設置跟隨角色
    /// </summary>
    /// <param name="character"></param>
    /// <param name="nickname"></param>
    /// <param name="nickname"></param>
    public void SetFollowCharacter(Transform character, string nickname, bool isOwner)
    {
        _target = character;

        // 暱稱顏色
        string nicknameColor =
            isOwner ?
            "F6BF23" :
            "D53C2B";

        // 暱稱文字
        string takeNickname =
            nickname.Length > 6 ?
            $"{new string(nickname.Take(6).ToArray())}..." :
            nickname;

        _thisTextMeshPro.text = $"<color=#{nicknameColor}>{takeNickname}</color>";
    }
}
