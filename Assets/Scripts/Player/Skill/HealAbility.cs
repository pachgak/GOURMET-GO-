using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Skill", menuName = "Skills/AbilitySkill/Heal Skill")]
public class HealAbility : AbilitySkill
{
    [Header("Heal Skill")]
    public int healAmount;

    // เขียนทับเมธอด Use() สำหรับสกิลนี้
    public override void Use(GameObject player, Vector3 _mousePosition)
    {
        // ค้นหา PlayerHealth component จากตัวละคร
        //PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        //if (playerHealth != null)
        //{
           //playerHealth.Heal(healAmount);
            Debug.Log($"healed for {healAmount} HP.");
        //}
    }
}