using System;
using System.Collections.Generic;
using UnityEngine;

//// Attribute นี้จะทำให้เราสร้าง ScriptableObject จากเมนู Assets ได้
//[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Generic Skill")]
public abstract class PlayerSkillSO : ScriptableObject
{
    [Header("Base Skill")]
    public Sprite skillIcon;
    public string skillName;
    [field: SerializeField]
    [field: TextArea]
    public string Description { get; set; }
    public float cooldown;
    //public int usesCount;

    [Space(20)]
    [Header("===== Modify Skill ================================================================")]
    [Header("LifeTime Skill")]
    public float skillLifeTime;

    // นี่คือเมธอดหลักที่จะถูกเรียกใช้
    // ต้องกำหนดให้เป็น abstract เพื่อบังคับให้คลาสลูกต้องเขียนทับ
    // เพิ่ม float damageMultiplier = 1f
    public abstract Coroutine Use(GameObject player, Vector3 mousePosition, float damageMultiplier = 1f);

    //public void EndSkilling()
    //{
    //    Debug.Log($"EndSkilling");
    //    PlayerSkillController.instance.DoSkillEnd();
    //}
    public enum AttackType
    {
        nope,V,O,X
    }
}