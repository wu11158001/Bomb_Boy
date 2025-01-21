using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _movement;
    private Animator _animator;

    // 移動速度
    private float _moveSpeed = 3;
    // 轉向速度
    private const float _turnSpeed = 10f;        
    // 動畫Hash_是否移動
    private readonly int _isMove_Hash = Animator.StringToHash("IsMove");

    // 最接近的地板物件
    private GameObject _nearestGrounds;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
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

        GameObject bombObj = SOManager.I.NetworkObject_SO.NetworkObjectList[0];

        Vector3 offset = new Vector3(0.4f, 0, -0.4f);
        Vector3 spawnPosition = _nearestGrounds.transform.position + offset + Vector3.up * _nearestGrounds.transform.lossyScale.y / 2;
        GameObject bomb = Instantiate(bombObj, spawnPosition, Quaternion.identity);
        bomb.layer = LayerMask.NameToLayer("NotCollision");
    }
}
