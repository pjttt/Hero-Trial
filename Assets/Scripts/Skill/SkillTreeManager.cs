using UnityEngine;
using System.Collections.Generic;

// 技能树管理器：技能点计算、加点校验、升级逻辑、读档存档、洗点
public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance { get; private set; }

    [Header("生命天赋配置")]
    public List<int> hpRowMaxPoints = new List<int>() { 1, 1, 1 };
    public int healthAddPerPoint = 20;

    [Header("力量天赋配置")]
    public List<int> powerRowMaxPoints = new List<int>() { 1, 1, 1 };
    public int powerAddPerPoint = 2;

    //这里现在主要保存【任务/道具赠送的额外技能点】，升级基础点运行时计算
    [HideInInspector] public int availableSkillPoint;
    [HideInInspector] public int[,] hpSkillLevels;
    [HideInInspector] public int[,] powerSkillLevels;

    public int hpRowCount = 3;
    public int hpColCount = 3;
    public int powerRowCount = 3;
    public int powerColCount = 3;

    private List<SkillNodeUI> _cachedSkillNodes = new List<SkillNodeUI>();
    private PlayerAttack _cachedPlayerAttack;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitSkillArrays();
        CachePlayerAttack();
    }

    void CachePlayerAttack()
    {
        if (_cachedPlayerAttack == null)
            _cachedPlayerAttack = FindObjectOfType<PlayerAttack>();
    }

    public void CollectSkillNodeUI()
    {
        _cachedSkillNodes.Clear();
        var arr = Object.FindObjectsOfType<SkillNodeUI>();
        _cachedSkillNodes.AddRange(arr);
    }

    public void InitSkillArrays()
    {
        hpSkillLevels = new int[hpRowCount, hpColCount];
        powerSkillLevels = new int[powerRowCount, powerColCount];
    }

    //统计已经消耗掉的技能点（所有技能等级累加）
    public int CalcUsedSkillPoints()
    {
        int used = 0;
        foreach (var v in hpSkillLevels) used += v;
        foreach (var v in powerSkillLevels) used += v;
        return used;
    }

    //根据玩家等级重新矫正可用技能点；读档、升级、点技能、洗点调用
    public void RecalcAvailablePoints()
    {
        if (GameManager.Instance == null) return;
        // Lv1=0，每升一级给1点，升级带来的总基础点数
        int totalBaseGranted = GameManager.Instance.playerLevel - 1;
        int used = CalcUsedSkillPoints();
        // 提取【额外赠送点】：availableSkillPoint中不属于升级得来的部分
        int bonusPoints = availableSkillPoint - (totalBaseGranted - used);
        if (bonusPoints < 0) bonusPoints = 0;
        // 最终可用点数 = 升级基础剩余 + 任务道具赠送点
        availableSkillPoint = (totalBaseGranted - used) + bonusPoints;
        if (availableSkillPoint < 0)
            availableSkillPoint = 0;
        //Debug.Log($"[技能点重算]总基础获得:{totalBaseGranted} 已消耗:{used} 额外赠送:{bonusPoints} 可用:{availableSkillPoint}");
    }

    //任务/道具获得额外技能点（不是升级给的）
    public void AddSkillPoint(int count = 1)
    {
        availableSkillPoint += count;
        RecalcAvailablePoints();
        GameManager.Instance?.SaveToUserFile();
        RefreshAllSkillUI();
    }

    public bool IsRowFull(int rowIdx, int[,] skillArray, int colCount, List<int> rowMax)
    {
        for (int col = 0; col < colCount; col++)
        {
            if (skillArray[rowIdx, col] < rowMax[rowIdx])
                return false;
        }
        return true;
    }

    public bool CanUpgrade(bool isHpTree, int row, int col)
    {
        if (availableSkillPoint <= 0) return false;
        int[,] targetArray = isHpTree ? hpSkillLevels : powerSkillLevels;
        List<int> rowMaxList = isHpTree ? hpRowMaxPoints : powerRowMaxPoints;
        int colCnt = isHpTree ? hpColCount : powerColCount;

        int currentLv = targetArray[row, col];
        int maxLv = rowMaxList[row];
        if (currentLv >= maxLv) return false;

        if (row > 0)
        {
            if (!IsRowFull(row - 1, targetArray, colCnt, rowMaxList))
            {
                return false;
            }
        }
        return true;
    }

    public bool TryUpgrade(bool isHpTree, int row, int col)
    {
        if (!CanUpgrade(isHpTree, row, col)) return false;
        availableSkillPoint--;
        int[,] targetArray = isHpTree ? hpSkillLevels : powerSkillLevels;
        targetArray[row, col] += 1;

        if (isHpTree)
        {
            GameManager.Instance.playerMaxHealth += healthAddPerPoint;
            GameManager.Instance.playerHealth += healthAddPerPoint;
        }
        else
        {
            //力量天赋加点，修改全局GameManager攻击力，不要直接改PlayerAttack
            GameManager.Instance.playerAttackDamage += powerAddPerPoint;
            //同步刷新场景上PlayerAttack组件的值
            if (_cachedPlayerAttack == null) CachePlayerAttack();
            if (_cachedPlayerAttack != null)
            {
                _cachedPlayerAttack.damaged = GameManager.Instance.playerAttackDamage;
            }
        }

        RecalcAvailablePoints();
        GameManager.Instance.SaveToUserFile();
        RefreshAllSkillUI();
        return true;
    }

    public void RefreshAllSkillUI()
    {
        foreach (var n in _cachedSkillNodes)
        {
            if (n != null) n.RefreshUI();
        }
        SkillPanelCtrl ctrl = Object.FindObjectOfType<SkillPanelCtrl>();
        ctrl?.UpdateSkillPointText();
    }

    #region 存档读写
    public void LoadSkillFromSave(FullUserSave save)
    {
        int oldSavedAvailable = save.availableSkillPoint;
        InitSkillArrays();
        if (save.hpSkillSave == null) save.hpSkillSave = new List<int>();
        if (save.powerSkillSave == null) save.powerSkillSave = new List<int>();

        int idx = 0;
        for (int r = 0; r < hpRowCount; r++)
        {
            for (int c = 0; c < hpColCount; c++)
            {
                if (idx < save.hpSkillSave.Count)
                    hpSkillLevels[r, c] = save.hpSkillSave[idx];
                else
                    hpSkillLevels[r, c] = 0;
                idx++;
            }
        }

        idx = 0;
        for (int r = 0; r < powerRowCount; r++)
        {
            for (int c = 0; c < powerColCount; c++)
            {
                if (idx < save.powerSkillSave.Count)
                    powerSkillLevels[r, c] = save.powerSkillSave[idx];
                else
                    powerSkillLevels[r, c] = 0;
                idx++;
            }
        }

        // 读入存档保存的额外赠送点
        availableSkillPoint = oldSavedAvailable;
        // 根据等级、已消耗重算，还原本次登录剩余点数
        RecalcAvailablePoints();

        //读档加载完力量技能，重新计算攻击力，覆盖GameManager的值
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerAttackDamage = CalcTotalPowerAttack();
        }
        CachePlayerAttack();
        RefreshAllSkillUI();
    }

    public void WriteSkillToSave(ref FullUserSave save)
    {
        // 存入存档：这里存的是【额外赠送点】，不是完整剩余点数
        save.availableSkillPoint = availableSkillPoint;
        List<int> hpList = new List<int>();
        List<int> powList = new List<int>();
        foreach (var v in hpSkillLevels) hpList.Add(v);
        foreach (var v in powerSkillLevels) powList.Add(v);
        save.hpSkillSave = hpList;
        save.powerSkillSave = powList;
    }

    public void SetDefaultSkillData()
    {
        availableSkillPoint = 0;
        InitSkillArrays();
    }

    //洗点功能，重置全部技能，保留任务道具赠送点
    public void ResetAllSkills()
    {
        InitSkillArrays();
        //洗点完重新计算攻击力，写回GameManager全局
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerAttackDamage = CalcTotalPowerAttack();
            //同步场景PlayerAttack
            if (_cachedPlayerAttack == null) CachePlayerAttack();
            if (_cachedPlayerAttack != null)
            {
                _cachedPlayerAttack.damaged = GameManager.Instance.playerAttackDamage;
            }
        }
        RecalcAvailablePoints();
        GameManager.Instance.SaveToUserFile();
        RefreshAllSkillUI();
        //Debug.Log("✅全部技能已重置");
    }

    //统计全部力量技能点总和，计算总攻击力（洗点/读档时调用）
    public int CalcTotalPowerAttack()
    {
        int totalPowerPoint = 0;
        foreach (var v in powerSkillLevels)
        {
            totalPowerPoint += v;
        }
        int baseDmg = 1; //基础攻击
        return baseDmg + totalPowerPoint * powerAddPerPoint;
    }
    #endregion
}
