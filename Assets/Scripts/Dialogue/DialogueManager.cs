using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 对话管理器，单例模式，控制对话显示、翻页、选项分支、对话结束逻辑
public class DialogueManager : MonoBehaviour
{
    // 单例实例，全局访问入口
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue UI")]
    // 对话整体UI画布组，控制显隐、交互、射线阻挡
    public CanvasGroup canvasGroup;
    // 角色头像图片组件
    public Image portrait;
    // 角色名称文本
    public TMP_Text actorName;
    // 对话内容文本
    public TMP_Text dialogueText;
    // 选项按钮数组，预先在场景挂载好按钮
    public Button[] choiceButtons;

    // 当前正在播放的对话SO数据
    private DialogueSO currentDialogue;
    // 当前对话行索引
    private int dialogueIndex;
    // 对话是否正在运行
    public bool isDialogueActive;

    private void Awake()
    {
        // 单例初始化，防止多个管理器实例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始状态隐藏对话UI，关闭交互与射线
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // 隐藏全部选项按钮
        foreach (var button in choiceButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    // 开启一段新对话
    public void StartDialogue(DialogueSO dialogueSO)
    {
        ClearChoiceButtons();
        currentDialogue = dialogueSO;
        dialogueIndex = 0;
        isDialogueActive = true;
        ShowDialogue();
    }

    // 渲染当前索引的一行对话内容到UI
    private void ShowDialogue()
    {
        if (currentDialogue != null && dialogueIndex < currentDialogue.dialogueLines.Count)
        {
            DialogueLine line = currentDialogue.dialogueLines[dialogueIndex];
            // 设置头像、角色名、对话文本
            portrait.sprite = line.actor.portrait;
            actorName.text = line.actor.actorName;
            dialogueText.text = line.text;

            // 显示对话面板，开启交互
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // 索引自增，为下一次翻页做准备
            dialogueIndex++;
        }
    }

    // 推进对话（点击继续按钮调用）
    // 如果还有台词就继续显示台词；台词读完则弹出选择分支
    public void AdvanceDialogue()
    {
        if (currentDialogue != null && dialogueIndex < currentDialogue.dialogueLines.Count)
        {
            ShowDialogue();
        }
        else
        {
            ShowChoices();
        }
    }

    // 结束对话，重置全部状态，隐藏UI
    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueIndex = 0;
        currentDialogue = null;
        ClearChoiceButtons();

        // 关闭对话面板，关闭交互与射线
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // 显示对话选项按钮；没有选项则显示默认结束按钮
    private void ShowChoices()
    {
        ClearChoiceButtons();
        if (currentDialogue != null && currentDialogue.dialogueOptions.Count > 0)
        {
            for (int i = 0; i < currentDialogue.dialogueOptions.Count; i++)
            {
                var option = currentDialogue.dialogueOptions[i];
                // 修复闭包捕获循环变量bug，临时拷贝局部变量
                DialogueSO nextDialogueRef = option.nextDialogue;

                choiceButtons[i].GetComponentInChildren<TMP_Text>().text = option.optionText;
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].onClick.AddListener(() => ChooseOption(nextDialogueRef));
            }
        }
        else
        {
            // 没有分支选项，显示结束按钮
            choiceButtons[0].gameObject.SetActive(true);
            choiceButtons[0].GetComponentInChildren<TMP_Text>().text = "End";
            choiceButtons[0].onClick.AddListener(EndDialogue);
        }
    }

    // 选择某个选项之后执行
    // dialogueSO：跳转的下一段对话SO，null直接结束对话
    private void ChooseOption(DialogueSO dialogueSO)
    {
        //Debug.Log($"ChooseOption被调用，nextSO = {(dialogueSO != null ? dialogueSO.name : "NULL")}");

        ClearChoiceButtons();
        if (dialogueSO == null)
        {
            EndDialogue();
        }
        else
        {
            StartDialogue(dialogueSO);
        }
    }

    // 清理所有选项按钮：隐藏物体+移除全部监听事件，防止多次注册造成重复调用
    private void ClearChoiceButtons()
    {
        foreach (var button in choiceButtons)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
        }
    }
}
