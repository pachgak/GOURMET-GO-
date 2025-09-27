using System.Collections.Generic;
using UnityEngine;

//// Attribute นี้จะทำให้เราสร้าง ScriptableObject จากเมนู Assets ได้
//[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Generic Skill")]
public abstract class SkillS : ScriptableObject
{
    [Header("Base Skill")]
    public string skillName;
    public Sprite skillIcon;
    public float cooldown;
    public int usesCount;

    [Space(20)]
    [Header("===== Modify Skill ================================================================")]
    [Header("LifeTime Skill")]
    public float skillLifeTime;

    // นี่คือเมธอดหลักที่จะถูกเรียกใช้
    // ต้องกำหนดให้เป็น abstract เพื่อบังคับให้คลาสลูกต้องเขียนทับ
    public abstract Coroutine Use(GameObject player, Vector3 mousePosition);

    //public void EndSkilling()
    //{
    //    Debug.Log($"EndSkilling");
    //    PlayerSkillController.instance.DoSkillEnd();
    //}
}