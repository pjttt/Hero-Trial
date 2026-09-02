using UnityEngine;
using UnityEngine.UI;

// 全局拖拽幽灵管理器，保证拖拽图标永远顶层，并且可以强制销毁幽灵
public class DragGhostManager : MonoBehaviour
{
    public static DragGhostManager Instance { get; private set; }

    [Header("拖拽幽灵预制体")]
    public Image dragGhostPrefab;

    private Canvas _ghostCanvas;
    private Image _activeGhost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 创建独立顶层Canvas，专门用于拖拽幽灵，永远最高层级
        GameObject canvasObj = new GameObject("GhostTopCanvas");
        canvasObj.transform.SetParent(transform);
        _ghostCanvas = canvasObj.AddComponent<Canvas>();
        _ghostCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _ghostCanvas.sortingOrder = 9999; // 强制最高渲染层级，不会被任何UI遮挡
        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.GetComponent<GraphicRaycaster>().enabled = false; // 不要阻挡鼠标射线
    }

    // 生成拖拽幽灵
    public Image SpawnGhost(Sprite icon, Vector2 startPos)
    {
        DestroyActiveGhost();
        _activeGhost = Instantiate(dragGhostPrefab, _ghostCanvas.transform);
        _activeGhost.sprite = icon;
        _activeGhost.rectTransform.sizeDelta = new Vector2(48, 48);
        _activeGhost.rectTransform.anchoredPosition = startPos;
        return _activeGhost;
    }

    // 更新幽灵位置
    public void SetGhostPosition(Vector2 screenPos)
    {
        if (_activeGhost == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _activeGhost.rectTransform.parent as RectTransform,
            screenPos,
            null, // ScreenSpace‑Overlay相机传null
            out Vector2 localPos);
        _activeGhost.rectTransform.anchoredPosition = localPos;
    }

    // 销毁当前拖拽幽灵（全局调用，关闭背包时调用这个）
    public void DestroyActiveGhost()
    {
        if (_activeGhost != null)
        {
            Destroy(_activeGhost.gameObject);
            _activeGhost = null;
        }
    }
}
