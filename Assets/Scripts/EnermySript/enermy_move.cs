using UnityEngine;

// 敌人移动状态机：闲置/追逐玩家，物理移动放在FixedUpdate
public class enermy_move : MonoBehaviour
{
    // 敌人状态枚举
    public enum EnemyState
    {
        Idle,    // 闲置
        Chase    // 追逐玩家
    }

    // 到达该距离停止追逐，留给攻击逻辑处理
    public float attackRange = 2f;
    // 敌人移动速度
    public float speed = 2f;

    private Rigidbody2D rb;
    private Animator anim;
    // 玩家Transform引用
    private Transform player;
    // 当前敌人状态
    private EnemyState enemyState;
    // 朝向 1向右 -1向左
    private int facingDirection = 1;
    // 是否处于被击退状态，true时禁止自主移动
    public bool isKnockBack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ChangeState(EnemyState.Idle);
    }

    void Update()
    {
        //Update只做输入/状态判断，不要修改刚体velocity
        if (isKnockBack)
        {
            return;
        }
    }

    // 物理移动全部放在FixedUpdate固定物理步
    private void FixedUpdate()
    {
        if (isKnockBack) return;
        if (enemyState == EnemyState.Chase)
        {
            Chase();
        }
    }

    // 追逐玩家逻辑
    void Chase()
    {
        if (player == null)
        {
            //Debug.Log("Chase退出：player == null");
            return;
        }
        Vector2 monsterPos = transform.position;
        Vector2 playerPos = player.position;
        float dis = Vector2.Distance(monsterPos, playerPos);
        //Debug.Log($"Chase:dis={dis:F2} attackRange={attackRange}");

        // 进入攻击范围，停止移动
        if (dis <= attackRange)
        {
            //Debug.Log("Chase：进入攻击范围，置零velocity");
            rb.velocity = Vector2.zero;
            return;
        }
        // 缓冲区间，不继续靠近
        if (dis <= attackRange + 0.4f)
        {
            //Debug.Log("Chase：处在缓冲区间，return");
            return;
        }

        // 根据玩家位置翻转朝向
        if (playerPos.x > monsterPos.x && facingDirection == -1)
        {
            Flip();
        }
        else if (playerPos.x < monsterPos.x && facingDirection == 1)
        {
            Flip();
        }

        Vector2 dir = (playerPos - monsterPos).normalized;
        Vector2 targetVelocity = dir * speed;
        float smoothRate = 8f;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, smoothRate * Time.fixedDeltaTime);
        //Debug.Log($"Chase成功执行！targetVel:{targetVelocity.magnitude:F2}");
    }

    // 翻转角色左右朝向
    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(
            transform.localScale.x * -1,
            transform.localScale.y,
            transform.localScale.z);
    }

    // 触发器检测玩家进入
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.transform;
            ChangeState(EnemyState.Chase);
        }
    }

    // 触发器检测玩家离开
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = null;
            rb.velocity = Vector2.zero;
            ChangeState(EnemyState.Idle);
        }
    }

    // 切换敌人状态，更新动画参数
    public void ChangeState(EnemyState newState)
    {
        enemyState = newState;
        if (enemyState == EnemyState.Idle)
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isChasing", false);
            rb.velocity = Vector2.zero;
        }
        else if (enemyState == EnemyState.Chase)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isChasing", true);
        }
    }

    // 是否拥有玩家目标
    public bool IsHavePlayerTarget()
    {
        return player != null;
    }

    // 获取玩家坐标，无玩家返回自身坐标
    public Vector2 GetPlayerPosition()
    {
        if (player == null)
            return transform.position;
        return player.position;
    }

    // 获取与玩家距离，无玩家返回999大值
    public float GetDistanceToPlayer()
    {
        if (player == null)
            return 999;
        return Vector2.Distance(transform.position, player.position);
    }
}
