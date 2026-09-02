using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NPC状态管理器，统一控制巡逻/闲逛/对话/待机状态，启用关闭对应AI组件
public class npc_manager : MonoBehaviour
{
    public enum NPCState { Default, Idle, Patrol, Wander, Talk }

    public NPCState currentState = NPCState.Patrol;
    public NPCState defaultState;

    public npc_patrol patrol;
    public npc_talk talk;
    public npc_wander wander;

    void Start()
    {
        defaultState = currentState;
        SwitchState(currentState);
    }

    private void SwitchState(NPCState newState)
    {
        // 第一步：处理Default，直接解析成真实目标状态，禁止递归
        NPCState targetState = newState == NPCState.Default ? defaultState : newState;
        currentState = targetState;

        // 统一关闭全部行为组件，避免多组件同时激活（核心！防止多个AI逻辑一起跑）
        patrol.enabled = false;
        wander.enabled = false;
        talk.enabled = false;

        // 根据目标状态开启对应组件
        switch (targetState)
        {
            case NPCState.Patrol:
                patrol.enabled = true;
                break;
            case NPCState.Wander:
                wander.enabled = true;
                break;
            case NPCState.Talk:
                talk.enabled = true;
                break;
            case NPCState.Idle:
                // 空闲，全部保持关闭即可
                break;
        }
    }

    // 玩家进入触发区域，切换为对话状态
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SwitchState(NPCState.Talk);
        }
    }

    // 玩家离开触发区域，切回NPC默认状态
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SwitchState(defaultState);
        }
    }
}
