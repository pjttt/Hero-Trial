using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO;

// 登录注册界面管理器，处理账号输入、注册校验、弹窗提示、音效开关
public class LoginManager : MonoBehaviour
{
    [Header("面板引用")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("登录输入框")]
    public TMP_InputField loginUserInput;
    public TMP_InputField loginPwdInput;

    [Header("注册输入框")]
    public TMP_InputField regUserInput;
    public TMP_InputField regPwdInput;
    public TMP_InputField regConfirmPwdInput;

    [Header("音效设置")]
    public AudioSource bgmAudioSource;
    public Button audioBtn;
    private bool isAudioOn = true;

    [Header("喇叭图片切换")]
    public Image audioBtnImage;
    public Sprite spriteSoundOn;
    public Sprite spriteSoundOff;

    [Header("场景名字")]
    public string gameSceneName = "SampleScene";

    [Header("提示弹窗")]
    public GameObject tipPopup;
    public TMP_Text tipText;
    public Button tipOkBtn;

    private CanvasGroup tipCanvasGroup;

    void Start()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        audioBtn.onClick.AddListener(ToggleAudio);
        audioBtnImage.sprite = spriteSoundOn;
        tipCanvasGroup = tipPopup.GetComponent<CanvasGroup>();
        tipOkBtn.onClick.AddListener(CloseTipAndJumpLogin);
        tipPopup.SetActive(false);
    }

    #region 面板切换
    public void ShowRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        ClearAllInput();
    }

    public void ShowLogin()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        ClearAllInput();
    }

    void ClearAllInput()
    {
        loginUserInput.text = "";
        loginPwdInput.text = "";
        regUserInput.text = "";
        regPwdInput.text = "";
        regConfirmPwdInput.text = "";
    }
    #endregion

    #region 登录逻辑
    public void OnLoginClick()
    {
        ShowTip("登录按钮被点击了！");
        string inputUser = loginUserInput.text.Trim();
        string inputPwd = loginPwdInput.text.Trim();
        if (string.IsNullOrEmpty(inputUser) || string.IsNullOrEmpty(inputPwd))
        {
            ShowTip("账号密码不能为空！");
            return;
        }
        //调用账号管理器校验
        bool pass = AccountManager.Instance.LoginCheck(inputUser, inputPwd);
        if (pass)
        {
            ShowTip("登录成功，即将进入游戏");
            UserSession.Instance.SetUser(inputUser);
            StartCoroutine(DelayLoadScene(1.2f));
        }
        else
        {
            ShowTip("账号或密码错误");
        }
    }
    #endregion

    #region 注册逻辑
    public void OnRegisterOKClick()
    {
        string regUser = regUserInput.text.Trim();
        string regPwd = regPwdInput.text.Trim();
        string confirmPwd = regConfirmPwdInput.text.Trim();

        //过滤文件名非法字符（用户名会作为存档json文件名）
        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (regUser.IndexOfAny(invalidChars) >= 0)
        {
            ShowTip($"用户名不能包含特殊字符：{new string(invalidChars)}");
            return;
        }
        if (string.IsNullOrEmpty(regUser) || string.IsNullOrEmpty(regPwd))
        {
            ShowTip("账号密码不能为空！");
            return;
        }
        if (regPwd != confirmPwd)
        {
            ShowTip("两次输入的密码不一致！");
            return;
        }
        bool regOk = AccountManager.Instance.Register(regUser, regPwd);
        if (regOk)
        {
            ShowTip("注册成功！点击确定返回登录");
        }
        else
        {
            ShowTip("注册失败：该用户名已经被占用");
        }
    }
    #endregion

    #region 弹窗逻辑
    void ShowTip(string msg)
    {
        StopAllCoroutines();
        tipPopup.SetActive(true);
        tipText.text = msg;
        tipCanvasGroup.alpha = 1;
    }

    void CloseTipAndJumpLogin()
    {
        tipPopup.SetActive(false);
        if (registerPanel.activeSelf)
        {
            ShowLogin();
        }
    }
    #endregion

    #region 延迟加载场景
    IEnumerator DelayLoadScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(gameSceneName);
    }
    #endregion

    #region 音效开关
    void ToggleAudio()
    {
        isAudioOn = !isAudioOn;
        bgmAudioSource.mute = !isAudioOn;
        audioBtnImage.sprite = isAudioOn ? spriteSoundOn : spriteSoundOff;
    }
    #endregion
}
