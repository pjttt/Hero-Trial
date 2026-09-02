using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 游戏全局管理器，单例DontDestroyOnLoad
// 管理玩家属性、存档读写、场景切换、淡入淡出、技能面板、相机跟随
public class GameManager : MonoBehaviour
{
    // 全局单例访问入口
    public static GameManager Instance;

    [Header("玩家配置")]
    // 玩家游戏物体引用
    public GameObject player;

    [Header("玩家血量属性")]
    // 玩家最大生命值
    public int playerMaxHealth = 100;
    // 玩家当前生命值
    public int playerHealth = 100;

    [Header("玩家经验等级")]
    // 玩家等级
    public int playerLevel = 1;
    // 当前已获得经验
    public int playerExp = 0;
    // 升级需要总经验
    public int playerMaxExp = 100;

    [Header("玩家金币")]
    // 玩家持有金币
    public int playerCoin = 0;

    [Header("玩家攻击力")]
    // 玩家基础攻击伤害
    public int playerAttackDamage = 1;

    [Header("场景过渡黑屏")]
    // 黑屏遮罩动画控制器
    public Animator fadeAnimator;
    // 淡入淡出过渡时长(秒)
    public float fadeTime = 1f;

    [Header("黑屏遮罩预制体（可选）")]
    // 过渡遮罩预制体，可携带动画控制器
    public GameObject fadeMaskPrefab;

    [HideInInspector]
    // 切换场景目标出口物体名称，用于传送玩家
    public string pendingExitName;

    // 是否正在执行淡入淡出，防止重复调用过渡
    private bool fading;

    [HideInInspector]
    // 背包是否打开，用来屏蔽部分按键输入
    public bool isBagOpen = false;

    [Header("技能面板预制体（拖入你的SkillCanvas预制体）")]
    // 技能UI面板预制体
    public GameObject skillCanvasPrefab;

    // 当前实例化出来的技能面板实例
    private GameObject _skillPanelInstance;
    // 技能面板控制脚本引用
    private SkillPanelCtrl _skillPanelCtrl;

    [Header("状态面板预制体")]
    public GameObject stateCanvasPrefab;
    private GameObject _stateCanvasInstance;



    #region 单例初始化 & 场景加载监听
    private void Awake()
    {
        // 保证全局UI事件系统唯一
        UniqueEventSystem.GetOrCreate();

        // 单例逻辑，重复实例销毁
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 加载本地存档数据
        LoadFromUserFile();
        // 在场景中查找玩家对象
        FindPlayer();
        // 自动创建/获取黑屏过渡遮罩
        FindOrCreateFade();
        // 注册场景加载完成回调
        SceneManager.sceneLoaded += SceneLoaded;



        //实例化状态面板
        if (stateCanvasPrefab != null)
        {
            _stateCanvasInstance = Instantiate(stateCanvasPrefab);
            DontDestroyOnLoad(_stateCanvasInstance);
        }


    }
    #endregion

    private void Update()
    {
        // 按键O打开关闭技能面板
        if (Input.GetKeyDown(KeyCode.O))
        {
            // 背包打开时不响应O，和旧逻辑保持一致
            if (isBagOpen) return;
            ToggleSkillPanel();
        }
    }

    // 动态实例化/关闭技能面板
    void ToggleSkillPanel()
    {
        // 第一次按下O：实例化预制体，之后不再销毁
        if (_skillPanelInstance == null)
        {
            if (skillCanvasPrefab == null)
            {
                //Debug.LogError("GameManager 没有赋值 skillCanvasPrefab！请拖入SkillCanvas预制体");
                return;
            }
            // 实例化UI预制体，不要传父物体transform，保持Overlay根层级
            _skillPanelInstance = Instantiate(skillCanvasPrefab);
            _skillPanelInstance.name = skillCanvasPrefab.name;
            _skillPanelCtrl = _skillPanelInstance.GetComponent<SkillPanelCtrl>();

            if (_skillPanelCtrl == null)
            {
                //Debug.LogError("预制体SkillCanvas上没有挂载 SkillPanelCtrl！");
                Destroy(_skillPanelInstance);
                _skillPanelInstance = null;
                return;
            }
            //刚生成出来默认把内容面板隐藏，等待按键打开
            _skillPanelCtrl.panelContent.SetActive(false);
            _skillPanelCtrl.panelOpen = false;
        }
        // 调用面板开关
        _skillPanelCtrl.TogglePanel();
    }

    #region ==========JSON存档读写【新】==========
    // 从用户存档文件读取角色数据
    void LoadFromUserFile()
    {
        string userName = UserSession.Instance.currentUserName;
        if (string.IsNullOrEmpty(userName))
        {
            //Debug.LogError("没有登录用户！使用默认数据");
            SetDefaultPlayerData();
            return;
        }
        FullUserSave save = UserSaveFileTool.Load(userName);
        if (save == null)
        {
            //Debug.Log("该账号第一次游戏，初始化角色数据");
            SetDefaultPlayerData();
            return;
        }
        // 读取基础角色属性
        playerMaxHealth = save.playerMaxHealth;
        playerHealth = save.playerHealth;
        playerLevel = save.playerLevel;
        playerExp = save.playerExp;
        playerAttackDamage = save.playerAttackDamage;
        playerMaxExp = save.playerMaxExp;
        playerCoin = save.playerCoin;

        // 延迟一帧，等SkillTreeManager.Awake完成再加载技能&背包
        StartCoroutine(DelayLoadSkillAndInventory(save));
    }

    // 延迟加载技能树与背包数据，等待其他管理器Awake执行完毕
    IEnumerator DelayLoadSkillAndInventory(FullUserSave save)
    {
        yield return null;
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.LoadSkillFromSave(save);
        }
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.LoadFromFullSave(save.inventorySave);
        }
        //Debug.Log($"✅延迟读档完成 | HP:{playerHealth}/{playerMaxHealth} Lv:{playerLevel} Exp:{playerExp}/{playerMaxExp} Coin:{playerCoin}");
    }

    // 设置玩家默认初始数据
    void SetDefaultPlayerData()
    {
        playerMaxHealth = 100;
        playerHealth = 100;
        playerLevel = 1;
        playerExp = 0;
        playerMaxExp = 100;
        playerCoin = 0;
        playerAttackDamage = 1;

        InventoryController.Instance?.ClearAllInventory();
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.SetDefaultSkillData();
        }
    }

    // 将当前游戏数据保存到用户存档文件
    public void SaveToUserFile()
    {
        string userName = UserSession.Instance.currentUserName;
        if (string.IsNullOrEmpty(userName))
        {
            //Debug.LogWarning("未登录账号，跳过保存");
            return;
        }
        FullUserSave saveObj = new FullUserSave();
        saveObj.playerMaxHealth = playerMaxHealth;
        saveObj.playerHealth = playerHealth;
        saveObj.playerLevel = playerLevel;
        saveObj.playerExp = playerExp;
        saveObj.playerMaxExp = playerMaxExp;
        saveObj.playerCoin = playerCoin;
        saveObj.playerAttackDamage = playerAttackDamage;

        // 写入技能树数据
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.WriteSkillToSave(ref saveObj);
        }
        // 写入背包数据
        if (InventoryController.Instance != null)
        {
            saveObj.inventorySave = InventoryController.Instance.GetInventorySaveData();
        }
        UserSaveFileTool.Save(userName, saveObj);
    }
    #endregion

    #region 玩家查找与状态重置
    // 在场景查找Player标签玩家物体，处理多玩家副本
    void FindPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            //Debug.LogError("没有找到Player，请检查物体Tag是否为Player");
            return;
        }
        if (players.Length > 1)
        {
            //Debug.LogWarning($"找到 {players.Length} 个 Player，只保留第一个");
            for (int i = 1; i < players.Length; i++)
            {
                Destroy(players[i]);
            }
        }
        player = players[0];
        ResetPlayerState();

        //同步玩家攻击伤害
        PlayerAttack patk = player.GetComponent<PlayerAttack>();
        if (patk != null)
        {
            patk.damaged = playerAttackDamage;
        }
        //Debug.Log($"✅ 找到玩家: {player.name}");
    }

    // 重置玩家刚体、旋转、缩放等状态，切换场景后调用
    void ResetPlayerState()
    {
        if (player == null) return;
        player.transform.localScale = Vector3.one;
        player.transform.rotation = Quaternion.identity;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.WakeUp();
        }
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 5;
        player.SetActive(true);
    }
    #endregion

    #region 场景加载后初始化流程
    // 场景加载完成回调入口
    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindOrCreateFade();
        StartCoroutine(InitScene());
    }

    // 场景初始化协程：清理UI、恢复时间、查找玩家、传送、绑定相机、淡入
    IEnumerator InitScene()
    {
        // 切换场景销毁动态生成的技能面板
        if (_skillPanelInstance != null)
        {
            Destroy(_skillPanelInstance);
            _skillPanelInstance = null;
            _skillPanelCtrl = null;
        }
        Time.timeScale = 1f;
        yield return null;

        FindPlayer();
        TeleportPlayer();
        SetupCamera();

        yield return StartCoroutine(StartFadeIn());
        //Debug.Log($"✅ 场景初始化完成: {SceneManager.GetActiveScene().name}");
    }
    #endregion

    #region 相机绑定
    // 查找Cinemachine虚拟相机，设置跟随玩家
    void SetupCamera()
    {
        CinemachineVirtualCamera cam = FindObjectOfType<CinemachineVirtualCamera>();
        if (cam == null)
        {
            //Debug.LogError("没有找到Virtual Camera");
            return;
        }
        if (player == null)
        {
            //Debug.LogError("Camera绑定失败，没有Player");
            return;
        }
        cam.Follow = player.transform;
        cam.LookAt = player.transform;
        //Debug.Log("摄像机绑定:" + player.name);

        CinemachineConfiner confiner = cam.GetComponent<CinemachineConfiner>();
        if (confiner != null) confiner.InvalidatePathCache();
    }
    #endregion

    #region 场景传送瞬移逻辑
    // 根据pendingExitName，把玩家传送到目标出口位置
    void TeleportPlayer()
    {
        if (string.IsNullOrEmpty(pendingExitName) || player == null) return;
        GameObject exit = GameObject.Find(pendingExitName);
        if (exit == null)
        {
            //Debug.LogError("找不到出口：" + pendingExitName);
            return;
        }

        Collider2D playerCollider = player.GetComponent<CapsuleCollider2D>();
        bool colliderIsActive = playerCollider.enabled;
        // 暂时关闭碰撞防止传送瞬间卡墙
        playerCollider.enabled = false;

        Vector3 safePos = exit.transform.position;
        safePos.y += 0.6f;
        player.transform.position = safePos;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        player.transform.localScale = Vector3.one;
        player.transform.rotation = Quaternion.identity;

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = false;
            sr.enabled = false;
            sr.enabled = true;
        }
        //延迟恢复碰撞体
        StartCoroutine(RestoreCollider(playerCollider, colliderIsActive));
        pendingExitName = "";
    }

    // 延迟恢复玩家碰撞体，避免传送卡墙
    IEnumerator RestoreCollider(Collider2D collider, bool originalState)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(0.2f);
        collider.enabled = originalState;
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
        rb.WakeUp();
    }
    #endregion

    #region 全局黑屏遮罩自动创建
    // 查找或自动生成FadeMask黑屏过渡UI
    void FindOrCreateFade()
    {
        GameObject fade = GameObject.Find("FadeMask");
        if (fade != null)
        {
            fadeAnimator = fade.GetComponent<Animator>();
            //Debug.Log("找到现有的FadeMask");
            return;
        }
        //Debug.Log("没有找到FadeMask，自动创建...");

        Canvas existingCanvas = FindObjectOfType<Canvas>();
        GameObject canvasGO;
        if (existingCanvas != null)
        {
            canvasGO = existingCanvas.gameObject;
            //Debug.Log("复用已经存在的Canvas，不新建Canvas");
        }
        else
        {
            canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(null);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
            //Debug.Log("不得已新建了FadeCanvas");
        }

        Transform existingFade = canvasGO.transform.Find("FadeMask");
        GameObject imageGO;
        if (existingFade != null)
        {
            imageGO = existingFade.gameObject;
            //Debug.Log("使用现有的FadeMask");
        }
        else
        {
            imageGO = new GameObject("FadeMask");
            imageGO.transform.SetParent(canvasGO.transform, false);
            imageGO.transform.SetAsLastSibling();
            Image image = imageGO.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
            RectTransform rect = imageGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            //Debug.Log("创建了新的FadeMask");
        }

        Animator animator = imageGO.GetComponent<Animator>();
        if (animator == null)
        {
            animator = imageGO.AddComponent<Animator>();
        }
        if (fadeMaskPrefab != null)
        {
            Animator prefabAnimator = fadeMaskPrefab.GetComponent<Animator>();
            if (prefabAnimator != null && prefabAnimator.runtimeAnimatorController != null)
            {
                animator.runtimeAnimatorController = prefabAnimator.runtimeAnimatorController;
                //Debug.Log("使用Prefab的Animator Controller");
            }
        }
        fadeAnimator = animator;
        //Debug.Log("✅ FadeMask 创建完成");
    }
    #endregion

    #region 淡入淡出动画
    // 淡出（画面变黑）协程
    public IEnumerator StartFadeOut()
    {
        if (fading) yield break;
        fading = true;
        if (fadeAnimator != null)
        {
            if (HasAnimationState("FadeOut"))
            {
                fadeAnimator.Play("FadeOut");
                yield return null;
            }
            else
            {
                Image image = fadeAnimator.GetComponent<Image>();
                if (image != null)
                {
                    float elapsed = 0f;
                    Color color = image.color;
                    while (elapsed < fadeTime)
                    {
                        elapsed += Time.deltaTime;
                        float alpha = Mathf.Lerp(0, 1, elapsed / fadeTime);
                        color.a = alpha;
                        image.color = color;
                        yield return null;
                    }
                    color.a = 1;
                    image.color = color;
                }
            }
        }
        yield return new WaitForSeconds(fadeTime);
    }

    // 淡入（画面恢复显示）协程
    public IEnumerator StartFadeIn()
    {
        if (fadeAnimator != null)
        {
            if (HasAnimationState("FadeIn"))
            {
                fadeAnimator.Play("FadeIn");
                yield return null;
            }
            else
            {
                Image image = fadeAnimator.GetComponent<Image>();
                if (image != null)
                {
                    float elapsed = 0f;
                    Color color = image.color;
                    while (elapsed < fadeTime)
                    {
                        elapsed += Time.deltaTime;
                        float alpha = Mathf.Lerp(1, 0, elapsed / fadeTime);
                        color.a = alpha;
                        image.color = color;
                        yield return null;
                    }
                    color.a = 0;
                    image.color = color;
                }
            }
        }
        yield return new WaitForSeconds(fadeTime);
        fading = false;
    }

    // 判断Animator是否包含指定名称动画状态
    private bool HasAnimationState(string stateName)
    {
        if (fadeAnimator == null || fadeAnimator.runtimeAnimatorController == null) return false;
        foreach (AnimationClip clip in fadeAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName) return true;
        }
        return false;
    }
    #endregion

    #region 生命周期销毁清理
    private void OnDestroy()
    {
        //移除事件订阅，防止内存泄漏
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    // 应用退出时执行保存
    private void OnApplicationQuit()
    {
        Time.timeScale = 1; //退出前恢复时间
        Instance = null;
        SaveToUserFile();
    }
    #endregion
}
