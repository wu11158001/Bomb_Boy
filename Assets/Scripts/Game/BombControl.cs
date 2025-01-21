using UnityEngine;

public class BombControl : MonoBehaviour
{
    [SerializeField] Vector3 _boxSize;

    // 判斷人物是否離開碰撞範圍
    private bool _isCharacterLeave;
    // 爆炸倒數時間
    private  float _explodeCd;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, _boxSize);
    }

    private void OnEnable()
    {
        _isCharacterLeave = false;
        _explodeCd = 3.0f;
    }

    private void Start()
    {
        _boxSize = new Vector3(1.4f, 1.5f, 1.4f);
    }

    void Update()
    {       
        // 等待人物離開更換layer
        if (!_isCharacterLeave)
        {
            if (!IsPlayerInRange())
            {
                _isCharacterLeave = true;
                gameObject.layer = LayerMask.NameToLayer("Obstacle");
            }
        }

        // 爆炸倒數
        _explodeCd -= Time.deltaTime;
        if (_explodeCd <= 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 檢查人物是否在方形範圍內
    /// </summary>
    /// <returns></returns>
    bool IsPlayerInRange()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, _boxSize / 2, Quaternion.identity, LayerMask.GetMask("Character"));
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Character")) 
            {
                return true;  // 角色在範圍內
            }
        }

        return false;  // 角色不在範圍內
    }


}
