using UnityEngine;
using Unity.Netcode;

public class DropProps : NetworkBehaviour
{
    [SerializeField] MeshRenderer _spriteRenderer;

    private DropPropsEnum _dropProps;

    private void OnCollisionEnter(Collision collision)
    {
        // 接觸角色
        if (collision.gameObject.layer == LayerMask.NameToLayer($"{LayerNameEnum.Character}"))
        {
            Debug.Log($"吃到道具: {_dropProps}");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 設置掉落道具類型
    /// </summary>
    /// <param name="dropProps"></param>
    public void SetDropPropsType(DropPropsEnum dropProps)
    {
        _dropProps = dropProps;
        _spriteRenderer.material = SOManager.I.DropProps_SO.MaterialList[(int)dropProps];
    }
}
