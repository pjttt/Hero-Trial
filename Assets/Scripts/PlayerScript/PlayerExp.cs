using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 玩家经验与等级管理，处理获取经验、升级，更新UI，同步存档
public class PlayerExp : MonoBehaviour
{
    public Slider expBar;
    public int Level = 1;
    public int currentExp = 0;
    public int maxExp = 100;
    public TMP_Text currentlevelText;

    void Start()
    {
        Level = GameManager.Instance.playerLevel;
        currentExp = GameManager.Instance.playerExp;
        maxExp = GameManager.Instance.playerMaxExp;
        expBar.maxValue = maxExp;
        expBar.value = currentExp;
        UpdateUI();
    }

    //增加经验值，自动处理多次升级
    public void AddExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= maxExp)
        {
            LevelUp();
        }
        UpdateExpBar();
        // 同步到全局并保存
        GameManager.Instance.playerLevel = Level;
        GameManager.Instance.playerExp = currentExp;
        GameManager.Instance.playerMaxExp = maxExp;
        GameManager.Instance.SaveToUserFile();

    }

    void UpdateExpBar()
    {
        expBar.value = currentExp;
    }

    //升级逻辑，提升等级，提升升级所需经验，刷新技能点
    void LevelUp()
    {
        Level++;
        currentExp -= maxExp;
        maxExp += 50;
        expBar.maxValue = maxExp;
        UpdateUI();
        GameManager.Instance.playerLevel = Level;
        GameManager.Instance.playerExp = currentExp;
        GameManager.Instance.playerMaxExp = maxExp;
        if (SkillTreeManager.Instance != null)
        {
            //升级后自动重算技能点
            SkillTreeManager.Instance.RecalcAvailablePoints();
        }
        GameManager.Instance.SaveToUserFile();
    }

    public void UpdateUI()
    {
        currentlevelText.text = "LEVEL  " + Level;
    }
}
