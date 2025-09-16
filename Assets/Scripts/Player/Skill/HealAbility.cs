using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Skill", menuName = "Skills/AbilitySkill/Heal Skill")]
public class HealAbility : AbilitySkill
{
    [Header("Heal Skill")]
    public int healAmount;

    // ��¹�Ѻ���ʹ Use() ����Ѻʡ�Ź��
    public override void Use(GameObject player, Vector3 _mousePosition)
    {
        // ���� PlayerHealth component �ҡ����Ф�
        //PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        //if (playerHealth != null)
        //{
           //playerHealth.Heal(healAmount);
            Debug.Log($"healed for {healAmount} HP.");
        //}
    }
}