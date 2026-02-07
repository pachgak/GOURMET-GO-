using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SkillAction // ไม่สืบทอดจาก ScriptableObject
{
    // บังคับให้ลูกๆ ต้องมีฟังก์ชันนี้
    public abstract void Execute(GameObject user, GameObject target, Vector3 diractionSkill , float speedMultiplier = 1.0f);
}

[System.Serializable]
public class DashAction : SkillAction
{
    public float dashForce = 10f; // ปรับค่า Default ได้
    public float duration = 0.5f;

    public override void Execute(GameObject user, GameObject target, Vector3 diractionSkill, float speedMultiplier = 1.0f)
    {
        Debug.Log($"Dash ด้วยความแรง {dashForce} at diraction : {diractionSkill}");
        // ใส่ Logic Dash ตรงนี้
    }
}

[System.Serializable]
public class SpawnHitboxAction : SkillAction
{
    public GameObject prefab;
    public float damage = 50;

    public override void Execute(GameObject user, GameObject target, Vector3 diractionSkill, float speedMultiplier = 1.0f)
    {
        Debug.Log($"เสก Hitbox ดาเมจ {damage} at diraction : {diractionSkill} | add EventActtionHitbox1 = event1 | add EventActtionHitbox2 = event2");
        // ใส่ Logic เสกของตรงนี้
    }
}