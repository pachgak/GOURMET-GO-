using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SkillAction
{
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;

    public abstract void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null);

    // --- Helper Method 1: คำนวณทิศทาง ---
    protected Vector3 CalculateTargetVector(GameObject user, GameObject target, Vector3 directionSkill)
    {
        Vector3 targetVector = Vector3.forward;
        switch (dirType)
        {
            case DirMethod.SkillDiraction:
                targetVector = directionSkill;
                break;
            case DirMethod.LoockTarget:
                if (target != null)
                    targetVector = (target.transform.position - user.transform.position).normalized;
                // ถ้าไม่มี Target ก็ใช้ directionSkill หรือ forward ไปก่อน
                else
                    targetVector = user.transform.forward;
                break;
        }
        return targetVector;
    }

    // --- Helper Method 2: Spawn และคำนวณตำแหน่ง ---
    protected void SpawnAndCalculatePosition(GameObject prefab, GameObject user, GameObject target,
                                             SpawnMethod spawnType, Vector2 offset, Vector3 targetVector,
                                             out GameObject attackInstance, out Vector3 spawnPos)
    {
        attackInstance = null;
        spawnPos = Vector3.zero;

        if (prefab == null) return;

        switch (spawnType)
        {
            case SpawnMethod.ParentToOwner:
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;

            case SpawnMethod.SpawnAtOwner:
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, user.transform.position);
                spawnPos = user.transform.position + (targetVector * offset.x);
                break;

            case SpawnMethod.SpawnAtTarget:
                Vector3 targetPos = (target != null) ? target.transform.position : (user.transform.position + targetVector * 5f);
                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, targetPos);

                float dist = Vector3.Distance(targetPos, user.transform.position);
                float skillFarTrue = (offset.x >= 0) ? Mathf.Clamp(dist, 0, offset.x) : dist;

                spawnPos = user.transform.position + (targetVector * skillFarTrue);
                break;
        }
    }
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

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        // ต้องมี BaseEnemyMovement ถึงจะ Dash ได้
        if (user.TryGetComponent(out BaseEnemyMovement enemyMovement))
        {

            Vector3 targetVector = CalculateTargetVector(user, target, directionSkill);

            // *** สูตรคำนวณ Dash เมื่อความเร็วเปลี่ยน (จากโค้ดเก่า) ***
            // - ความเร็วต้อง "คูณ" speedMultiplier (ไวขึ้น)
            // - เวลาต้อง "หาร" speedMultiplier (จบไวขึ้น)
            float adjustedSpeed = dashSpeed * speedMultiplier;
            float adjustedTime = dashDuration / speedMultiplier;

            // สั่ง Dash
            enemyMovement.SkillDash(targetVector, adjustedSpeed, adjustedTime);
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
public class SpawnHitAction_M : SkillAction
{
    [Header("Prefab & Settings")]
    public GameObject prefab;
    public SpawnMethod spawnType = SpawnMethod.ParentToOwner;

    [Header("Offset & Position")]
    public Vector2 offset = Vector2.zero;

    [Header("Hitbox Stats (Optional)")]
    // ค่าพวกนี้จะถูกใช้ก็ต่อเมื่อ Prefab มี IHitBox
    public float damage = 1;
    public float knockbackForce = 5f;

    [Header("Modifiers")]
    // *** นี่คือหัวใจสำคัญ: ใส่ Modifier ได้ไม่อั้น ***
    [SerializeReference, SubclassSelector] // สำคัญ! เพื่อให้เลือก Subclass ใน Inspector ได้
    public List<SpawnModifier> modifiers = new List<SpawnModifier>();

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        // 1. คำนวณทิศทาง (ใช้ Helper)
        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill);

        // 2. Spawn และหาตำแหน่ง (ใช้ Helper)
        SpawnAndCalculatePosition(prefab, user, target, spawnType, offset, targetVector,
                                  out GameObject attackInstance, out Vector3 spawnPos);

        if (attackInstance == null) return;

        // 3. Set Position & Rotation
        attackInstance.transform.position = spawnPos + new Vector3(0, offset.y, 0);
        targetVector.y = 0f;
        if (targetVector != Vector3.zero)
        {
            attackInstance.transform.rotation = Quaternion.LookRotation(targetVector);
        }

        // 4. Setup Basic Hitbox (ถ้ามี) - ส่วนนี้ยังคงไว้เพราะเป็น Core Feature
        if (attackInstance.TryGetComponent(out IHitBox hitBox))
        {
            LayerMask finalLayer = (layerTarget.HasValue && layerTarget.Value.value != 0) ? layerTarget.Value : LayerMask.GetMask("Player");
            hitBox._targetLayer = finalLayer;
            hitBox._ownerHit = user;
            hitBox._damage = damage;
            hitBox._knockbackDirection = targetVector; // หรือ directionSkill ตาม Logic เดิม
            hitBox._knockbackForce = knockbackForce;

            hitBox.PerformAttack();
        }

        // 5. *** รัน Modifiers ทั้งหมด ***
        foreach (var modifier in modifiers)
        {
            modifier.Apply(user, attackInstance, speedMultiplier);
        }
    }
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

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        // เตรียมตัวแปร
        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill);

        Vector3 heightOffset = new Vector3(0, offset.y, 0);

        // คำนวณตำแหน่งตาม SpawnMethod (Logic เดิมจาก InstallAttackHit)
        // 2. เรียก Helper: Spawn และหาตำแหน่ง (ใช้ out เพื่อรับค่ากลับมา 2 ตัว)
        SpawnAndCalculatePosition(prefab, user, target, spawnType, offset, targetVector,
                                  out GameObject attackInstance, out Vector3 spawnPos);

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

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill);

        Vector3 heightOffset = new Vector3(0, offset.y, 0);

        // คำนวณตำแหน่งตาม SpawnMethod (Logic เดิมจาก InstallAttackHit)
        // 2. เรียก Helper: Spawn และหาตำแหน่ง (ใช้ out เพื่อรับค่ากลับมา 2 ตัว)
        SpawnAndCalculatePosition(prefab, user, target, spawnType, offset, targetVector,
                                  out GameObject attackInstance, out Vector3 spawnPos);

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

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill);

        Vector3 heightOffset = new Vector3(0, offset.y, 0);

        // คำนวณตำแหน่งตาม SpawnMethod (Logic เดิมจาก InstallAttackHit)
        // 2. เรียก Helper: Spawn และหาตำแหน่ง (ใช้ out เพื่อรับค่ากลับมา 2 ตัว)
        SpawnAndCalculatePosition(prefab, user, target, spawnType, offset, targetVector,
                                  out GameObject attackInstance, out Vector3 spawnPos);

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

    public float stunDuration = 3.0f;
    // ใช้ LayerMask? เพื่อรองรับค่า Default เป็น null
    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        // 2. คำนวณทิศทาง
        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill);

        // คำนวณตำแหน่งตาม SpawnMethod (Logic เดิมจาก InstallAttackHit)
        // 2. เรียก Helper: Spawn และหาตำแหน่ง (ใช้ out เพื่อรับค่ากลับมา 2 ตัว)
        SpawnAndCalculatePosition(prefab, user, target, spawnType, offset, targetVector,
                                  out GameObject attackInstance, out Vector3 spawnPos);

        // Set Position & Rotation
        attackInstance.transform.position = spawnPos + new Vector3(0, offset.y, 0);

        targetVector.y = 0f;
        if (targetVector != Vector3.zero)
            attackInstance.transform.rotation = Quaternion.LookRotation(targetVector);

        // --- 4. LOGIC INJECTION (ส่วนสำคัญ) ---
        if (attackInstance.TryGetComponent(out IHitBox hitBox))
        {
            // Setup ค่าปกติให้ Hitbox
            // 1. จัดการ Layer: ถ้าไม่ส่งมา (null) ให้ใช้ Layer "Player" เป็นค่าเริ่มต้น
            LayerMask finalLayer = (layerTarget.HasValue && layerTarget.Value.value != 0) ? layerTarget.Value : LayerMask.GetMask("Player");
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
                CheckHitLogic(user, attackInstance, colliders ,stunDuration);
                ObjectPoolingManager.Instance.Respawn(attackInstance);
            };

            // สั่งเริ่มทำงาน
            hitBox.PerformAttack();
        }
    }

    // แยก Logic การเช็คชนออกมา
    private void CheckHitLogic(GameObject user, GameObject hitboxObj, Collider[] hits, float stunDuration)
    {
        foreach (var col in hits)
        {
            if (((1 << col.gameObject.layer) & wallLayer) != 0)
            {
                Debug.Log("SpawnWallHitStunAction: Hit Wall!");

                // *** เพิ่มตรงนี้: ถ้าชนกำแพงเปราะบาง ให้สั่งมันแตก! ***
                if (col.TryGetComponent(out FragileWall fragileWall))
                {
                    fragileWall.BreakWall();
                }

                //if (user.TryGetComponent(out BaseEnemyCombat combat))
                //{
                //    // คุณอาจจะต้องแคสต์เป็น KumongaCombat หรือถ้า Base มี OnHitWall ก็เรียกได้เลย
                //    if (combat is HogCombat hogCombat) hogCombat.OnHitWall();
                //    if (combat is KumongaCombat kumongaCombat) kumongaCombat.OnHitWall();
                //}

                // 2. เช็คผ่าน Interface เลย! (ไม่ต้องดึง BaseEnemyCombat แล้ว)
                if (user.TryGetComponent(out IWallCollidable wallCollidable))
                {
                    // ไม่ว่า user จะเป็นตัวอะไรก็ตาม ถ้ามันแปะ Interface นี้ไว้ มันจะทำงานทันที
                    wallCollidable.OnHitWall(stunDuration);
                }

                return;
            }
        }
    }
}

[System.Serializable]
public class StunSelfAction : SkillAction
{
    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        // ถ้าเป็น Kumonga ก็เรียก AI ของมัน
        if (user.TryGetComponent(out KumongaAI kumongaAI))
        {
            kumongaAI.ApplyStun();
        }
        // (เผื่ออนาคต) ถ้าเป็นตัวอื่นที่มีระบบ Stun
        else if (user.TryGetComponent(out HogAI hogAI))
        {
            hogAI.ApplyStun();
        }

        Debug.Log($"{user.name} Applied Stun to Self!");
    }
}

[System.Serializable]
public class JumpAction : SkillAction
{
    [Header("Jump Settings")]
    public float jumpHeight = 5f;
    public float jumpDuration = 1.0f; // เวลาที่ลอยอยู่

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (user.TryGetComponent(out BaseEnemyMovement movement))
        {
            // คำนวณทิศทาง (ใช้ Helper ที่เราทำไว้)
            Vector3 targetVector = CalculateTargetVector(user, target, directionSkill);

            // สมมติว่ากระโดดไปข้างหน้านิดนึง หรือกระโดดอยู่กับที่
            // ถ้าอยากให้อยู่กับที่ ก็ใช้ user.transform.position
            Vector3 jumpTargetPos = user.transform.position;

            // สั่งกระโดด (คูณ speedMultiplier ด้วยก็ได้ถ้าต้องการ)
            movement.SkillJump(jumpTargetPos, jumpHeight, jumpDuration / speedMultiplier);
            Debug.Log($"{user.name} Jumped!");
        }
    }
}

[System.Serializable]
public class TeleportAction : SkillAction
{
    [Header("Teleport Settings")]
    public Vector3 offset = Vector3.zero; // ใส่ Y ติดลบเพื่อให้อยู่ใต้ดิน

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (target == null) return;

        // คำนวณตำแหน่งที่จะวาปไป
        Vector3 targetPos = target.transform.position + offset;

        // สั่งวาป (ถ้ามี NavMeshAgent ควรปิดก่อนย้ายตำแหน่ง หรือใช้ Warp)
        if (user.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
        {
            agent.Warp(targetPos);
        }
        else
        {
            user.transform.position = targetPos;
        }

        // หันหน้าหาเป้าหมาย
        user.transform.LookAt(new Vector3(target.transform.position.x, user.transform.position.y, target.transform.position.z));

        Debug.Log($"{user.name} Teleported to {targetPos}");
    }
}