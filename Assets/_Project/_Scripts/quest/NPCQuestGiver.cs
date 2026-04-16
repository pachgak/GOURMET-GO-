using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class NPCQuestGiver : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject dialogueBubble;
    public TMP_Text questText;
    public TMP_Text actionText;

    [Header("Quest Data")]
    public List<QuestSO> mainQuestLine;

    private int currentQuestIndex = 0;
    private int currentStepIndex = 0;
    private GameObject _playerObj;

    private void Start()
    {
        _playerObj = GameObject.FindGameObjectWithTag("Player");
        UpdateDialogueUI();
    }

    public void InteractWithNPC()
    {
        if (currentQuestIndex >= mainQuestLine.Count) return;

        QuestSO currentQuest = mainQuestLine[currentQuestIndex];

        // --- เช็คว่าตอนนี้อยู่ในโหมดไหน? ---
        if (currentStepIndex < currentQuest.dialogues.Count)
        {
            // โหมด 1: กำลังคุยปกติ -> ให้ข้ามไปหน้าถัดไป
            currentStepIndex++;
        }
        else
        {
            // โหมด 2: อยู่หน้าส่งเควส -> ให้เช็คเงื่อนไขและแจกของ
            bool canPass = true;
            foreach (var req in currentQuest.turnInStep.requirements)
            {
                if (!req.IsMet(_playerObj)) canPass = false;
            }

            if (canPass)
            {
                // 1. หักของ
                foreach (var req in currentQuest.turnInStep.requirements) req.ConsumeRequirement(_playerObj);

                // 2. แจกรางวัล
                foreach (var reward in currentQuest.rewards) reward.GiveReward(_playerObj);

                // 3. ไปเควสใหม่ และรีเซ็ตหน้าคุยกลับไปหน้าศูนย์
                currentQuestIndex++;
                currentStepIndex = 0;
            }
            else Debug.Log("ของยังไม่ครบ!");
        }

        UpdateDialogueUI();
    }

    private void UpdateDialogueUI()
    {
        if (currentQuestIndex >= mainQuestLine.Count)
        {
            questText.text = "ไม่มีเควสแล้ว";
            actionText.text = "ปิด";
            return;
        }

        QuestSO currentQuest = mainQuestLine[currentQuestIndex];

        // --- อัปเดต UI ตามโหมดที่อยู่ ---
        if (currentStepIndex < currentQuest.dialogues.Count)
        {
            // ------------------------------------
            // แสดง UI แบบหน้าคุยปกติ
            // ------------------------------------
            QuestDialogueStep step = currentQuest.dialogues[currentStepIndex];

            // เปลี่ยน [CL] ได้ทันทีไม่ต้องวนลูป!
            string finalActionText = step.actionText;
            if (finalActionText.Contains("[CL]"))
            {
                // หน้าปัจจุบัน (index + 1) / จำนวนหน้าพูดคุยทั้งหมด
                finalActionText = finalActionText.Replace("[CL]", $"({currentStepIndex + 1}/{currentQuest.dialogues.Count})");
            }

            actionText.text = finalActionText;
            questText.text = step.questText;
        }
        else
        {
            // ------------------------------------
            // แสดง UI แบบหน้าส่งเควส (มีเงื่อนไข)
            // ------------------------------------
            QuestTurnInStep turnIn = currentQuest.turnInStep;
            actionText.text = turnIn.actionText;

            string finalQuestText = turnIn.questText;
            bool hasUsedTags = false;

            if (turnIn.requirements.Count > 0)
            {
                for (int i = 0; i < turnIn.requirements.Count; i++)
                {
                    string tag = $"[R{i}]";
                    if (finalQuestText.Contains(tag))
                    {
                        string progressText = turnIn.requirements[i].GetProgressText(_playerObj);
                        finalQuestText = finalQuestText.Replace(tag, progressText);
                        hasUsedTags = true;
                    }
                }

                if (!hasUsedTags)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(finalQuestText);
                    sb.AppendLine("\n<color=yellow>เงื่อนไข:</color>");
                    foreach (var req in turnIn.requirements)
                    {
                        sb.AppendLine($"- {req.GetProgressText(_playerObj)}");
                    }
                    finalQuestText = sb.ToString();
                }
            }

            questText.text = finalQuestText;
        }
    }
}