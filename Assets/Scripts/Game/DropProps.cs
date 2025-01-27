using UnityEngine;
using Unity.Netcode;

public class DropProps : BaseNetworkObject
{
    [SerializeField] MeshRenderer _spriteRenderer;

    // 掉落道具類型
    private NetworkVariable<DropPropsEnum> _dropPropsType = 
        new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // 是否已被吃掉
    private bool _isGet;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _dropPropsType.OnValueChanged += (DropPropsEnum previousValuem, DropPropsEnum newValue) =>
        {
            _spriteRenderer.material = SOManager.I.DropProps_SO.MaterialList[(int)newValue];
        };

        _spriteRenderer.material = SOManager.I.DropProps_SO.MaterialList[(int)_dropPropsType.Value];
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (_isGet) return;

        // 接觸角色
        if (collision.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
        {
            _isGet = true;
            NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();
            GameRpcManager.I.GetDropPropsServerRpc(networkObject.NetworkObjectId, _dropPropsType.Value);
            GameRpcManager.I.DespawnObjectServerRpc(thisObjectId);
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
