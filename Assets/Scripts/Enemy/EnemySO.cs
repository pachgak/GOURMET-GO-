using System.Collections.Generic;
using UnityEngine;
using static ItemDropRage;

//// Attribute นี้จะทำให้เราสร้าง ScriptableObject จากเมนู Assets ได้
[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemys/BaseData")] 
public class EnemySO : ScriptableObject
{
    [Header("Base Skill")]
    public Sprite skillIcon;
    public string skillName;
    [field: SerializeField]
    [field: TextArea]
    public string Description { get; set; }
    public float hp;
    public List<ItemDropCount> drop;
}
