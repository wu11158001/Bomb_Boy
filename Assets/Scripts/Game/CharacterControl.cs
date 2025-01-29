using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CharacterControl : BaseNetworkObject
{
    [SerializeField] GameObject TagObj;
    [SerializeField] ParticleSystem DieEffect;
    [SerializeField] GameObject Body;

    private Rigidbody _rigidbody;
    private Vector3 _movement;
    private Animator _animator;

    // 炸彈數量
    private int _bombCount;
    // 爆炸等級
    private int _explotionLevel;
    // 移動速度
    private float _moveSpeed;
    // 死亡狀態
    private bool _isDie;

    // 轉向速度
    private float _turnSpeed = 15f;        
    // 動畫Hash_是否移動
    private readonly int _isMove_Hash = Animator.StringToHash("IsMove");

    // 最接近的地板物件
    private GameObject _nearestGrounds;
    // 攝影機跟隨腳本
    CameraFollow cameraFollow;

    // 是否已首次更新資料
    private bool _isFirstUpdateData;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        StopAllCoroutines();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        TagObj.SetActive(IsOwner);

        if (IsOwner)
        {
            // 設置攝影機跟隨
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
            cameraFollow.Target = transform;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (_isDie) return;

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
        if (_isDie) return;

        if (_movement != Vector3.zero)
        {
            /*角色移動*/

            // 轉向角色朝向
            Quaternion targetRotation = Quaternion.LookRotation(_movement);
            _rigidbody.rotation = Quaternion.Slerp(
                _rigidbody.rotation,
                targetRotation,
                _turnSpeed * Time.fixedDeltaTime
            );

            _rigidbody.linearVelocity = _movement.normalized * _moveSpeed;
        }
        else
        {
            /*停止移動*/

            _rigidbody.linearVelocity = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!IsOwner) return;
        if (_isDie) return;

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
        // 首次更新資料
        if (_isFirstUpdateData == false)
        {
            GamePlayerData data = GameRpcManager.I.GetGamePlayerData(thisObjectId);
            SetNicknameText($"{data.Nickname}");
            _isFirstUpdateData = true;
        }


        if (!IsOwner) return;
        if (_isDie) return;

        GamePlayerData gamePlayerData = GameRpcManager.I.GetGamePlayerData(thisObjectId);
        _bombCount = gamePlayerData.BombCount;
        _explotionLevel = gamePlayerData.ExplotionLevel;
        _moveSpeed = gamePlayerData.MoveSpeed;
        _isDie = gamePlayerData.IsDie;
    }

    /// <summary>
    /// 設置暱稱文字
    /// </summary>
    /// <param name="nickanem"></param>
    public void SetNicknameText(string nickanem)
    {
        string color =
            IsOwner ?
            "F6BF23" :
            "D53C2B";
        string nicknameStr = $"<color=#{color}>{nickanem}</color>";

        GameObject characterNicknameObj = SOManager.I.NormalObject_SO.GameObjectList[0];
        CharacterNickname characterNickname = Instantiate(characterNicknameObj).GetComponent<CharacterNickname>();
        characterNickname.SetFollowCharacter(transform, nicknameStr);
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public void OnDie()
    {
        _isDie = true;
        StartCoroutine(IDieBehavior());
    }

    /// <summary>
    /// 死亡行為
    /// </summary>
    /// <returns></returns>
    private IEnumerator IDieBehavior()
    {
        Body.SetActive(false);
        _rigidbody.isKinematic = true;

        yield return new WaitForSeconds(0.5f);

        DieEffect.Play();

        yield return new WaitForSeconds(2.5f);

        if (IsOwner)
        {
            GameRpcManager.I.DespawnObjectServerRpc(thisObjectId);
            cameraFollow.OnLoccalDie();
        }
    }
}
