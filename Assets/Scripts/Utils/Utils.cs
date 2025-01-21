using System.Collections.Generic;
using UnityEngine;

public class Utils : UnitySingleton<Utils>
{
    /// <summary>
    /// List洗牌
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public List<T> Shuffle<T>(List<T> list) where T : Component
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("List洗牌 傳入值是空");
            return new List<T>();
        }

        List<T> result = list != null ? new List<T>(list) : new();
        System.Random rng = new();
        int n = result.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = result[k];
            result[k] = result[n];
            result[n] = value;
        }

        return result;
    }
}
