using UnityEngine;

// 확장 메서드는 반드시 static 클래스 안에 있어야 합니다.
public static class Extension
{
    /// <summary>
    /// 컴포넌트를 가져오고, 없으면 새로 추가해서 반환합니다.
    /// </summary>
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }

    /// <summary>
    /// Component에서도 바로 호출할 수 있도록 오버로딩 (transform.GetOrAddComponent 등 가능)
    /// </summary>
    public static T GetOrAddComponent<T>(this Component component) where T : Component
    {
        return component.gameObject.GetOrAddComponent<T>();
    }
}