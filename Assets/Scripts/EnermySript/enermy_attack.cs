using UnityEngine;

// 敌人攻击逻辑：检测玩家距离、触发攻击动画、动画事件执行伤害判定
public class enermy_attack : MonoBehaviour
{
    // 攻击造成伤害值
    public int damage = 1;

    [Header("攻击点")]
    // 水平方向攻击点
    public Transform attackPointHor;
    // 向上攻击点
    public Transform attackPointUp;
    // 向下攻击点
    public Transform attackPointDown;

    // 攻击判定圆形半径
    public float weaponrange = 0.5f;

    [Header("攻击范围")]
    // 进入该距离就可以发起攻击
    public float attackRange = 2f;
    // 目标层级，只识别玩家层
    public LayerMask playerLayer;

    [Header("攻击CD")]
    // 两次攻击冷却时间
    public float attackCd = 1f;
    // CD计时器
    private float attackTimer;

    // 敌人动画控制器
    public Animator anim;
    // 是否正在攻击动画中
    private bool isAttacking;

    // 敌人移动脚本引用
    private enermy_move enemyMove;

    void Start()
    {
        enemyMove = GetComponent<enermy_move>();
    }

    void Update()
    {
        // 攻击CD倒计时
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // 攻击中直接跳过逻辑
        if (isAttacking)
            return;
        if (enemyMove == null)
            return;
        // 没有玩家目标不攻击
        if (!enemyMove.IsHavePlayerTarget())
            return;

        float dis = enemyMove.GetDistanceToPlayer();
        // 玩家在攻击范围内，并且CD结束，发起攻击
        if (dis <= attackRange && attackTimer <= 0)
        {
            StartAttack();
        }
    }

    // 启动攻击，设置动画参数，触发攻击动画
    void StartAttack()
    {
        int dir = GetAttackDirection();
        anim.SetInteger("AttackDir", dir);
        anim.Update(0);
        anim.SetTrigger("Attack");
        isAttacking = true;
        attackTimer = attackCd;
    }

    // 根据玩家位置获取攻击方向 0水平，1向上，2向下
    int GetAttackDirection()
    {
        Vector2 playerPos = enemyMove.GetPlayerPosition();
        Vector2 dir = playerPos - (Vector2)transform.position;
        // Y轴差值更大 → 上下攻击
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            if (dir.y > 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        // 水平攻击
        return 0;
    }

    // 动画事件回调：执行伤害检测
    public void Attack()
    {
        Transform point = attackPointHor;
        int dir = anim.GetInteger("AttackDir");
        if (dir == 1)
        {
            point = attackPointUp;
        }
        else if (dir == 2)
        {
            point = attackPointDown;
        }

        if (point == null)
            return;

        // 圆形范围检测玩家
        Collider2D[] hitPlayers =
        Physics2D.OverlapCircleAll(
            point.position,
            weaponrange,
            playerLayer);

        foreach (Collider2D player in hitPlayers)
        {
            PlayerHealthy hp = player.GetComponent<PlayerHealthy>();
            if (hp != null)
            {
                Vector2 hitDir = (player.transform.position - transform.position).normalized;
                hp.ChangeHealth(-damage, hitDir);
            }
        }
    }

    // 动画事件回调：攻击动画结束，解除攻击状态
    public void AttackAnimOver()
    {
        isAttacking = false;
    }

    // Scene视图绘制调试Gizmos，打包不生效
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
        if (attackPointHor != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                attackPointHor.position,
                weaponrange);
        }
        if (attackPointUp != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(
                attackPointUp.position,
                weaponrange);
        }
        if (attackPointDown != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(
                attackPointDown.position,
                weaponrange);
        }
    }
}
