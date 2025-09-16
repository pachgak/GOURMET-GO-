using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Skill", menuName = "Skills/Skill/Heal Skill")]
public class HealSkill : SkillS
{
    [Space(20)]
    [Header("===== Modify Skill ================================================================")]

    [Header("Heal Skill")]
    public int healAmount;

    // ��¹�Ѻ���ʹ Use() ����Ѻʡ�Ź��
    public override Coroutine Use(GameObject player, Vector3 _mousePosition)
    {
        // ���� PlayerHealth component �ҡ����Ф�
        //PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        //if (playerHealth != null)
        //{
        //playerHealth.Heal(healAmount);
        Debug.Log($"healed for {healAmount} HP.");
        //}
        EndSkilling();

        return null;
    }

}