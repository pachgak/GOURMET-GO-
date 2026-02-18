using System.Collections.Generic;
using UnityEngine;
using static SpawnHitboxAction;

[System.Serializable]
public abstract class SkillAction // ไม่สืบทอดจาก ScriptableObject
{
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;
    // บังคับให้ลูกๆ ต้องมีฟังก์ชันนี้
    public abstract void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null);
}

public enum DirMethod
{
    LoockTarget,  // หันตาม player
    SkillDiraction   // หันตาม Dir
}

[System.Serializable]
public class DashAction : SkillAction
{
    [Header("Dash Settings")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f , LayerMask? layerTarget = null)
    {
        // ต้องมี BaseEnemyMovement ถึงจะ Dash ได้
        if (user.TryGetComponent(out BaseEnemyMovement enemyMovement))
        {

            Vector3 finalDirection = Vector3.forward;
            switch (dirType)
            {
                case DirMethod.SkillDiraction:
                    finalDirection = directionSkill;
                    break;

                case DirMethod.LoockTarget:
                    finalDirection = (target.transform.position - user.transform.position).normalized;
                    break;
            }

            // คำนวณทิศทาง: ถ้ามี Target ให้พุ่งหา Target, ถ้าไม่มีให้พุ่งตามทิศที่ส่งมา (หรือพุ่งไปข้างหน้า)
                    
            //if (target != null)
            //{
            //    finalDirection = (target.transform.position - user.transform.position).normalized;
            //}

            // *** สูตรคำนวณ Dash เมื่อความเร็วเปลี่ยน (จากโค้ดเก่า) ***
            // - ความเร็วต้อง "คูณ" speedMultiplier (ไวขึ้น)
            // - เวลาต้อง "หาร" speedMultiplier (จบไวขึ้น)
            float adjustedSpeed = dashSpeed * speedMultiplier;
            float adjustedTime = dashDuration / speedMultiplier;

            // สั่ง Dash
            enemyMovement.SkillDash(finalDirection, adjustedSpeed, adjustedTime);
        }
        else
        {
            Debug.LogWarning($"{user.name} ไม่มี BaseEnemyMovement Component เลย Dash ไม่ได้!");
        }
    }
}

public enum SpawnMethod
{
    ParentToOwner,  // เกิดแล้วเป็นลูกของคนยิง (เช่น ดาบที่ถือในมือ)
    SpawnAtOwner,   // เกิดที่จุดคนยิง แต่เป็นอิสระ (เช่น ยิงลูกบอลไฟ)
    SpawnAtTarget   // เกิดที่จุดเป้าหมาย (เช่น เสกสายฟ้าลงหัว)
}

[System.Serializable]
public class SpawnHitboxAction : SkillAction
{

    [Header("Prefab & Settings")]
    public GameObject prefab;
    public SpawnMethod spawnType = SpawnMethod.ParentToOwner;

    [Header("Offset & Position")]
    public Vector2 offset = Vector2.zero; // x = ระยะห่าง (Far), y = ความสูง (Height)

    [Header("Combat Stats")]
    public float damage = 1;
    public float knockbackForce = 5f;

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f , LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        // เตรียมตัวแปร
        GameObject attackInstance = null;
        Vector3 spawnPos = Vector3.zero;

        Vector3 targetVector = Vector3.forward; ; // ทิศทางที่จะหันหน้าไป
        switch (dirType)
        {
            case DirMethod.SkillDiraction:
                targetVector = directionSkill;
                break;

            case DirMethod.LoockTarget:
                targetVector = (target.transform.position - user.transform.position).normalized;
                break;
        }

        Vector3 heightOffset = new Vector3(0, offset.y, 0);

        // คำนวณตำแหน่งตาม SpawnMethod (Logic เดิมจาก InstallAttackHit)
        switch (spawnType)
        {
            case SpawnMethod.ParentToOwner:
                // เกิดเป็นลูกของ User
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;

            case SpawnMethod.SpawnAtOwner:
                // เกิดที่ User แต่อิสระ
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform.position);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;

            case SpawnMethod.SpawnAtTarget:
                // เกิดที่ Target (MouseWorld เดิม)
                Vector3 targetPos = (target != null) ? target.transform.position : (user.transform.position + targetVector * 5f);
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, targetPos);

                // Logic เดิม: Clamp ระยะทางไม่ให้เกิน offset.x (ถ้ามี)
                float dist = Vector3.Distance(targetPos, user.transform.position);
                float skillFarTrue = (offset.x >= 0) ? Mathf.Clamp(dist, 0, offset.x) : dist;

                spawnPos = user.transform.position + (targetVector * skillFarTrue);
                break;
        }

        // 1. ตั้งตำแหน่ง
        attackInstance.transform.position = spawnPos + heightOffset;

        // 2. ตั้งการหมุน (Rotation)
        targetVector.y = 0f;
        if (targetVector != Vector3.zero)
        {
            attackInstance.transform.rotation = Quaternion.LookRotation(targetVector);
        }

        // 3. Setup IHitBox (ตั้งค่า Damage, Owner)
        if (attackInstance.TryGetComponent(out IHitBox hitBox))
        {
            // เป้าหมายของ Enemy คือ Player
            LayerMask finalLayer = (layerTarget.HasValue && layerTarget.Value.value != 0) ? layerTarget.Value : LayerMask.GetMask("Player");
            hitBox._targetLayer = finalLayer;
            hitBox._ownerHit = user;
            hitBox._damage = damage;
            hitBox._knockbackDirection = targetVector;
            hitBox._knockbackForce = knockbackForce;

            hitBox.PerformAttack();
        }
    }
}

[System.Serializable]
public class SpawnprojectileHitboxAction : SkillAction
{

    [Header("Prefab & Settings")]
    public GameObject prefab;
    public SpawnMethod spawnType = SpawnMethod.ParentToOwner;

    [Header("Offset & Position")]
    public Vector2 offset = Vector2.zero; // x = ระยะห่าง (Far), y = ความสูง (Height)

    [Header("Combat Stats")]
    public float damage = 1;
    public float knockbackForce = 5f;
    public float projectileSpeed = 0f; // สำหรับ IProjectile / ISpeed

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f , LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        // เตรียมตัวแปร
        GameObject attackInstance = null;
        Vector3 spawnPos = Vector3.zero;

        Vector3 targetVector = Vector3.forward; ; // ทิศทางที่จะหันหน้าไป
        switch (dirType)
        {
            case DirMethod.SkillDiraction:
                targetVector = directionSkill;
                break;

            case DirMethod.LoockTarget:
                targetVector = (target.transform.position - user.transform.position).normalized;
                break;
        }

        Vector3 heightOffset = new Vector3(0, offset.y, 0);

        // คำนวณตำแหน่งตาม SpawnMethod (Logic เดิมจาก InstallAttackHit)
        switch (spawnType)
        {
            case SpawnMethod.ParentToOwner:
                // เกิดเป็นลูกของ User
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;

            case SpawnMethod.SpawnAtOwner:
                // เกิดที่ User แต่อิสระ
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform.position);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;

            case SpawnMethod.SpawnAtTarget:
                // เกิดที่ Target (MouseWorld เดิม)
                Vector3 targetPos = (target != null) ? target.transform.position : (user.transform.position + targetVector * 5f);
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, targetPos);

                // Logic เดิม: Clamp ระยะทางไม่ให้เกิน offset.x (ถ้ามี)
                float dist = Vector3.Distance(targetPos, user.transform.position);
                float skillFarTrue = (offset.x >= 0) ? Mathf.Clamp(dist, 0, offset.x) : dist;

                spawnPos = user.transform.position + (targetVector * skillFarTrue);
                break;
        }

        // 1. ตั้งตำแหน่ง
        attackInstance.transform.position = spawnPos + heightOffset;

        // 2. ตั้งการหมุน (Rotation)
        targetVector.y = 0f;
        if (targetVector != Vector3.zero)
        {
            attackInstance.transform.rotation = Quaternion.LookRotation(targetVector);
        }

        // 3. Setup IHitBox (ตั้งค่า Damage, Owner)
        if (attackInstance.TryGetComponent(out IHitBox hitBox))
        {
            // เป้าหมายของ Enemy คือ Player
            LayerMask finalLayer = (layerTarget.HasValue && layerTarget.Value.value != 0) ? layerTarget.Value : LayerMask.GetMask("Player");
            hitBox._targetLayer = finalLayer;
            hitBox._ownerHit = user;
            hitBox._damage = damage;
            hitBox._knockbackDirection = directionSkill;
            hitBox._knockbackForce = knockbackForce;

            hitBox.PerformAttack();
        }

        // 4. Setup ISpeed (ความเร็วกระสุน)
        if (attackInstance.TryGetComponent(out ISpeed iSpeed))
        {
            // คูณ speedMultiplier หรือไม่ ขึ้นอยู่กับดีไซน์ (ถ้าอยากให้กระสุนไวขึ้นด้วยก็คูณ)
            iSpeed._speed = projectileSpeed;
            // iSpeed._speed = projectileSpeed * speedMultiplier; // แบบคูณ
        }
    }
}

[System.Serializable]
public class SpawnVFXAction : SkillAction
{

    [Header("Prefab & Settings")]
    public GameObject prefab;
    public SpawnMethod spawnType = SpawnMethod.ParentToOwner;

    [Header("Offset & Position")]
    public Vector2 offset = Vector2.zero; // x = ระยะห่าง (Far), y = ความสูง (Height)

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f , LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        // เตรียมตัวแปร
        GameObject attackInstance = null;
        Vector3 spawnPos = Vector3.zero;

        Vector3 targetVector = Vector3.forward; ; // ทิศทางที่จะหันหน้าไป
        switch (dirType)
        {
            case DirMethod.SkillDiraction:
                targetVector = directionSkill;
                break;

            case DirMethod.LoockTarget:
                targetVector = (target.transform.position - user.transform.position).normalized;
                break;
        }

        Vector3 heightOffset = new Vector3(0, offset.y, 0);

        // คำนวณตำแหน่งตาม SpawnMethod (Logic เดิมจาก InstallAttackHit)
        switch (spawnType)
        {
            case SpawnMethod.ParentToOwner:
                // เกิดเป็นลูกของ User
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;

            case SpawnMethod.SpawnAtOwner:
                // เกิดที่ User แต่อิสระ
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform.position);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;

            case SpawnMethod.SpawnAtTarget:
                // เกิดที่ Target (MouseWorld เดิม)
                Vector3 targetPos = (target != null) ? target.transform.position : (user.transform.position + targetVector * 5f);
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, targetPos);

                // Logic เดิม: Clamp ระยะทางไม่ให้เกิน offset.x (ถ้ามี)
                float dist = Vector3.Distance(targetPos, user.transform.position);
                float skillFarTrue = (offset.x >= 0) ? Mathf.Clamp(dist, 0, offset.x) : dist;

                spawnPos = user.transform.position + (targetVector * skillFarTrue);
                break;
        }

        // 1. ตั้งตำแหน่ง
        attackInstance.transform.position = spawnPos + heightOffset;

        // 2. ตั้งการหมุน (Rotation)
        targetVector.y = 0f;
        if (targetVector != Vector3.zero)
        {
            attackInstance.transform.rotation = Quaternion.LookRotation(targetVector);
        }
        Debug.Log($"Spawned {prefab.name} via Action");
    }
}

[System.Serializable]
public class SpawnWallHitStunAction : SkillAction
{
    [Header("Wall Hit Logic")]
    public LayerMask wallLayer; // เลเยอร์กำแพงที่จะทำให้มึน

    [Header("Prefab & Settings")]
    public GameObject prefab; // ใส่ PersonHitbox หรือ Hitbox ธรรมดาได้เลย
    public SpawnMethod spawnType = SpawnMethod.ParentToOwner;
    public Vector2 offset = Vector2.zero; // x = ระยะห่าง, y = ความสูง

    [Header("Combat Stats")]
    public float damage = 10f;
    public float knockbackForce = 10f;

    // ใช้ LayerMask? เพื่อรองรับค่า Default เป็น null
    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        // 1. จัดการ Layer: ถ้าไม่ส่งมา (null) ให้ใช้ Layer "Player" เป็นค่าเริ่มต้น
        LayerMask finalLayer = (layerTarget.HasValue && layerTarget.Value.value != 0) ? layerTarget.Value : LayerMask.GetMask("Player");

        // 2. คำนวณทิศทาง
        Vector3 targetVector = Vector3.forward;
        switch (dirType)
        {
            case DirMethod.SkillDiraction:
                targetVector = directionSkill;
                break;
            case DirMethod.LoockTarget:
                if (target != null)
                    targetVector = (target.transform.position - user.transform.position).normalized;
                break;
        }

        // 3. Spawn Logic
        GameObject attackInstance = null;
        Vector3 spawnPos = Vector3.zero;

        switch (spawnType)
        {
            case SpawnMethod.ParentToOwner:
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;
            default: // SpawnAtOwner หรือ SpawnAtTarget
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform.position);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;
        }

        // Set Position & Rotation
        attackInstance.transform.position = spawnPos + new Vector3(0, offset.y, 0);

        targetVector.y = 0f;
        if (targetVector != Vector3.zero)
            attackInstance.transform.rotation = Quaternion.LookRotation(targetVector);

        // --- 4. LOGIC INJECTION (ส่วนสำคัญ) ---
        if (attackInstance.TryGetComponent(out IHitBox hitBox))
        {
            // Setup ค่าปกติให้ Hitbox
            hitBox._targetLayer = finalLayer;
            hitBox._ownerHit = user;
            hitBox._damage = damage;
            hitBox._knockbackDirection = directionSkill;
            hitBox._knockbackForce = knockbackForce;

            // *** ฝัง Logic: เคลียร์ Event เก่า -> ใส่ Logic ใหม่ ***

            // ล้าง Event เก่าทิ้ง (สำคัญมากสำหรับ Object Pooling เพื่อไม่ให้ Logic ซ้อนทับ)
            //hitBox._OnAttackHit = null;

            // Subscribe Event ใหม่: เมื่อชนอะไรก็ตาม ให้เรียก CheckHitLogic
            hitBox._OnAttackHit += (colliders) =>
            {
                CheckHitLogic(user, attackInstance, colliders);
                ObjectPoolingManager.Instance.Respawn(attackInstance);
            };

            // สั่งเริ่มทำงาน
            hitBox.PerformAttack();
        }
    }

    // แยก Logic การเช็คชนออกมา
    private void CheckHitLogic(GameObject user, GameObject hitboxObj, Collider[] hits)
    {
        foreach (var col in hits)
        {
            // เช็คว่า ชนกำแพง หรือไม่? (ใช้ Bitwise Check กับ LayerMask)
            if (((1 << col.gameObject.layer) & wallLayer) != 0)
            {
                Debug.Log("SpawnWallHitStunAction: Hit Wall!");

                // ถ้า User มี HogCombat ให้สั่งหยุดและมึน
                if (user.TryGetComponent(out HogCombat hogCombat))
                {
                    hogCombat.OnHitWall();
                }

                return; // จบงานทันที
            }
        }
    }
}