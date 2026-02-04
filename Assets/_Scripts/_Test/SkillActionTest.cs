using UnityEngine;

[System.Serializable]
public abstract class SkillActionTest // ไม่สืบทอดจาก ScriptableObject
{
    // บังคับให้ลูกๆ ต้องมีฟังก์ชันนี้
    public abstract void Execute(GameObject user, GameObject target);
}

[System.Serializable]
public class DashAction : SkillActionTest
{
    public float dashForce = 10f; // ปรับค่า Default ได้
    public float duration = 0.5f;

    public override void Execute(GameObject user, GameObject target)
    {
        Debug.Log($"Dash ด้วยความแรง {dashForce}");
        // ใส่ Logic Dash ตรงนี้
    }
}

[System.Serializable]
public class SpawnHitboxAction : SkillActionTest
{
    public GameObject prefab;
    public float damage = 50;

    public override void Execute(GameObject user, GameObject target)
    {
        Debug.Log($"เสก Hitbox ดาเมจ {damage}");
        // ใส่ Logic เสกของตรงนี้
    }
}