using UnityEngine;

[CreateAssetMenu(fileName = "New Health Data", menuName = "Item System/Health Data")]
public class HealthSO : ScriptableObject
{
    [Header("Health Settings")]
    [Tooltip("จำนวนเลือดที่ต้องการฟื้นฟู")]
    public float healAmount = 20f;

    // อนาคตถ้าอยากใส่ Effect ตอนกินยา ก็ใส่ตรงนี้ได้ เช่น
    // public GameObject healVFX;
}