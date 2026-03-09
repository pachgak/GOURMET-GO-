using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // ต้องใช้ Linq เพื่อช่วยเรียงลำดับข้อมูล

public class MockSkillRunner : MonoBehaviour
{
    [System.Serializable]
    public class SkillTime
    {
        [Header("Debug Info")]
        public string description = "Skill 1 Config"; // ใส่ไว้กันลืมว่าคือสกิลอะไร

        [Header("Settings")]
        [Tooltip("เวลาทั้งหมดของท่านั้นๆ (จบ Animation)")]
        public float simulatedDurationSkill = 2.0f;

        [Tooltip("ลำดับเวลาที่จะสั่ง Execute Action (Index 0 คือ Action ตัวแรก, Index 1 คือ Action ตัวสอง...)")]
        public List<float> runAtTimeSkill;
    }

    [Header("Mock Config")]
    public List<SkillTime> setMockSkill;

    private BaseEnemyCombat _combat;
    private Coroutine _mockCoroutine;

    private void Awake()
    {
        _combat = GetComponent<BaseEnemyCombat>();

        if (_combat == null)
        {
            Debug.LogError("MockSkillRunner ต้องอยู่กับ BaseEnemyCombat!");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_combat != null)
        {
            _combat.OnSkillUesd += HandleSkillUsed;
        }
    }

    private void OnDisable()
    {
        if (_combat != null)
        {
            _combat.OnSkillUesd -= HandleSkillUsed;
        }
    }

    // เมื่อ Combat สั่งใช้สกิล
    private void HandleSkillUsed(int skillIndex, float speedMultiplier)
    {
        // เช็คว่าเราตั้งค่า Mock ไว้ครบไหม
        if (setMockSkill == null || skillIndex >= setMockSkill.Count)
        {
            Debug.LogWarning($"MockSkillRunner: ไม่มี Config สำหรับ Skill Index {skillIndex} ใช้ Default 1 วินาที");
            StartCoroutine(DefaultFallbackRoutine());
            return;
        }

        if (_mockCoroutine != null) StopCoroutine(_mockCoroutine);
        _mockCoroutine = StartCoroutine(RunMockSkillRoutine(skillIndex, setMockSkill[skillIndex], speedMultiplier));
    }

    private IEnumerator RunMockSkillRoutine(int index, SkillTime config, float speedMultiplier)
    {
        // Debug.Log($"[Mock] Start Skill {index} | Duration: {config.simulatedDurationSkill}");

        // 1. เตรียมข้อมูล Action: จับคู่ (เวลา, Index) แล้วเรียงตามเวลาจากน้อยไปมาก
        // เราต้องทำแบบนี้เพราะ WaitForSeconds มันต้องรอก่อน-หลัง ตามลำดับ
        var actionTimeline = new List<ActionTimePair>();

        for (int i = 0; i < config.runAtTimeSkill.Count; i++)
        {
            actionTimeline.Add(new ActionTimePair
            {
                triggerTime = config.runAtTimeSkill[i],
                actionIndex = i
            });
        }

        // เรียงลำดับตามเวลา (น้อย -> มาก)
        actionTimeline = actionTimeline.OrderBy(x => x.triggerTime).ToList();

        // 2. เริ่มรันตาม Timeline
        float currentTime = 0f;

        foreach (var actionPair in actionTimeline)
        {
            // คำนวณเวลาที่ต้องรอก่อนจะถึงคิว Action นี้
            // สูตร: เวลาเป้าหมาย - เวลาปัจจุบัน = เวลาที่ต้องรอเพิ่ม
            float waitDuration = (actionPair.triggerTime - currentTime) / speedMultiplier; // หาร speedMultiplier เพื่อเร่งความเร็ว

            if (waitDuration > 0)
            {
                yield return new WaitForSeconds(waitDuration);
                currentTime += waitDuration * speedMultiplier;
            }

            // ถึงเวลาแล้ว! สั่ง Action ทำงาน
            // Debug.Log($"[Mock] Trigger Action {actionPair.actionIndex} at {currentTime}s");
            _combat.ExecuteSkillAction(actionPair.actionIndex);
        }

        // 3. รอเวลาที่เหลือจนจบท่า (ถ้ามี)
        float remainingTime = (config.simulatedDurationSkill - currentTime) / speedMultiplier;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // 4. จบสกิล
        _combat.FinishSkillAnimation();
        // Debug.Log($"[Mock] Finish Skill {index}");
        _mockCoroutine = null;
    }

    // เผื่อลืมตั้งค่า ให้รันแบบโง่ๆ ไปก่อน
    private IEnumerator DefaultFallbackRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        _combat.FinishSkillAnimation();
    }

    // Class ช่วยเก็บข้อมูลสำหรับเรียงลำดับ
    private struct ActionTimePair
    {
        public float triggerTime;
        public int actionIndex;
    }
}