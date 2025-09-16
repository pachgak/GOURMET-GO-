using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Skill", menuName = "Skills/Skill/Heal Skill")]
public class HealSkill : SkillS
{
    [Space(20)]
    [Header("===== Modify Skill ================================================================")]

    [Header("Heal Skill")]
    public int healAmount;

    // เขียนทับเมธอด Use() สำหรับสกิลนี้
    public override Coroutine Use(GameObject player, Vector3 _mousePosition)
    {
        // ค้นหา PlayerHealth component จากตัวละคร
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