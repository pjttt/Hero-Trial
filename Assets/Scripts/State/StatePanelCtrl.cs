using UnityEngine;
using UnityEngine.UI;

// 脚本挂载：StateCanvas【Canvas根物体】
public class StatePanelCtrl : MonoBehaviour
{
    [Header("拖拽赋值")]
    public GameObject statePanelContent;    // 子物体：StatePanel（内容面板）
    public GameObject backgroundGrid;        // StatePanel下的background
    public GameObject statItemPrefab;       // max_health条目预制体

    private bool _isOpen;

    void Awake()
    {
        // 初始隐藏内容面板，Canvas根本体保持激活，脚本不会休眠
        _isOpen = false;
        statePanelContent.SetActive(false);
    }

    void Update()
    {
        // I键切换
        if (Input.GetKeyDown(KeyCode.I))
        {
            TogglePanel();
        }
        // ESC关闭
        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }

        // 面板打开状态，实时刷新属性
        if (_isOpen)
        {
            RefreshAllStatItems();
        }
    }

    // 切换打开关闭，只操作子内容面板，Canvas根不会被关闭
    public void TogglePanel()
    {
        _isOpen = !_isOpen;
        statePanelContent.SetActive(_isOpen);

        if (_isOpen)
        {
            RefreshAllStatItems();
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    // 清空并重新生成属性条目
    void RefreshAllStatItems()
    {
        foreach (Transform child in backgroundGrid.transform)
        {
            Destroy(child.gameObject);
        }

        if (GameManager.Instance == null) return;

        AddStatItem($"最大生命值：{GameManager.Instance.playerMaxHealth}");
        AddStatItem($"当前生命值：{GameManager.Instance.playerHealth}");
        AddStatItem($"攻击力：{GameManager.Instance.playerAttackDamage}");
        AddStatItem($"等级：{GameManager.Instance.playerLevel}");
        AddStatItem($"金币：{GameManager.Instance.playerCoin}");
    }

    // 实例化单条属性UI
    void AddStatItem(string contentText)
    {
        GameObject itemObj = Instantiate(statItemPrefab, backgroundGrid.transform);
        Text txt = itemObj.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.text = contentText;
        }
    }
}
