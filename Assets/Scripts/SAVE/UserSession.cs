using UnityEngine;

// 用户会话管理器，保存当前登录的用户名，全局访问
public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    [HideInInspector] public string currentUserName = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 登录成功调用，设置当前账号
    public void SetUser(string userName)
    {
        currentUserName = userName;
        //Debug.Log($"🔑当前登录账号：{currentUserName}");
    }

    // 退出登录清空账号
    public void Logout()
    {
        currentUserName = "";
    }
}
