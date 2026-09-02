using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 电梯离开区域：恢复山体碰撞，还原玩家渲染层级
public class elevator_exit : MonoBehaviour
{
    public Collider2D[] mountaincolliders;
    public Collider2D[] boundarycolliders;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            foreach (var mountain in mountaincolliders)
            {
                mountain.enabled = true;
            }
            foreach (var boundary in boundarycolliders)
            {
                boundary.enabled = false;
            }
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            sr.transform.localScale = Vector3.one;
        }
    }
}
