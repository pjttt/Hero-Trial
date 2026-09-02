using UnityEngine;
using UnityEngine.UI;

// 单个技能树节点UI，处理点击升级，刷新显示等级
public class SkillNodeUI : MonoBehaviour
{
    [Header("分配这个按钮属于哪里")]
    public bool isHpTree;
    public int rowIndex;
    public int colIndex;

    [Header("UI拖拽赋值，来自预制体内部")]
    public Text txtShow;
    public Button btn;

    void Start()
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
        // 节点生成后通知管理器重新收集全部UI节点
        SkillTreeManager.Instance?.CollectSkillNodeUI();
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (SkillTreeManager.Instance == null) return;
        var mgr = SkillTreeManager.Instance;
        int currentLv;
        int maxLv;
        bool canUse = mgr.CanUpgrade(isHpTree, rowIndex, colIndex);

        if (isHpTree)
        {
            currentLv = mgr.hpSkillLevels[rowIndex, colIndex];
            maxLv = mgr.hpRowMaxPoints[rowIndex];
        }
        else
        {
            currentLv = mgr.powerSkillLevels[rowIndex, colIndex];
            maxLv = mgr.powerRowMaxPoints[rowIndex];
        }

        txtShow.text = $"{currentLv}/{maxLv}";
        btn.interactable = canUse;
    }

    void OnClick()
    {
        if (SkillTreeManager.Instance == null) return;
        bool ok = SkillTreeManager.Instance.TryUpgrade(isHpTree, rowIndex, colIndex);
        if (ok) RefreshUI();
    }
}
