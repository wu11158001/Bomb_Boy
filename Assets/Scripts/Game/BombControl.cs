using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

public class BombControl : BaseNetworkObject
{
    private Collider _bombCollider;
    // 自身碰撞射線範圍
    private Vector3 _boxSize = new(1.6f, 0, 1.6f);
    // 回復碰撞距離
    private const float _collisionRestoreDistance = 1.25f;
    // 紀錄初始忽略碰撞角色
    private List<GameObject> _ignorCharacterList = new();

    // 爆炸倒數時間
    private float _explodeCd;
    // 是否已爆炸
    private bool _isExplode;

    /// <summary>
    /// 爆炸等級
    /// </summary>
    public int ExplotionLevel { get; set; }

    // 產生角色Id
    public NetworkVariable<ulong> CharacterObjectId_NV
        = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void OnDrawGizmos()
    {
        // 當下位置射線
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, _boxSize);
    }

    private void Awake()
    {
        _bombCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        // 初始忽略當前位置的角色碰撞
        Collider[] colliders = Physics.OverlapBox(transform.position, _boxSize);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
            {
                /*接觸角色*/
                Collider playerCollider = collider.gameObject.GetComponent<CapsuleCollider>();
                Physics.IgnoreCollision(_bombCollider, playerCollider, true);
                _ignorCharacterList.Add(collider.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        _isExplode = false;
        _explodeCd = 3;
    }

    private void Update()
    {
        /*碰撞角色判斷*/
        for (int i = _ignorCharacterList.Count - 1; i >= 0; i--)
        {
            float distance = Vector3.Distance(_ignorCharacterList[i].transform.position, transform.position);
            if (distance > _collisionRestoreDistance)
            {
                Collider playerCollider = _ignorCharacterList[i].GetComponent<CapsuleCollider>();
                Physics.IgnoreCollision(_bombCollider, playerCollider, false);

                _ignorCharacterList.Remove(_ignorCharacterList[i]);
            }
        }

        if (IsServer)
        {
            /*爆炸倒數*/
            _explodeCd -= Time.deltaTime;
            if (!_isExplode && _explodeCd <= 0)
            {
                _isExplode = true;

                // 生成爆炸效果
                GameRpcManager.I.SpawnExplosionServerRpc(
                    ExplotionLevel,
                    transform.position,
                    0,
                    true);

                // 更新遊戲玩家資料
                GamePlayerData gamePlayerData = GameRpcManager.I.GetGamePlayerData(CharacterObjectId_NV.Value);
                gamePlayerData.BombCount += 1;
                GameRpcManager.I.UpdateLobbyPlayerServerRpc(gamePlayerData);

                // 消除物件
                GameRpcManager.I.DespawnObjectServerRpc(thisObjectId);
            }
        }
    }

    /// <summary>
    /// 立即爆炸
    /// </summary>
    public void ImmediateExplosion()
    {
        _explodeCd = 0;
    }
}
