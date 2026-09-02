using UnityEngine;
using UnityEngine.UI;

// 技能面板控制器，打开关闭技能面板，暂停游戏，刷新技能点文本
public class SkillPanelCtrl : MonoBehaviour
{
    [Header("把你SkillPoints下面points文本拖进来")]
    public Text pointText;

    [Header("技能面板内容物体（要隐藏显示的面板本体）")]
    public GameObject panelContent;

    [HideInInspector] public bool panelOpen;

    //外部调用开关面板（由GameManager调用）
    public void TogglePanel()
    {
        panelOpen = !panelOpen;
        panelContent.SetActive(panelOpen);
        if (panelOpen)
        {
            Time.timeScale = 0;
            UpdateSkillPointText();
            SkillTreeManager.Instance?.RefreshAllSkillUI();
        }
        else
        {
            Time.timeScale = 1;
            DragGhostManager.Instance?.DestroyActiveGhost();
        }
    }

    public void UpdateSkillPointText()
    {
        if (pointText == null) return;
        if (SkillTreeManager.Instance == null)
        {
            pointText.text = "0";
            return;
        }
        pointText.text = SkillTreeManager.Instance.availableSkillPoint.ToString();
    }

    public void ClosePanel()
    {
        panelOpen = false;
        panelContent.SetActive(false);
        Time.timeScale = 1;
    }
}
