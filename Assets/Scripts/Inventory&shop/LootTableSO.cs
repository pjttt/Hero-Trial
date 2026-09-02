using UnityEngine;
using System.Collections.Generic;

//右键菜单：Loot → LootTable 创建掉落表
[CreateAssetMenu(fileName = "New LootTable", menuName = "Loot/LootTable")]
public class LootTableSO : ScriptableObject
{
    //可序列化类，可以在Inspector面板填数据
    [System.Serializable]
    public class LootDrop
    {
        public ItemSO item;               //要掉落哪个物品
        [Range(0f, 100f)] public float dropChance; //掉落概率0‑100
        public int minCount = 1;          //最少掉几个
        public int maxCount = 1;          //最多掉几个
    }

    public List<LootDrop> dropList; //掉落列表，可以填多条掉落

    // 执行随机掉落计算，返回本次实际要掉的物品和数量
    public List<(ItemSO item, int count)> GetDrops()
    {
        List<(ItemSO, int count)> result = new List<(ItemSO, int)>();
        //遍历每一条掉落配置
        foreach (var drop in dropList)
        {
            //0‑100随机数
            float roll = Random.Range(0f, 100f);
            //随机数 ≤概率 → 掉落成功
            if (roll <= drop.dropChance)
            {
                //数量取min~max（Range第二个参数取不到 ，所以+1）
                int count = Random.Range(drop.minCount, drop.maxCount + 1);
                result.Add((drop.item, count));
            }
        }
        return result;
    }
}
