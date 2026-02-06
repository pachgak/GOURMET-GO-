using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SkillActionTest // ไม่สืบทอดจาก ScriptableObject
{
    // บังคับให้ลูกๆ ต้องมีฟังก์ชันนี้
    public abstract void Execute(GameObject user, GameObject target , Vector3 diraction);
}

[System.Serializable]
public class DashAction : SkillActionTest
{
    public float dashForce = 10f; // ปรับค่า Default ได้
    public float duration = 0.5f;

    public override void Execute(GameObject user, GameObject target, Vector3 diraction)
    {
        Debug.Log($"Dash ด้วยความแรง {dashForce} at diraction : {diraction}");
        // ใส่ Logic Dash ตรงนี้
    }
}

[System.Serializable]
public class SpawnHitboxAction : SkillActionTest
{
    public GameObject prefab;
    public float damage = 50;

    public override void Execute(GameObject user, GameObject target, Vector3 diraction)
    {
        Debug.Log($"เสก Hitbox ดาเมจ {damage} at diraction : {diraction} | add EventActtionHitbox1 = event1 | add EventActtionHitbox2 = event2");
        // ใส่ Logic เสกของตรงนี้
    }
}

//[System.Serializable]
//public class SpawnHitboxAction2 : SkillActionTest
//{
//    public GameObject prefab;
//    public float damage = 50;

//    // *** ทีเด็ดอยู่ตรงนี้ ***
//    // เราใส่ Action ซ้อนเข้าไปใน Action ได้!
//    // นี่คือสิ่งที่ Hitbox จะทำเมื่อชนโดนเป้าหมาย
//    [SerializeReference, SubclassSelector]
//    public List<SkillActionTest> onHitActions;

//    public override void Execute(GameObject user, GameObject target, Vector3 direction) // ตัด Event ออก
//    {
//        // 1. เสก Hitbox
//        GameObject hitboxObj = Object.Instantiate(prefab, user.transform.position, Quaternion.identity);

//        // 2. ยัดไส้ Logic "onHitActions" ใส่เข้าไปในตัว HitboxScript
//        var hitboxScript = hitboxObj.GetComponent<>();
//        if (hitboxScript != null)
//        {
//            // ส่ง "รายการ Action" ไปให้ Hitbox ถือไว้
//            // พอ Hitbox ชนโดนศัตรู -> Hitbox จะเป็นคนสั่งรัน onHitActions เอง
//            hitboxScript.Initialize(user, damage, onHitActions);
//        }
//    }
//}