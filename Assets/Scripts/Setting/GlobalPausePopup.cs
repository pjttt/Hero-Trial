using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 全局暂停弹窗管理器，ESC呼出，包含继续、设置、退出到登录界面功能
public class GlobalPausePopup : MonoBehaviour
{
    public static GlobalPausePopup Instance { get; private set; }

    [Header("主暂停弹窗（继续游戏/设置/退出游戏）")]
    public GameObject mainPausePanel;
    [Header("设置子弹窗（点设置按钮弹出来的面板，可以留空）")]
    public GameObject settingSubPanel;

    private void Awake()
    {
        //单例防复制
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); //全局跨场景不销毁
    }

    void Start()
    {
        //游戏启动默认隐藏弹窗
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        if (settingSubPanel != null) settingSubPanel.SetActive(false);
    }

    void Update()
    {
        //ESC按键：打开/关闭主暂停弹窗
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //如果设置子面板打开，优先关闭子面板
            if (settingSubPanel != null && settingSubPanel.activeSelf)
            {
                settingSubPanel.SetActive(false);
                return;
            }
            if (mainPausePanel != null)
            {
                bool isActive = mainPausePanel.activeSelf;
                mainPausePanel.SetActive(!isActive);
                //弹窗打开时阻止游戏操作，关闭恢复
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.isBagOpen = !isActive;
                }
            }
        }
    }

    //继续游戏按钮：关闭暂停弹窗
    public void Btn_ContinueGame()
    {
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        if (settingSubPanel != null) settingSubPanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.isBagOpen = false;
    }

    //设置按钮：打开设置子面板
    public void Btn_OpenSettingSub()
    {
        if (settingSubPanel != null)
        {
            settingSubPanel.SetActive(true);
        }
    }

    //设置子面板 返回按钮，关闭设置，回到主暂停弹窗
    public void Btn_CloseSettingSub()
    {
        if (settingSubPanel != null)
        {
            settingSubPanel.SetActive(false);
        }
    }

    //退出游戏【返回登录界面】：保存存档，销毁游戏全局对象，切登录场景
    public void Btn_QuitToLogin()
    {
        //第一步强制保存当前全部存档
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveToUserFile();
            //Debug.Log("📦退出前已执行存档保存");
        }
        //销毁游戏DontDestroy对象
        if (GameManager.Instance != null) Destroy(GameManager.Instance.gameObject);
        if (InventoryController.Instance != null) Destroy(InventoryController.Instance.gameObject);
        if (DragGhostManager.Instance != null) Destroy(DragGhostManager.Instance.gameObject);
        UserSession.Instance.Logout();
        //隐藏全局UI，登录页面不要显示暂停弹窗
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        //跳转到登录场景
        SceneManager.LoadScene("LoginUI");
    }

    //    //【可选】真正关闭整个游戏程序的按钮
    //    public void Btn_ReallyQuitApp()
    //    {
    //        if (GameManager.Instance != null) GameManager.Instance.SaveToUserFile();
    //#if UNITY_EDITOR
    //        EditorApplication.isPlaying = false;
    //#else
    //        Application.Quit();
    //#endif
    //    }
}
