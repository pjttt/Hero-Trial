using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人检测触发器，检测玩家进入/离开触发区域，切换敌人状态
// 挂载在敌人的检测碰撞体上
public class EnemyDetect : MonoBehaviour
{
    // 敌人移动控制脚本引用
    public enermy_move enemyMove;

    // 玩家进入检测区域
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 切换为追逐状态
            enemyMove.ChangeState(
                enermy_move.EnemyState.Chase
            );
        }
    }

    // 玩家离开检测区域
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 切回闲置状态
            enemyMove.ChangeState(
                enermy_move.EnemyState.Idle
            );
        }
    }
}
