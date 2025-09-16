using System.Collections.Generic;
using UnityEngine;

//// Attribute ���з����������ҧ ScriptableObject �ҡ���� Assets ��
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

    // ��������ʹ��ѡ���ж١���¡��
    // ��ͧ��˹������ abstract ���ͺѧ�Ѻ�������١��ͧ��¹�Ѻ
    //public abstract void Use(GameObject player, Vector3 mousePosition);

    [Header("Abilities")]
    // �� List �ͧ AbilitySkill �����纤�������ö����� ���ҧ�ʡ������
    public List<AbilitySkill> abilities = new List<AbilitySkill>();

    // ���ʹ Use() �ͧ���� Skill �з�˹�ҷ��ǹ�ٻ���¡�����ʹ Use() �ͧ�ء��������ö���ʵ�
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