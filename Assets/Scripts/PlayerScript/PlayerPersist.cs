using UnityEngine;

// 玩家对象持久化脚本，保证场景切换不会重复生成玩家
public class PlayerPersist : MonoBehaviour
{
    private static bool created;

    private void Awake()
    {
        if (created)
        {
            Destroy(gameObject);
            return;
        }
        created = true;
        DontDestroyOnLoad(gameObject);
    }
}
