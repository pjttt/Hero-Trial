using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 电梯进入区域：切换碰撞层，修改玩家SortingOrder，实现人物被遮挡效果
public class elevator_entry : MonoBehaviour
{
    public Collider2D[] mountaincolliders;
    public Collider2D[] boundarycolliders;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            foreach (var mountain in mountaincolliders)
            {
                mountain.enabled = false;
            }
            foreach (var boundary in boundarycolliders)
            {
                boundary.enabled = true;
            }
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 15;
            sr.transform.localScale = Vector3.one; //锁定精灵渲染尺寸
        }
    }
}
