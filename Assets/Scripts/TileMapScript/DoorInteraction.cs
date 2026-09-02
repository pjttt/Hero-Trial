using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// 门交互组件，鼠标右键点击门，执行场景切换与玩家传送
public class DoorInteraction : MonoBehaviour
{
    [Header("场景设置")]
    public string targetSceneName;
    public string targetExitPointName;

    [Header("检测设置")]
    public LayerMask doorLayerMask;

    [Header("交互范围")]
    public CircleCollider2D interactionTrigger;

    [Header("交互提示")]
    public string interactionPrompt = "进入房子";

    private bool isHovering = false;
    private bool isTeleporting = false;

    private void Update()
    {
        CheckMouseHover();
        if (Input.GetMouseButtonDown(1))
        {
            TryInteract();
        }
    }

    private void CheckMouseHover()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, doorLayerMask);
        bool hovering = (hit.collider != null && hit.collider.gameObject == gameObject);
        if (hovering != isHovering)
        {
            isHovering = hovering;
            //Debug.Log(isHovering ? $"🟢 悬停: {gameObject.name}" : "🔴 取消悬停");
        }
    }

    private void TryInteract()
    {
        if (isTeleporting)
        {
            //Debug.Log("正在传送中，请稍后...");
            return;
        }
        if (!IsPlayerInRange())
        {
            //Debug.Log("玩家不在门附近，无法交互");
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, doorLayerMask);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            StartCoroutine(Teleport());
        }
    }

    private bool IsPlayerInRange()
    {
        if (GameManager.Instance?.player == null)
        {
            //Debug.LogWarning("没有找到玩家");
            return false;
        }
        if (GameManager.Instance == null)
        {
            //Debug.LogWarning("GameManager 不存在");
            return false;
        }

        // 使用 CircleCollider2D 检测
        if (interactionTrigger != null)
        {
            Vector2 center = interactionTrigger.bounds.center;
            float radius = interactionTrigger.radius * transform.lossyScale.x;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
            foreach (Collider2D col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    //Debug.Log($"✅ 玩家在交互范围内");
                    return true;
                }
            }
            //Debug.Log($"❌ 玩家不在交互范围内");
            return false;
        }
        // 如果没有设置 CircleCollider，回退到距离检测
        float distance = Vector2.Distance(
            transform.position,
            GameManager.Instance.player.transform.position
        );
        return distance <= 2f;
    }

    private IEnumerator Teleport()
    {
        isTeleporting = true;
        //Debug.Log($"🚪 开始传送: {gameObject.name} → {targetSceneName}");
        GameManager.Instance.pendingExitName = targetExitPointName;
        yield return StartCoroutine(GameManager.Instance.StartFadeOut());
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;
        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
        //Debug.Log($"✅ 传送完成: {targetSceneName}");
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionTrigger != null)
        {
            Vector2 center = interactionTrigger.bounds.center;
            float radius = interactionTrigger.radius * transform.lossyScale.x;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, radius);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}
