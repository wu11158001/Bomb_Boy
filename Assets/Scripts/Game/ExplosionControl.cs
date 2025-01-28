using UnityEngine;
using Unity.Netcode;

public class ExplosionControl : BaseNetworkObject
{
    // 爆炸是否為中心點
    public bool IsCenterExplosion { get; set; }
    // 剩餘爆炸次數
    public int LastCount { get; set; }
    // 爆炸方向(0=原地, 1=上, 2=下, 3=左, 4=右)
    public int ExplosionDirection { get; set; }

    // 消失時間
    private float _despawnTime;
    // 下個爆炸位置列表
    private Vector3[] _nextCenters;

    private void OnDrawGizmos()
    {
        // 下個爆炸位置射線
        Gizmos.color = Color.red;
        Vector3 center = new(transform.position.x + GameDataManager.NextGroundDistance, transform.position.y, transform.position.z);
        Gizmos.DrawWireCube(center, GameDataManager.I.PhysicsSize);
        center = new(transform.position.x - GameDataManager.NextGroundDistance, transform.position.y, transform.position.z);
        Gizmos.DrawWireCube(center, GameDataManager.I.PhysicsSize);
        center = new(transform.position.x, transform.position.y, transform.position.z - GameDataManager.NextGroundDistance);
        Gizmos.DrawWireCube(center, GameDataManager.I.PhysicsSize);
        center = new(transform.position.x, transform.position.y, transform.position.z + GameDataManager.NextGroundDistance);
        Gizmos.DrawWireCube(center, GameDataManager.I.PhysicsSize);

        // 當下位置射線
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, GameDataManager.I.PhysicsSize);
    }

    private void Update()
    {
        if (!IsServer) return;

        // 生存時間倒數
        _despawnTime -= Time.deltaTime;
        if (_despawnTime <= 0)
        {
            GameRpcManager.I.DespawnObjectServerRpc(thisObjectId);
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
            new(transform.position.x + GameDataManager.NextGroundDistance, transform.position.y, transform.position.z),
            new(transform.position.x - GameDataManager.NextGroundDistance, transform.position.y, transform.position.z),
            new(transform.position.x, transform.position.y, transform.position.z - GameDataManager.NextGroundDistance),
            new(transform.position.x, transform.position.y, transform.position.z + GameDataManager.NextGroundDistance),
        };

        ExplosionTrigger();

        if (IsCenterExplosion) CenterExplosion();
        else DirectionExplosion();
    }

    /// <summary>
    /// 爆炸觸發行為
    /// </summary>
    private void ExplosionTrigger()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, GameDataManager.I.PhysicsSize);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Bomb}"))
            {
                /*接觸炸彈*/

                if (collider.gameObject.TryGetComponent<BombControl>(out BombControl bombControl))
                {
                    bombControl.ImmediateExplosion();
                }
            }

            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.BreakObstacle}"))
            {
                /*接觸可擊破物*/

                GameSceneManager.I.DespawnBreakObstacle(collider.gameObject);
                LastCount = 0;
            }

            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
            {
                /*接觸角色*/

                ulong networkObjectId = collider.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
                GameRpcManager.I.CharacterDieServerRpc(networkObjectId);
                Debug.Log($"角色物件:{networkObjectId}: 被炸到");
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
        Collider[] colliders = Physics.OverlapBox(_nextCenters[dir], GameDataManager.I.PhysicsSize);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Obstacle}"))
            {
                /*下個爆炸位置是障礙物*/

                isExplosion = false;
                break;
            }
        }

        if (isExplosion)
        {
            /*生成爆炸效果*/

            GameRpcManager.I.SpawnExplosionServerRpc(
                LastCount,
                _nextCenters[dir],
                dir,
                false);
        }
    }
}
