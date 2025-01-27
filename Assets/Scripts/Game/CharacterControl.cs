using UnityEngine;
using Unity.Netcode;

public class CharacterControl : BaseNetworkObject
{
    // 射線Size
    private Vector3 _physicsSize = new(0.5f, 1.5f, 0.5f);
    // 下個地板射線判斷位置
    private const float _nextDistance = 1.6f;

    private Rigidbody _rigidbody;
    private Vector3 _movement;
    private Animator _animator;

    // 炸彈數量
    private int _bombCount;
    // 爆炸等級
    private int _explotionLevel;
    // 移動速度
    private float _moveSpeed;

    // 轉向速度
    private const float _turnSpeed = 10f;        
    // 動畫Hash_是否移動
    private readonly int _isMove_Hash = Animator.StringToHash("IsMove");

    // 最接近的地板物件
    private GameObject _nearestGrounds;

    private void OnDrawGizmos()
    {
        // 下個爆炸位置射線
        Gizmos.color = Color.white;
        Vector3 center = new(transform.position.x + _nextDistance, transform.position.y, transform.position.z);
        Gizmos.DrawWireCube(center, _physicsSize);
        center = new(transform.position.x - _nextDistance, transform.position.y, transform.position.z);
        Gizmos.DrawWireCube(center, _physicsSize);
        center = new(transform.position.x, transform.position.y, transform.position.z - _nextDistance);
        Gizmos.DrawWireCube(center, _physicsSize);
        center = new(transform.position.x, transform.position.y, transform.position.z + _nextDistance);
        Gizmos.DrawWireCube(center, _physicsSize);
    }
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            // 設置攝影機跟隨
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
            cameraFollow.Target = transform;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        _movement = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow)) _movement.z = 1;
        if (Input.GetKey(KeyCode.DownArrow)) _movement.z = -1;
        if (Input.GetKey(KeyCode.LeftArrow)) _movement.x = -1;
        if (Input.GetKey(KeyCode.RightArrow)) _movement.x = 1;

        if (Input.GetKeyDown(KeyCode.Space)) SpawnBomb();

        _animator.SetBool(_isMove_Hash, _movement.x != 0 || _movement.z != 0);
    }

    private void FixedUpdate()
    {
        if (_movement != Vector3.zero)
        {
            /*角色移動*/

            _rigidbody.linearVelocity = _movement.normalized * _moveSpeed;

            // 轉向角色朝向
            Quaternion targetRotation = Quaternion.LookRotation(_movement);
            _rigidbody.rotation = Quaternion.Slerp(
                _rigidbody.rotation,
                targetRotation,
                _turnSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            /*停止移動*/

            _rigidbody.linearVelocity = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Ground}"))
        {
            GameObject collidedObject = collision.gameObject;
            float distanceToPlayer = Vector3.Distance(transform.position, collidedObject.transform.position);

            // 找最近的地板
            if (_nearestGrounds == null || distanceToPlayer < Vector3.Distance(transform.position, _nearestGrounds.transform.position))
            {
                _nearestGrounds = collidedObject;
            }
        }
    }

    /// <summary>
    /// 生成炸彈
    /// </summary>
    /// <param name="target"></param>
    private void SpawnBomb()
    {
        if (_nearestGrounds == null) return;
        if (_bombCount <= 0) return;

        Vector3 offset = GameDataManager.I.CreateSceneObjectOffset;
        Vector3 spawnPosition = _nearestGrounds.transform.position + offset + Vector3.up * _nearestGrounds.transform.lossyScale.y / 2;

        GameRpcManager.I.SpawnBombServerRpc(
            thisObjectId,
            _explotionLevel,
            spawnPosition);
    }

    /// <summary>
    /// 更新角色資料
    /// </summary>
    public void UpdateCharacterData()
    {
        GamePlayerData gamePlayerData = GameRpcManager.I.GetGamePlayerData(thisObjectId);
        _bombCount = gamePlayerData.BombCount;
        _explotionLevel = gamePlayerData.ExplotionLevel;
        _moveSpeed = gamePlayerData.MoveSpeed;
    }
}
