using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Buff System/Buff Data")]
public class BuffSO : ScriptableObject
{
    [field: SerializeField]
    [Tooltip("ไอดีสำหรับเซฟเกม ห้ามซ้ำกัน (เช่น buff_regen_hp)")]
    public string ID { get; private set; }

    [Header("UI & Info")]
    public string buffName; // สำคัญมาก เอาไว้เช็คบัพซ้ำ!
    [TextArea] public string description;
    public Sprite icon;

    [Header("Duration & Stack")]
    public float duration = 240f; // เช่น 4 นาที = 240 วิ
    public bool isStackable = false;
    public int maxStacks = 1;

    [Header("Tick Settings")]
    public bool hasTickEffect = false;
    public float tickInterval = 1f;

    [Header("Effects (Logic)")]
    [SerializeReference, SubclassSelector]
    public List<BuffEffect> effects = new List<BuffEffect>();

    // *** แก้ฟังก์ชันนี้ใน BuffSO.cs ***
    public string GetEffectsDescription()
    {
        if (effects == null || effects.Count == 0) return "";

        StringBuilder sb = new StringBuilder();
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                // แก้ตรงนี้: ส่ง 'this' (ตัว BuffSO) เข้าไปให้ Effect ดึงข้อมูลได้!
                string effectDesc = effect.GetDescription(this);

                if (!string.IsNullOrEmpty(effectDesc))
                {
                    sb.AppendLine();
                    sb.Append("   " + effectDesc);
                }
            }
        }
        return sb.ToString();
    }

}