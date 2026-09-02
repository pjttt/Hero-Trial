using System;
using System.Collections.Generic;

// 一个账号完整存档：角色数据+背包数据
[Serializable]
public class FullUserSave
{
    //角色属性
    public int playerMaxHealth;
    public int playerHealth;
    public int playerLevel;
    public int playerExp;
    public int playerMaxExp;
    public int playerCoin;
    public int playerAttackDamage;

    //新增技能存档数据
    public int availableSkillPoint;
    public List<int> hpSkillSave;
    public List<int> powerSkillSave;

    //背包存档，服用你已有的 InventorySaveData
    public InventorySaveData inventorySave;
}
