using System.Collections.Generic;
using UnityEngine;

//// Attribute นี้จะทำให้เราสร้าง ScriptableObject จากเมนู Assets ได้
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/AbilitySkill/_Generic Skill")]
public class Skill : ScriptableObject
{
    [Header("Base Skill")]
    public string skillName;
    public Sprite skillIcon;
    public float cooldown;
    public int usesCount;
    [Header("LifeTime Skill")]
    public int skillLifeTime;

    // นี่คือเมธอดหลักที่จะถูกเรียกใช้
    // ต้องกำหนดให้เป็น abstract เพื่อบังคับให้คลาสลูกต้องเขียนทับ
    //public abstract void Use(GameObject player, Vector3 mousePosition);

    [Header("Abilities")]
    // ใช้ List ของ AbilitySkill เพื่อเก็บความสามารถหลายๆ อย่างในสกิลเดียว
    public List<AbilitySkill> abilities = new List<AbilitySkill>();

    // เมธอด Use() ของคลาส Skill จะทำหน้าที่วนลูปเรียกใช้เมธอด Use() ของทุกความสามารถในลิสต์
    public void Use(GameObject player, Vector3 mousePosition)
    {
        foreach (var ability in abilities)
        {
            if (ability != null)
            {
                ability.Use(player, mousePosition);
            }
        }
    }
}