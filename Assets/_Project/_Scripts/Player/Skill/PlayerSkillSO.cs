using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// --- โครงสร้างที่คุณออกแบบมา (เปลี่ยนชื่อให้ดูโปรขึ้นนิดนึง) ---
[Serializable]
public class SkillNewStepTT
{
    [Tooltip("เวลาที่จะให้ Action นี้ทำงาน (นับจาก 0)")]
    public float playAtTime;

    [SerializeReference, SubclassSelector]
    public PlayerSkillAction action;
}

[CreateAssetMenu(fileName = "New Timeline Skill", menuName = "Skills/New Player Skill")]
public class PlayerSkillSO : ScriptableObject
{
    [Header("Base Skill")]
    public Sprite skillIcon;
    public string skillName;
    [TextArea] public string Description;
    public float cooldown;

    [Space(10)]
    [Header("Skill Timeline Settings")]
    [Tooltip("เวลาทั้งหมดของสกิล เพื่อล็อคการเดิน/โจมตี (ต้องมากกว่า Step สุดท้าย)")]
    public float skillLifeTime;

    // *** หัวใจหลักของระบบใหม่ ***
    public List<SkillNewStepTT> skillSteps = new List<SkillNewStepTT>();

    // เมธอดหลักที่ PlayerSkill.cs จะเรียกใช้
    public virtual Coroutine Use(GameObject player, Vector3 mousePosition, float damageMultiplier = 1f)
    {
        // สั่งให้ Player เป็นคนรัน Coroutine
        return player.GetComponent<MonoBehaviour>().StartCoroutine(ExecuteTimeline(player, mousePosition, damageMultiplier));
    }

    private IEnumerator ExecuteTimeline(GameObject player, Vector3 mousePosition, float damageMultiplier)
    {
        float currentTime = 0f;

        // เรียงลำดับ Step ตามเวลา playAtTime (เผื่อคุณเผลอสลับลำดับใน Inspector มันจะได้ไม่บั๊ก)
        var sortedSteps = skillSteps.OrderBy(step => step.playAtTime).ToList();

        foreach (var step in sortedSteps)
        {
            // คำนวณว่าต้องรอกี่วินาทีก่อนจะถึง Step ถัดไป
            float waitTime = step.playAtTime - currentTime;

            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
                currentTime += waitTime; // อัปเดตเวลาปัจจุบัน
            }

            // รัน Action!
            if (step.action != null)
            {
                step.action.Execute(player, mousePosition, damageMultiplier);
            }
        }
    }
}