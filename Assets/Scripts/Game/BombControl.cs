using UnityEngine;

public class BombControl : MonoBehaviour
{
    // 射線Size
    private Vector3 _boxSize = new(1.4f, 1.5f, 1.4f);

    // 判斷人物是否離開碰撞範圍
    private bool _isCharacterLeave;
    // 爆炸倒數時間
    private  float _explodeCd;

    /// <summary>
    /// 爆炸等級
    /// </summary>
    public int ExplotionLevel { get; set; }

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

    private void Update()
    {       
        // 等待人物離開更換layer
        if (!_isCharacterLeave)
        {
            if (!IsPlayerInRange())
            {
                _isCharacterLeave = true;
                gameObject.layer = LayerMask.NameToLayer($"{LayerNameEnum.Bomb}");
            }
        }

        // 爆炸倒數
        _explodeCd -= Time.deltaTime;
        if (_explodeCd <= 0)
        {
            // 生成爆炸效果
            GameObject explosionObj = SOManager.I.NetworkObject_SO.NetworkObjectList[1];
            ExplosionControl explosionControl = Instantiate(explosionObj, transform.position, Quaternion.identity).GetComponent<ExplosionControl>();
            explosionControl.LastCount = ExplotionLevel;
            explosionControl.IsCenterExplosion = true;
            explosionControl.InitializeExplosion();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 檢查人物是否在方形範圍內
    /// </summary>
    /// <returns></returns>
    private bool IsPlayerInRange()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, _boxSize / 2, Quaternion.identity, LayerMask.GetMask($"{LayerNameEnum.Character}"));
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}")) 
            {
                return true;  // 角色在範圍內
            }
        }

        return false;  // 角色不在範圍內
    }

    /// <summary>
    /// 立即爆炸
    /// </summary>
    public void ImmediateExplosion()
    {
        _explodeCd = 0;
    }
}
