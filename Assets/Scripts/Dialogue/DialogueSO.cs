using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 对话配置数据，ScriptableObject资源，在编辑器创建配置
[CreateAssetMenu(fileName = "DialogueSO", menuName = "Dialog/Dialogue")]
public class DialogueSO : ScriptableObject
{
    // 本段对话的台词行列表
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    // 本段对话结束后的分支选项列表
    public List<DialogueLine.DialogueOption> dialogueOptions = new List<DialogueLine.DialogueOption>();
}

// 单条对话行数据：说话人、台词文本
[System.Serializable]
public class DialogueLine
{
    // 说话角色数据（头像、角色名存在ActorSO）
    public ActorSO actor;

    // 对话文本内容，编辑器多行输入
    [TextArea(3, 10)] public string text;

    // 对话选项结构体：选项文字，跳转下一段对话SO
    [System.Serializable]
    public class DialogueOption
    {
        // 选项按钮显示文字
        public string optionText;
        // 点击该选项跳转的下一段对话，填null代表结束对话
        public DialogueSO nextDialogue;
    }
}
