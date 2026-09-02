using UnityEngine;

// 将挂载物体跨场景不销毁
public class PersistThis : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
