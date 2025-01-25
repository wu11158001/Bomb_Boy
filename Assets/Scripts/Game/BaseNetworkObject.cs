using UnityEngine;
using Unity.Netcode;

public class BaseNetworkObject : NetworkBehaviour
{
    protected NetworkObject thisNetworkObject;
    protected ulong thisObjectId;

    public override void OnNetworkSpawn()
    {
        thisNetworkObject = GetComponent<NetworkObject>();
        thisObjectId = thisNetworkObject.NetworkObjectId;
    }
}
