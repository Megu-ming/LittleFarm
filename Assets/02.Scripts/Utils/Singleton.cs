using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;

    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindAnyObjectByType<T>();

                if(_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    singleton.name = $"{typeof(T)} [Singleton]";

                    DontDestroyOnLoad(singleton);
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
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // 이미 인스턴스가 존재하는데 다른 객체가 생성되었다면 파괴
            Destroy(gameObject);
        }
    }
}
