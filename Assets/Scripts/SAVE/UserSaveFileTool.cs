using System.IO;
using UnityEngine;

// 用户存档静态工具类，读写每个账号独立json存档
public static class UserSaveFileTool
{
    // 获取账号存档完整路径 PersistentDataPath/Saves/用户名.json
    public static string GetSaveFilePath(string userName)
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        string fileName = $"{userName}.json";
        return Path.Combine(saveFolder, fileName);
    }

    // 保存完整账号存档
    public static void Save(string userName, FullUserSave saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        string path = GetSaveFilePath(userName);
        File.WriteAllText(path, json);
        //Debug.Log($"✅保存账号[{userName}]存档：{path}");
    }

    // 读取账号存档，无存档返回null
    public static FullUserSave Load(string userName)
    {
        string path = GetSaveFilePath(userName);
        if (!File.Exists(path))
        {
            //Debug.Log($"📄账号[{userName}]无存档文件");
            return null;
        }
        string json = File.ReadAllText(path);
        FullUserSave data = JsonUtility.FromJson<FullUserSave>(json);
        //Debug.Log($"📂读取账号[{userName}]存档");
        return data;
    }

    public static bool HasSave(string userName)
    {
        return File.Exists(GetSaveFilePath(userName));
    }

    public static void DeleteSave(string userName)
    {
        string path = GetSaveFilePath(userName);
        if (File.Exists(path)) File.Delete(path);
    }
}
