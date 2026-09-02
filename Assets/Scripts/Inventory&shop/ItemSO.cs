using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item")]
public class ItemSO : ScriptableObject
{
    //物品名字
    public string itemName;
    //物品描述
    public string itemDescription;
    //物品图标
    public Sprite icon;

    [Header("食用回血设置")]
    public bool isEdible;          // 是否可以食用
    public int healAmount;         // 食用回复多少血量
    public AudioClip eatSound;     // 吃东西音效，食物才赋值

    [Header("拾取音效")]
    public AudioClip pickupSound;  // 拾取该物品播放的音效
}
