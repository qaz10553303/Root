using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    Debug.LogError($"Cannot find singleton of type:{typeof(T)}");
                    //GameObject singletonObject = new GameObject();
                    //_instance = singletonObject.AddComponent<T>();
                    //singletonObject.name = typeof(T).ToString() + " (Singleton)";
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
