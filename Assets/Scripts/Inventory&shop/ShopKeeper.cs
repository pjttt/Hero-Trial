using UnityEngine;

// 商人NPC交互：靠近NPC，按【攻击键Slash】打开商店
public class ShopKeeperInteraction : MonoBehaviour
{
    [Header("交互检测圈（CircleCollider2D，勾选Is Trigger，拖进来）")]
    public CircleCollider2D interactionTrigger;

    private bool _isPlayerInRange; //玩家是否走到NPC附近

    private void Update()
    {
        // 如果UI（商店/背包）已经打开，直接不处理交互输入
        if (GameManager.Instance == null)
            return;
        if (GameManager.Instance.isBagOpen)
        {
            return;
        }
        // 按下【攻击键 Slash】（和玩家挥刀同一个按键）
        if (Input.GetButtonDown("Slash"))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        //Debug.Log("尝试和商人交互");
        //不在范围内直接返回
        if (!_isPlayerInRange)
        {
            return;
        }
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OpenShop();
        }
        else
        {
            //Debug.LogError("ShopManager.Instance == null！检查ShopManagerObj物体是否激活");
        }
    }

    //玩家进入交互圈
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
        }
    }

    //玩家离开交互圈
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
        }
    }

    //Scene窗口绘制绿色调试圆圈
    private void OnDrawGizmosSelected()
    {
        if (interactionTrigger != null)
        {
            Gizmos.color = Color.green;
            Vector2 center = interactionTrigger.bounds.center;
            float radius = interactionTrigger.radius * transform.lossyScale.x;
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}
