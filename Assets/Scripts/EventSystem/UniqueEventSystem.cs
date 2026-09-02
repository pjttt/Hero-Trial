using UnityEngine;
using UnityEngine.EventSystems;

// 全局唯一EventSystem管理器，DontDestroyOnLoad
// 解决多场景切换出现多个EventSystem导致UI点击失效问题
[RequireComponent(typeof(EventSystem))]
public class UniqueEventSystem : MonoBehaviour
{
    // 静态单例实例
    private static UniqueEventSystem _inst;

    // 本机挂载的EventSystem组件
    private EventSystem _selfEs;

    void Awake()
    {
        //单例优先：已经存在实例，直接自杀，不要执行任何删除逻辑！
        if (_inst != null)
        {
            Destroy(gameObject);
            return;
        }
        //当前没有实例，自己成为全局唯一
        _inst = this;
        _selfEs = GetComponent<EventSystem>();
        DontDestroyOnLoad(gameObject);

        //查找并清理场景中其他残留EventSystem（防止自动生成出来的副本）
        EventSystem[] allEs = Object.FindObjectsOfType<EventSystem>();
        foreach (var es in allEs)
        {
            if (es != null && es != _selfEs)
            {
                //Debug.Log($"删除多余EventSystem:{es.gameObject.name}");
                Destroy(es.gameObject);
            }
        }
    }

    // 注册场景加载完成回调
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 移除场景加载回调，防止内存泄漏
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 场景加载完成后查杀一遍自动生成的ES
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        //每加载完一个新场景，把自动生成出来的多余EventSystem全部删掉
        EventSystem[] allEs = Object.FindObjectsOfType<EventSystem>();
        foreach (var es in allEs)
        {
            if (es != null && es != _selfEs)
            {
                //Debug.Log($"场景加载，清除自动生成多余EventSystem：{es.gameObject.name}");
                Destroy(es.gameObject);
            }
        }
    }

    // 获取全局唯一实例，如果不存在，动态生成完整EventSystem对象
    public static UniqueEventSystem GetOrCreate()
    {
        if (_inst == null)
        {
            GameObject go = new GameObject("GlobalEventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            _inst = go.AddComponent<UniqueEventSystem>();
        }
        return _inst;
    }
}
