using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NPC对话行为组件：进入对话状态时关闭物理，监听交互按键，启动/推进对话
public class npc_talk : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;

    public Animator interactAnim;
    public DialogueSO dialogueSO;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    //组件启用：进入对话状态
    private void OnEnable()
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; // 禁用物理模拟
        animator.Play("Idle");
        interactAnim.Play("Open");
    }

    //组件禁用：退出对话状态
    private void OnDisable()
    {
        interactAnim.Play("Close");
        rb.isKinematic = false; // 启用物理模拟
    }

    private void Update()
    {
        //按下交互键，有对话则推进，没有则开启新对话
        if (Input.GetButtonDown("Interact"))
        {
            if (DialogueManager.Instance.isDialogueActive)
            {
                DialogueManager.Instance.AdvanceDialogue();
            }
            else
            {
                DialogueManager.Instance.StartDialogue(dialogueSO);
            }
        }
    }
}
