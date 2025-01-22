using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "View_SO", menuName = "Scriptable Objects/View_SO")]
public class View_SO : ScriptableObject
{
    public List<GameObject> ViewList;
    public List<GameObject> PermanentViewList;
}
