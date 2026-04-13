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

/// --- 3. Action: โจมตี/เสกของ (แบบ AttacksSkill เดิม) ---
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

    public override void Execute(GameObject player, Vector3 mousePosition, float damageMultiplier)
    {
        if (skillPrefab == null) return;

        GameObject attackInstance = null;
        Vector3 directionToMouse = (mousePosition - player.transform.position).normalized;
        Vector3 posInstance = Vector3.zero;
        Vector3 targetVecter = Vector3.zero;

        // --- Logic การ Spawn ที่คลีนขึ้น (เอา Offset ออก) ---
        switch (spawnType)
        {
            case AttacksSkill.SpawnSkillPrefabsType.PlayerParent:
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefab);
                attackInstance.transform.parent = player.transform;
                posInstance = player.transform.position; // ออกที่ตัว Player
                targetVecter = directionToMouse;
                break;

            case AttacksSkill.SpawnSkillPrefabsType.PlayerWorld:
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefab);
                posInstance = player.transform.position; // ออกที่ตัว Player
                targetVecter = directionToMouse;
                break;

            case AttacksSkill.SpawnSkillPrefabsType.MouseWorld:
                attackInstance = ObjectPoolingManager.Instance.Spawn(skillPrefab);
                posInstance = mousePosition; // ออกที่เมาส์เป๊ะๆ
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

// --- 5. Action: เสกเอฟเฟกต์ (VFX) อย่างเดียว ---
[Serializable]
public class SpawnVFXPlayerAction : PlayerSkillAction
{
    public AttacksSkill.SpawnSkillPrefabsType spawnType;

    [Tooltip("Prefab ของ Effect ที่ต้องการเสก (ไม่ต้องมี Hitbox ก็ได้)")]
    public GameObject vfxPrefab;

    public override void Execute(GameObject player, Vector3 mousePosition, float damageMultiplier)
    {
        if (vfxPrefab == null) return;

        GameObject vfxInstance = null;
        Vector3 directionToMouse = (mousePosition - player.transform.position).normalized;
        Vector3 posInstance = Vector3.zero;
        Vector3 targetVecter = Vector3.zero;

        // --- Logic การ Spawn วางตำแหน่ง (เอา Offset ออก) ---
        switch (spawnType)
        {
            case AttacksSkill.SpawnSkillPrefabsType.PlayerParent:
                // เกิดที่ตัวผู้เล่นและขยับตามผู้เล่น
                vfxInstance = ObjectPoolingManager.Instance.Spawn(vfxPrefab);
                vfxInstance.transform.parent = player.transform;
                posInstance = player.transform.position;
                targetVecter = directionToMouse;
                break;

            case AttacksSkill.SpawnSkillPrefabsType.PlayerWorld:
                // เกิดที่ตัวผู้เล่น แต่ไม่ขยับตาม (ทิ้งไว้ตรงนั้น)
                vfxInstance = ObjectPoolingManager.Instance.Spawn(vfxPrefab);
                posInstance = player.transform.position;
                targetVecter = directionToMouse;
                break;

            case AttacksSkill.SpawnSkillPrefabsType.MouseWorld:
                // เกิดที่ตำแหน่งเมาส์ชี้เป๊ะๆ
                vfxInstance = ObjectPoolingManager.Instance.Spawn(vfxPrefab);
                posInstance = mousePosition;
                targetVecter = mousePosition - player.transform.position;
                break;
        }

        // --- ตั้งค่าตำแหน่ง ---
        vfxInstance.transform.position = posInstance;

        // --- ตั้งค่าการหันหน้า (Rotation) ---
        targetVecter.y = 0f;
        if (targetVecter != Vector3.zero)
        {
            vfxInstance.transform.rotation = Quaternion.LookRotation(targetVecter);
        }

        Debug.Log($"[Skill] Spawned VFX: {vfxPrefab.name}");
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

