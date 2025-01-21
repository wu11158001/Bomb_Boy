using UnityEngine;

public class UnitySingleton<T> : MonoBehaviour where T: Component
{
    private static T _instance = null;
    public static T I
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<T>();
                    obj.hideFlags = HideFlags.DontSave;
                    obj.name = typeof(T).Name;
                }
            }

            return _instance;
        }
    }

    public virtual void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (_instance == null) _instance = this as T;
        else Destroy(this.gameObject);
    }
}
