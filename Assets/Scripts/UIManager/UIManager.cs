using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI管理器单例，基础DontDestroyOnLoad占位，后续可扩展全局UI逻辑
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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
}
