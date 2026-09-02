using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NPC闲逛AI组件：在指定矩形范围的边缘随机选取目标点来回移动，到达后等待再选新点
public class npc_wander : MonoBehaviour
{
    [Header("Wander Area")]
    public float wanderWidth = 5f;
    public float wanderHeight = 5f;
    public Vector2 startingPosition;
    public float speed = 2f;

    public Vector2 target;
    private Rigidbody2D rb;
    public float awaittime = 1;
    private bool isPause;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    //组件开启，立刻开始闲逛逻辑
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(PauseAndPickNewDestination());
    }

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if (isPause)
        {
            return;
        }
        // 到达目标点，才暂停+选新目标；被推开不改变target，继续向旧点前进
        float distToTarget = Vector2.Distance(transform.position, target);
        if (distToTarget < .1f)
        {
            StartCoroutine(PauseAndPickNewDestination());
        }
    }

    private void FixedUpdate()
    {
        if (isPause || GameManager.Instance == null)
        {
            rb.velocity = Vector2.zero;
            animator.Play("Idle");
            return;
        }
        // 一直朝着【原来的target】移动，被推撞也不更换目标
        Move();
        // ✅核心：根据刚体速度自动切换动画
        if (Mathf.Abs(rb.velocity.x) > 0.05f)
        {
            animator.Play("Walk");
        }
        else
        {
            animator.Play("Idle");
        }
    }

    private void Move()
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        // 翻转朝向
        if (direction.x < 0 && transform.localScale.x > 0 || direction.x > 0 && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        rb.velocity = direction * speed;
    }

    IEnumerator PauseAndPickNewDestination()
    {
        isPause = true;
        rb.velocity = Vector2.zero;
        animator.Play("Idle");
        yield return new WaitForSeconds(awaittime);
        target = GetRandomTarget();
        isPause = false;
        //❗删掉这里 animator.Play("Walk");
        //动画交给FixedUpdate根据rb.velocity自动控制
    }

    //在闲逛区域四条边上随机生成一个目标坐标
    private Vector2 GetRandomTarget()
    {
        float halfWidth = wanderWidth / 2;
        float halfHeight = wanderHeight / 2;
        int edge = Random.Range(0, 4);
        Vector2 point = startingPosition;
        switch (edge)
        {
            case 0:
                point = new Vector2(startingPosition.x - halfWidth, Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight));
                break;
            case 1:
                point = new Vector2(startingPosition.x + halfWidth, Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight));
                break;
            case 2:
                point = new Vector2(Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth), startingPosition.y - halfHeight);
                break;
            case 3:
                point = new Vector2(Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth), startingPosition.y + halfHeight);
                break;
        }
        return point;
    }
}
