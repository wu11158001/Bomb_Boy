using UnityEngine;

public class ExplosionControl : MonoBehaviour
{
    // 射線Size
    private Vector3 _physicsSize = new(0.5f, 1.5f, 0.5f);
    // 下個爆炸位置距離
    private const float _nextDistance = 1.6f;

    // 爆炸是否為中心點
    public bool IsCenterExplosion { get; set; }
    // 剩餘爆炸次數
    public int LastCount { get; set; }
    // 爆炸方向(0=上, 1=下, 2=左, 3=右)
    public int ExplosionDirection { get; set; }

    // 消失時間
    private float _despawnTime;
    // 下個爆炸位置列表
    private Vector3[] _nextCenters;

    private void OnDrawGizmos()
    {
        // 下個爆炸位置射線
        Gizmos.color = Color.red;
        Vector3 center = new(transform.position.x + _nextDistance, transform.position.y, transform.position.z);
        Gizmos.DrawWireCube(center, _physicsSize);
        center = new(transform.position.x - _nextDistance, transform.position.y, transform.position.z);
        Gizmos.DrawWireCube(center, _physicsSize);
        center = new(transform.position.x, transform.position.y, transform.position.z - _nextDistance);
        Gizmos.DrawWireCube(center, _physicsSize);
        center = new(transform.position.x, transform.position.y, transform.position.z + _nextDistance);
        Gizmos.DrawWireCube(center, _physicsSize);

        // 當下位置射線
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, _physicsSize);
    }

    private void Update()
    {
        _despawnTime -= Time.deltaTime;
        if (_despawnTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 重製爆炸
    /// </summary>
    public void InitializeExplosion()
    {
        _despawnTime = 3.0f;
        _nextCenters = new Vector3[]
        {
            new(transform.position.x + _nextDistance, transform.position.y, transform.position.z),
            new(transform.position.x - _nextDistance, transform.position.y, transform.position.z),
            new(transform.position.x, transform.position.y, transform.position.z - _nextDistance),
            new(transform.position.x, transform.position.y, transform.position.z + _nextDistance),
        };

        if (IsCenterExplosion) CenterExplosion();
        else DirectionExplosion();

        ExplosionTrigger();
    }

    /// <summary>
    /// 爆炸觸發行為
    /// </summary>
    private void ExplosionTrigger()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, _physicsSize);
        foreach (Collider collider in colliders)
        {
            /*接觸炸彈*/
            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Bomb}"))
            {
                if(collider.gameObject.TryGetComponent<BombControl>(out BombControl bombControl))
                {
                    bombControl.ImmediateExplosion();
                }
            }

            /*接觸可擊破物*/
            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.BreakObstacle}"))
            {
                GameSceneManager.I.DespawnBreakObstacle(collider.gameObject);
            }

            /*接觸角色*/
            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
            {
                Debug.Log("角色被炸到");
            }
        }
    }

    /// <summary>
    /// 中心點爆炸
    /// </summary>
    private void CenterExplosion()
    {
        if (LastCount <= 1) LastCount = 1;

        for (int i = 0; i < _nextCenters.Length; i++)
        {
            SpawnNextExplosion(i);
        }        
    }

    /// <summary>
    /// 方向位置爆炸
    /// </summary>
    private void DirectionExplosion()
    {
        LastCount--;

        if (LastCount > 0)
        {
            SpawnNextExplosion(ExplosionDirection);
        }
    }

    /// <summary>
    /// 產生下個爆炸
    /// </summary>
    /// <param name="dir">爆炸方向</param>
    private void SpawnNextExplosion(int dir)
    {
        bool isExplosion = true;
        Collider[] colliders = Physics.OverlapBox(_nextCenters[dir], _physicsSize);
        foreach (Collider collider in colliders)
        {
            // 下個爆炸位置是障礙物
            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Obstacle}"))
            {
                isExplosion = false;
                break;
            }
        }

        if (isExplosion)
        {
            // 生成爆炸效果
            GameObject explosionObj = SOManager.I.NetworkObject_SO.NetworkObjectList[1];
            ExplosionControl explosionControl = Instantiate(explosionObj, _nextCenters[dir], Quaternion.identity).GetComponent<ExplosionControl>();
            explosionControl.LastCount = LastCount;
            explosionControl.ExplosionDirection = dir;
            explosionControl.IsCenterExplosion = false;
            explosionControl.InitializeExplosion();
        }
    }
}
