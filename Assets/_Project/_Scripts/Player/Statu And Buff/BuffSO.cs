using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Buff System/Buff Data")]
public class BuffSO : ScriptableObject
{
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
}