using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// 账号管理器：注册、登录校验；账号列表持久化到accounts.json
// 【注意：仅本地Demo演示！商业项目必须后端服务，本地不能做真正安全账号系统】
public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    private string _accountFilePath;
    private AccountListSave _accountList;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _accountFilePath = Path.Combine(Application.persistentDataPath, "accounts.json");
        LoadAllAccounts();
    }

    // 读取全部注册账号
    void LoadAllAccounts()
    {
        if (File.Exists(_accountFilePath))
        {
            string json = File.ReadAllText(_accountFilePath);
            _accountList = JsonUtility.FromJson<AccountListSave>(json);
        }
        else
        {
            _accountList = new AccountListSave();
            _accountList.accounts = Array.Empty<AccountData>();
        }
    }

    // 保存账号列表到本地json
    void SaveAccountFile()
    {
        string json = JsonUtility.ToJson(_accountList, true);
        File.WriteAllText(_accountFilePath, json);
    }

    // 计算密码简单哈希，不保存明文密码
    private string GetPasswordHash(string rawPassword)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(rawPassword);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>注册账号
    /// 返回值：true注册成功；false失败（用户名已存在）
    /// </summary>
    public bool Register(string userName, string rawPassword)
    {
        //查重
        foreach (var acc in _accountList.accounts)
        {
            if (acc.userName.Equals(userName, StringComparison.Ordinal))
            {
                //Debug.LogWarning($"注册失败：用户名 {userName} 已经存在");
                return false;
            }
        }
        //新建账号
        AccountData newAcc = new AccountData
        {
            userName = userName,
            passwordHash = GetPasswordHash(rawPassword)
        };
        //数组扩容添加
        int oldLen = _accountList.accounts.Length;
        AccountData[] newArr = new AccountData[oldLen + 1];
        Array.Copy(_accountList.accounts, newArr, oldLen);
        newArr[oldLen] = newAcc;
        _accountList.accounts = newArr;
        SaveAccountFile();
        //Debug.Log($"✅注册成功：{userName}");
        return true;
    }

    // 登录校验，账号密码正确返回true
    public bool LoginCheck(string userName, string rawPassword)
    {
        string inputHash = GetPasswordHash(rawPassword);
        foreach (var acc in _accountList.accounts)
        {
            if (acc.userName.Equals(userName, StringComparison.Ordinal)
                && acc.passwordHash == inputHash)
            {
                //Debug.Log($"✅登录校验通过 {userName}");
                return true;
            }
        }
        //Debug.LogWarning("❌账号或密码错误");
        return false;
    }
}
