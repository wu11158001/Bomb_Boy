using UnityEngine;
using Unity.Netcode;

public class DropProps : BaseNetworkObject
{
    [SerializeField] MeshRenderer _spriteRenderer;

    private BoxCollider _boxCollider;

    // 掉落道具類型
    private NetworkVariable<DropPropsEnum> _dropPropsType = 
        new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 是否已被吃掉
    private bool _isGet;

    private void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _dropPropsType.OnValueChanged += (DropPropsEnum previousValuem, DropPropsEnum newValue) =>
        {
            _spriteRenderer.material = SOManager.I.DropProps_SO.MaterialList[(int)newValue];
        };

        _spriteRenderer.material = SOManager.I.DropProps_SO.MaterialList[(int)_dropPropsType.Value];
    }

    private void OnTriggerEnter(Collider other)
    {
        // 接觸角色
        if (other.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
        {
            NetworkObject networkObject = other.gameObject.GetComponent<NetworkObject>();

            // 發送獲得道具
            if (IsServer && !_isGet)
            {
                GameRpcManager.I.GetDropPropsServerRpc(networkObject.NetworkObjectId, _dropPropsType.Value);
                GameRpcManager.I.DespawnObjectServerRpc(thisObjectId);
            }

            // 本地玩家音效
            if (networkObject.IsOwner)
            {
                AudioManager.I.PlaySound(SoundEnum.GetDropProps, false, 0.7f);
            }

            _isGet = true;
        }
    }

    /// <summary>
    /// 設置掉落道具類型
    /// </summary>
    /// <param name="dropProps"></param>
    public void SetDropPropsType(DropPropsEnum dropProps)
    {
        Debug.Log($"設置掉落道具類型: {dropProps}");
        _isGet = false;
        _dropPropsType.Value = dropProps;
    }
}
