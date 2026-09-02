using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NPC巡逻AI组件：沿着指定路点循环巡逻，到达点后等待一段时间再去下一个点
public class npc_patrol : MonoBehaviour
{
    public Vector2[] patroPoints;
    public float speed = 2f;
    public float waitTime = 1.5f;

    private bool isWaiting;
    private Vector2 target;
    private Rigidbody2D rb;
    private Animator animator;
    private int currentPatrolIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        StartCoroutine(SetPatrolPoint());
    }

    void Update()
    {
        if (GameManager.Instance == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if (isWaiting) return;
        // 距离目标点足够近，切换下一个巡逻点
        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            StartCoroutine(SetPatrolPoint());
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null || isWaiting)
        {
            rb.velocity = Vector2.zero;
            animator.Play("Idle");
            return;
        }

        Vector2 direction = (target - (Vector2)transform.position).normalized;
        // 根据移动方向翻转人物朝向
        if (direction.x < 0 && transform.localScale.x > 0 || direction.x > 0 && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        rb.velocity = direction * speed;
        // 根据刚体速度自动切换动画，被推撞也不会卡Idle
        if (Mathf.Abs(rb.velocity.x) > 0.05f)
        {
            animator.Play("Walk");
        }
        else
        {
            animator.Play("Idle");
        }
    }

    IEnumerator SetPatrolPoint()
    {
        isWaiting = true;
        animator.Play("Idle");
        yield return new WaitForSeconds(waitTime);
        // 循环取巡逻索引
        currentPatrolIndex = (currentPatrolIndex + 1) % patroPoints.Length;
        target = patroPoints[currentPatrolIndex];
        isWaiting = false;
        //❗删掉原来这里 animator.Play("Walk"); 交给FixedUpdate自动处理
    }
}
