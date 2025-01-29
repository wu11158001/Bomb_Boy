using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameObject_SO", menuName = "Scriptable Objects/GameObject_SO")]
public class GameObject_SO : ScriptableObject
{
    public List<GameObject> GameObjectList;
}
