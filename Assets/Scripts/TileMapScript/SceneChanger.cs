using UnityEngine;
using UnityEngine.SceneManagement;

// 旧版场景切换触发器，玩家进入触发区域直接切换场景
public class SceneChanger : MonoBehaviour
{
    public string targetSceneName;
    public string targetExitPointName;
    private static bool teleporting;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log($"🔵 OnTriggerEnter2D 触发: {collision.gameObject.name}");
        if (teleporting)
        {
            //Debug.Log("⏳ 正在传送中，跳过");
            return;
        }
        if (!collision.CompareTag("Player"))
        {
            //Debug.Log($"❌ 不是 Player Tag: {collision.tag}");
            return;
        }
        //Debug.Log($"✅ 玩家进入，准备传送到 {targetSceneName}");
        teleporting = true;
        if (GameManager.Instance == null)
        {
            //Debug.LogError("❌ GameManager 不存在！");
            return;
        }
        GameManager.Instance.pendingExitName = targetExitPointName;
        SceneManager.LoadScene(targetSceneName);
    }

    private void Start()
    {
        //Debug.Log($"🚪 SceneChanger 初始化: {gameObject.name}");
        Invoke(nameof(ResetTeleport), 1f);
    }

    void ResetTeleport()
    {
        teleporting = false;
    }
}
