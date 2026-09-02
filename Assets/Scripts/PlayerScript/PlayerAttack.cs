using UnityEngine;

// 玩家攻击逻辑，按键触发攻击动画，动画事件执行伤害判定
public class PlayerAttack : MonoBehaviour
{
    public Animator anim;
    public PlayerMove playerMove;

    //攻击冷却
    private float timer;
    public float cooldown = 1f;

    //攻击检测点
    public Transform attackPoint;
    //武器攻击范围
    public float weaponRange = 1f;
    //敌人Layer
    public LayerMask enemyLayers;
    //攻击伤害
    public int damaged = 1;

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    //按下攻击键调用
    public void Attack()
    {
        if (timer <= 0)
        {
            anim.SetBool("isAttacking", true);
            timer = cooldown;
        }
    }

    //由攻击动画Animation Event调用
    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            weaponRange,
            enemyLayers
        );
        foreach (Collider2D enemy in enemies)
        {
            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            //判断敌人在玩家前方还是后方
            float attackDirection = dir.x * playerMove.facingDirection;
            //大于0代表敌人在面朝方向
            if (attackDirection > 0)
            {
                enermy_healthy health =
                enemy.GetComponent<enermy_healthy>();
                if (health != null)
                {
                    health.ChangeHealth(-damaged, dir);
                }
            }
        }
    }

    //动画结束调用
    public void FinishAttacking()
    {
        anim.SetBool("isAttacking", false);
    }

    //显示攻击范围
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(
            attackPoint.position,
            weaponRange
        );
    }
}
