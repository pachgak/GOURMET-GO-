using System.Collections.Generic;
using UnityEngine;
using static ItemDropRageOld;

//// Attribute นี้จะทำให้เราสร้าง ScriptableObject จากเมนู Assets ได้
[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemys/BaseData")] 
public class EnemySO : ScriptableObject
{
    [Header("Base Skill")]
    public Sprite enemyIcon;
    public string enemyName;
    [field: SerializeField]
    [field: TextArea]
    public string Description { get; set; }
    public float hp;
    public List<ItemDropFormat> drop;
}
