using System.Collections.Generic;
using UnityEngine;

// 1. คลาสสำหรับหน้าคุยปกติ (ไม่มีเงื่อนไข)
[System.Serializable]
public class QuestDialogueStep
{
    [TextArea(2, 5)]
    public string questText;

    [Tooltip("ใช้ [CL] เพื่อแสดงเลขหน้า เช่น (1/3)")]
    public string actionText = "กด E ต่อ [CL]";
}

// 2. คลาสสำหรับหน้าส่งเควส (มีเงื่อนไข)
[System.Serializable]
public class QuestTurnInStep
{
    [Tooltip("ใช้ [R0], [R1] เพื่อแทรกตัวเลขเงื่อนไข")]
    [TextArea(2, 5)]
    public string questText;

    public string actionText = "กด E เพื่อส่ง"; // ตั้งค่าเริ่มต้นไว้เลย

    [SerializeReference, SubclassSelector]
    public List<QuestRequirement> requirements = new List<QuestRequirement>();
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class QuestSO : ScriptableObject
{
    public string questID;
    public string questName;

    [Header("1. Dialogue (บทสนทนาช่วงแรก)")]
    public List<QuestDialogueStep> dialogues = new List<QuestDialogueStep>();

    [Header("2. Objective (หน้าส่งเควสและเงื่อนไข)")]
    public QuestTurnInStep turnInStep;

    [Header("3. Rewards (รางวัล)")]
    [SerializeReference, SubclassSelector]
    public List<QuestRewardBase> rewards = new List<QuestRewardBase>();
}