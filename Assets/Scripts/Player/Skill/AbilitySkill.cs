using UnityEngine;

// ��� AbilitySkill �� ScriptableObject ��������������ö���ҧ Asset ��
public abstract class AbilitySkill : ScriptableObject
{
    // ���ʹ���ж١���¡�������ʡ�ŷӧҹ
    // ���Ф�������ö���ա�÷ӧҹ���ᵡ��ҧ�ѹ
    public abstract void Use(GameObject player, Vector3 mousePosition);
}