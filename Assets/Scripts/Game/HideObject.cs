using UnityEngine;
using Unity.Netcode;

public class HideObject : BaseNetworkObject
{
    private Animator _animator;

    // 動畫Hash_搖動
    private readonly int _isShake_Hash = Animator.StringToHash("Shake");

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // 角色進入躲藏物件
        if (other.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
        {
            NetworkObject networkObject = other.gameObject.GetComponent<NetworkObject>();
            GameRpcManager.I.CharacverHideServerRpc(networkObject.NetworkObjectId, true);

            _animator.SetTrigger(_isShake_Hash);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        // 角色離開躲藏物件
        if (other.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
        {
            NetworkObject networkObject = other.gameObject.GetComponent<NetworkObject>();
            GameRpcManager.I.CharacverHideServerRpc(networkObject.NetworkObjectId, false);

            _animator.SetTrigger(_isShake_Hash);
        }
    }
}
