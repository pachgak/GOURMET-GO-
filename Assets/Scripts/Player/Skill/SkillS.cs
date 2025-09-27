using System.Collections.Generic;
using UnityEngine;

//// Attribute ���з����������ҧ ScriptableObject �ҡ���� Assets ��
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

    // ��������ʹ��ѡ���ж١���¡��
    // ��ͧ��˹������ abstract ���ͺѧ�Ѻ�������١��ͧ��¹�Ѻ
    public abstract Coroutine Use(GameObject player, Vector3 mousePosition);

    //public void EndSkilling()
    //{
    //    Debug.Log($"EndSkilling");
    //    PlayerSkillController.instance.DoSkillEnd();
    //}
}