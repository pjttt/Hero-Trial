using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 玩家移动控制，处理输入、刚体移动、角色翻转、攻击按键输入
public class PlayerMove : MonoBehaviour
{
    public int speed;
    public Rigidbody2D rb;
    public Animator anim;
    public int facingDirection = 1;
    public PlayerAttack playerAttack;

    // 缓存原始标准缩放，永久基准
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        // 强制锁定Y、Z永远不变，只允许X翻转
        originalScale.y = Mathf.Abs(originalScale.y);
        originalScale.z = Mathf.Abs(originalScale.z);
        //自动拿取组件
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;
        if (GameManager.Instance.isBagOpen) return;
        if (Input.GetButtonDown("Slash"))
        {
            // 增加判空保护，防止没拿到组件时报错
            if (playerAttack != null)
            {
                playerAttack.Attack();
            }
        }
    }

    void FixedUpdate()
    {
        // 每一帧强制修正Y/Z缩放，防止变大
        transform.localScale = new Vector3(transform.localScale.x, originalScale.y, originalScale.z);
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        rb.velocity = new Vector2(horizontal, vertical) * speed;
        anim.SetFloat("horizontal", Mathf.Abs(horizontal));
        anim.SetFloat("vertical", Mathf.Abs(vertical));
        if (horizontal > 0 && transform.localScale.x < 0 || horizontal < 0 && transform.localScale.x > 0)
        {
            Filp();
        }
    }

    //翻转角色左右朝向
    void Filp()
    {
        facingDirection *= -1;
        // 基于原始scale翻转，不会累积放大
        transform.localScale = new Vector3(originalScale.x * facingDirection, originalScale.y, originalScale.z);
    }
}
