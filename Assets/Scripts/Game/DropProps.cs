using UnityEngine;
using Unity.Netcode;

public class DropProps : BaseNetworkObject
{
    [SerializeField] MeshRenderer _spriteRenderer;

    private DropPropsEnum _dropPropsType;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // 接觸角色
        if (collision.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
        {
            NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();
            Debug.Log($"{networkObject.NetworkObjectId} 吃到道具: {_dropPropsType}");
            GameRpcManager.I.DespawnObjectServerRpc(thisObjectId);
        }
    }

    /// <summary>
    /// 設置掉落道具類型
    /// </summary>
    /// <param name="dropProps"></param>
    public void SetDropPropsType(DropPropsEnum dropProps)
    {
        _dropPropsType = dropProps;
        _spriteRenderer.material = SOManager.I.DropProps_SO.MaterialList[(int)dropProps];
    }
}
