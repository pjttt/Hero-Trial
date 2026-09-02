using System;

// 单个账号信息，存储用户名+哈希密码，不存明文密码【求职Demo】
[Serializable]
public class AccountData
{
    public string userName;
    public string passwordHash;
}

// 账号列表包装类，Unity JsonUtility必须外层套对象，不能直接序列化List
[Serializable]
public class AccountListSave
{
    public AccountData[] accounts;
}
