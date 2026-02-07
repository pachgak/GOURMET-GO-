using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    // ลากตัว BearCombat มาใส่ใน Inspector
    public BaseEnemyCombat combatScript;

    private void Awake()
    {
        // ถ้าไม่ลากมา ลองหาจาก Parent ดู
        if (combatScript == null) combatScript = GetComponentInParent<BaseEnemyCombat>();
        if (combatScript == null) Debug.Log($"AnimationEventRelay {this.name} : combatScript == null");
    }

    // 1. Event สำหรับสั่ง Action (Dash, Hitbox)
    // ชื่อฟังก์ชันนี้ต้องตรงกับที่เลือกใน Animation Event
    public void AE_TriggerAction(int index)
    {
        if (combatScript != null) combatScript.ExecuteSkillAction(index);
    }

    // 2. Event สำหรับบอกว่าจบท่าแล้ว
    public void AE_FinishSkill()
    {
        if (combatScript != null) combatScript.FinishSkillAnimation();
        Debug.Log($"0 AE_FinishSkill()");
    }
}