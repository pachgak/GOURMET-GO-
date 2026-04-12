using System;
using UnityEngine;

// --- 1. คลาสแม่สุดของ Player Action ---
[Serializable]
public abstract class PlayerSkillAction
{
    // รับค่า Player, ตำแหน่งเมาส์ และ ตัวคูณดาเมจ
    public abstract void Execute(GameObject player, Vector3 mousePosition, float damageMultiplier);
}

// --- 2. Action: ฟื้นฟูเลือด (แบบ HealSkill เดิม) ---
[Serializable]
public class HealPlayerAction : PlayerSkillAction
{
    public float healAmount = 20f;

    public override void Execute(GameObject player, Vector3 mousePosition, float damageMultiplier)
    {
        if (player.TryGetComponent(out PlayerHealth health))
        {
            health.addHp(healAmount);
            Debug.Log($"[Skill] Healed player for {healAmount} HP.");
        }
    }
}

// --- 3. Action: โจมตี/เสกของ (แบบ AttacksSkill เดิม) ---
[Serializable]
public class SpawnAttackPlayerAction : PlayerSkillAction
{
    public AttacksSkill.SpawnSkillPrefabsType spawnType;
    public GameObject skillPrefab;

    [Header("Stats")]
    public float damage = 10f;
    public float knockbackForce = 5f;
    public float knockbackTime = 0.2f;
    public float speed = 0f; // สำหรับ Projectile

    [Header("Offset")]
    public float skillFar = 0f;

    public override void Execute(GameObject player, Vector3 mousePosition, float damageMultiplier)
    {
        if (skillPrefab == null) return;

        GameObject attackInstance = null;
        Vector3 directionToMouse = (mousePosition - player.transform.position).normalized;
        Vector3 posInstance = Vector3.zero;
        Vector3 targetVecter = Vector3.zero;

        // --- Logic การ Spawn เหมือนระบบเดิมของคุณ ---
        switch (spawnType)
        {
            case AttacksSkill.SpawnSkillPrefabsType.PlayerParent:
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefab);
                attackInstance.transform.parent = player.transform;
                posInstance = player.transform.position + (directionToMouse * skillFar);
                targetVecter = directionToMouse;
                break;

            case AttacksSkill.SpawnSkillPrefabsType.PlayerWorld:
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefab);
                attackInstance.transform.position = player.transform.position;
                posInstance = player.transform.position + (directionToMouse * skillFar);
                targetVecter = directionToMouse;
                break;

            case AttacksSkill.SpawnSkillPrefabsType.MouseWorld:
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefab);
                attackInstance.transform.position = mousePosition;

                float skillFarTrue = (skillFar >= 0) ? Mathf.Clamp(Vector3.Distance(mousePosition, player.transform.position), 0, skillFar) : Vector3.Distance(mousePosition, player.transform.position);
                posInstance = player.transform.position + (directionToMouse * skillFarTrue);
                targetVecter = mousePosition - player.transform.position;
                break;
        }

        attackInstance.transform.position = posInstance;
        targetVecter.y = 0f;
        if (targetVecter != Vector3.zero) attackInstance.transform.rotation = Quaternion.LookRotation(targetVecter);

        // --- ตั้งค่า Hitbox ---
        if (attackInstance.TryGetComponent(out IHitBox iHurtBox))
        {
            LayerMask enemyLayer = LayerMask.GetMask("Enemy");
            float finalDamage = damage * damageMultiplier;

            iHurtBox.SetUpHitBox(enemyLayer, player, finalDamage, directionToMouse, knockbackForce, knockbackTime);
            iHurtBox.PerformAttack();
        }

        if (attackInstance.TryGetComponent(out ISpeed iSpeed)) iSpeed._speed = speed;
    }
}

// --- 4. Action: พุ่งตัว (Dash) ---
[Serializable]
public class DashPlayerAction : PlayerSkillAction
{
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;

    public override void Execute(GameObject player, Vector3 mousePosition, float damageMultiplier)
    {
        if (player.TryGetComponent(out PlayerMovement movement))
        {
            Vector3 direction = (mousePosition - player.transform.position).normalized;
            direction.y = 0;
            // เรียกใช้ Event Dash เดิมของคุณ
            movement.OnSkillDash?.Invoke(direction, dashSpeed, dashTime, null);
        }
    }
}