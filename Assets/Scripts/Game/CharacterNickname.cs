using UnityEngine;
using TMPro;

public class CharacterNickname : MonoBehaviour
{
    private TextMeshPro _thisTextMeshPro;
    private Transform _target;
    private readonly Vector3 _offset = new(0, 2.5f, -0.8f);

    private void Awake()
    {
        _thisTextMeshPro = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        if (_target != null)
        {
            transform.position = _target.position + _offset;

            if (_target == null || !_target.gameObject.activeSelf)
            {
                Destroy(gameObject);
            }
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
    public void SetFollowCharacter(Transform character, string nickname)
    {
        _target = character;
        _thisTextMeshPro.text = nickname;
    }
}
