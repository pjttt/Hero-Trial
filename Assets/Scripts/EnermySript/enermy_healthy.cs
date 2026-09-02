using System.Collections;
using UnityEngine;

// 敌人生命值组件：扣血、受伤闪烁、击退、死亡给经验、掉落战利品
public class enermy_healthy : MonoBehaviour
{
    // 当前生命值
    public int currentHealth;
    // 最大生命值
    public int maxHealth = 10;
    // 被击退的力度
    public float knockBackForce;
    // 敌人刚体2D
    public Rigidbody2D rb;
    // 敌人精灵渲染组件
    public SpriteRenderer spriteRenderer;
    // 受伤闪烁颜色
    public Color hurtColor = Color.red;
    // 受伤颜色持续时间
    public float hurtTime = 0.1f;
    // 音效播放组件
    public AudioSource audioSource;
    // 受伤音效
    public AudioClip hurtSound;

    // 保存原始精灵颜色
    private Color originalColor;
    // 击杀给予玩家经验值
    public int expReward = 30;

    // 战利品掉落配置表SO
    public LootTableSO enemyLootTable;
    // 地上掉落物预制体
    public GameObject lootItemPrefab;

    private void Start()
    {
        currentHealth = maxHealth;
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // 修改生命值，处理受伤、击退、死亡逻辑
    // amount：血量变化值，负数扣血
    // hitDirection：攻击来源方向，用于击退
    public void ChangeHealth(int amount, Vector2 hitDirection)
    {
        currentHealth += amount;
        // 受到伤害
        if (amount < 0)
        {
            HurtEffect();
            KnockBack(hitDirection);
        }
        // 血量小于等于0执行死亡
        if (currentHealth <= 0)
        {
            GiveExp();
            SpawnLoot();
            Destroy(gameObject);
        }
    }

    // 受伤效果：闪烁颜色 + 播放受伤音效
    void HurtEffect()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(HurtFlash());
        }
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
    }

    // 执行敌人击退
    void KnockBack(Vector2 direction)
    {
        if (rb == null)
            return;
        enermy_move move = GetComponent<enermy_move>();
        if (move != null)
        {
            move.isKnockBack = true;
        }
        rb.velocity = Vector2.zero;
        rb.AddForce(
            direction * knockBackForce,
            ForceMode2D.Impulse
        );
        StartCoroutine(StopKnockBack(move));
    }

    // 给玩家增加击杀经验
    void GiveExp()
    {
        PlayerExp playerExp = FindObjectOfType<PlayerExp>();
        if (playerExp != null)
        {
            playerExp.AddExp(expReward);
        }
    }

    // 敌人死亡生成战利品
    void SpawnLoot()
    {
        // 没有配置掉落表或者掉落预制体直接跳过
        if (enemyLootTable == null || lootItemPrefab == null)
            return;

        // 根据掉落表获取本次要掉落的物品
        var dropResult = enemyLootTable.GetDrops();
        foreach (var dropData in dropResult)
        {
            // 给掉落物一点随机偏移，防止全部堆在一起
            Vector2 randomOffset = new Vector2(Random.Range(-0.6f, 0.6f), Random.Range(-0.6f, 0.6f));
            Vector2 spawnPos = (Vector2)transform.position + randomOffset;

            // 生成地上战利品物体
            GameObject lootObj = Instantiate(lootItemPrefab, spawnPos, Quaternion.identity);
            LootItem loot = lootObj.GetComponent<LootItem>();
            loot.SetLoot(dropData.item, dropData.count);
        }
    }

    // 对象被激活时重置击退标记，防止协程异常卡住状态
    private void OnEnable()
    {
        enermy_move move = GetComponent<enermy_move>();
        if (move != null)
        {
            move.isKnockBack = false;
        }
    }

    // 受伤颜色闪烁协程
    IEnumerator HurtFlash()
    {
        spriteRenderer.color = hurtColor;
        yield return new WaitForSeconds(hurtTime);
        spriteRenderer.color = originalColor;
    }

    // 击退结束，关闭击退标记，恢复敌人移动
    IEnumerator StopKnockBack(enermy_move move)
    {
        yield return new WaitForSeconds(0.3f);
        // 如果物体已经销毁直接退出协程
        if (this == null || move == null) yield break;
        move.isKnockBack = false;
    }
}
