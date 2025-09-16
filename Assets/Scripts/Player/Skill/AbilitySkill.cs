using UnityEngine;

// ให้ AbilitySkill เป็น ScriptableObject เพื่อให้เราสามารถสร้าง Asset ได้
public abstract class AbilitySkill : ScriptableObject
{
    // เมธอดนี้จะถูกเรียกใช้เมื่อสกิลทำงาน
    // แต่ละความสามารถจะมีการทำงานที่แตกต่างกัน
    public abstract void Use(GameObject player, Vector3 mousePosition);
}