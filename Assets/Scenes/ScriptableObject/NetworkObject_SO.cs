using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "NetworkObject_SO", menuName = "Scriptable Objects/NetworkObject_SO")]
public class NetworkObject_SO : ScriptableObject
{
    public List<GameObject> NetworkObjectList;
}
