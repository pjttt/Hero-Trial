using UnityEngine;
using TMPro;

// 金币UI组件，实时同步显示玩家金币数量
public class CoinUI : MonoBehaviour
{
    // 显示金币数字的TMP文本组件
    public TMP_Text coinText;

    void Update()
    {
        // 防止GameManager实例为空、文本组件为空报空引用
        if (GameManager.Instance != null && coinText != null)
        {
            coinText.text = GameManager.Instance.playerCoin.ToString();
        }
    }
}
