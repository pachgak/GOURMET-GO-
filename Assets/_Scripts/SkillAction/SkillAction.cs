using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public abstract class SkillAction
{
    public abstract void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null);

    // --- Helper Method 1: คำนวณทิศทาง ---
    protected Vector3 CalculateTargetVector(GameObject user, GameObject target, Vector3 directionSkill, DirMethod dirType)
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
                else
                    targetVector = user.transform.forward;
                break;
            case DirMethod.Origin:
                targetVector = Vector3.zero;
                break;
        }

        // *** ฝัง Logic ใหม่ตรงนี้: อัปเดตทิศทางล่าสุดกลับไปที่ Combat เสมอ! ***
        if (user.TryGetComponent(out BaseEnemyCombat combat))
        {
            combat.currentDiractionSkill = targetVector;
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

                string targetName = (target != null) ? target.name : "null";
                Debug.Log($"Target : {targetName}");

                attackInstance = ObjectPoolingManager.Instance.Spawn(prefab, targetPos);

                float dist = Vector3.Distance(targetPos, user.transform.position);
                float skillFarTrue = (offset.x >= 0) ? Mathf.Clamp(dist, 0, offset.x) : dist;

                spawnPos = targetPos + (targetVector * skillFarTrue);
                break;
        }
    }
}
public enum DirMethod
{
    LoockTarget,  // หันตาม player
    SkillDiraction,   // หันตาม Dir
    Origin
}

[System.Serializable]
public class DashAction : SkillAction
{
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;

    [Header("Dash Settings")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        // ต้องมี BaseEnemyMovement ถึงจะ Dash ได้
        if (user.TryGetComponent(out BaseEnemyMovement enemyMovement))
        {

            Vector3 targetVector = CalculateTargetVector(user, target, directionSkill,dirType);

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
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;

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
        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill, dirType);

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
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;

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
        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill, dirType);

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
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;

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

        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill, dirType);

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
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;   

    [Header("Prefab & Settings")]
    public GameObject prefab;
    public SpawnMethod spawnType = SpawnMethod.ParentToOwner;

    [Header("Offset & Position")]
    public Vector2 offset = Vector2.zero; // x = ระยะห่าง (Far), y = ความสูง (Height)

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill, dirType);

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
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;

    [Header("Wall Hit Logic")]
    public LayerMask wallLayer; // เลเยอร์กำแพงที่จะทำให้มึน

    [Header("Prefab & Settings")]
    public GameObject prefab; // ใส่ PersonHitbox หรือ Hitbox ธรรมดาได้เลย
    public SpawnMethod spawnType = SpawnMethod.ParentToOwner;
    public Vector2 offset = Vector2.zero; // x = ระยะห่าง, y = ความสูง

    [Header("Combat Stats")]
    public float damage = 10f;
    public float knockbackForce = 10f;

    [Header("Wall Hit Settings")]
    public float stunDuration = 3.0f;

    // *** เพิ่มตัวแปรตั้งค่าการเด้งถอยหลัง ***
    public float bounceForce = 15f;
    public float bounceTime = 0.2f;
    // ใช้ LayerMask? เพื่อรองรับค่า Default เป็น null
    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (prefab == null) return;

        // 2. คำนวณทิศทาง
        Vector3 targetVector = CalculateTargetVector(user, target, directionSkill, dirType);

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
                CheckHitLogic(user, attackInstance, colliders, stunDuration, bounceForce, bounceTime);
                ObjectPoolingManager.Instance.Respawn(attackInstance);
            };

            // สั่งเริ่มทำงาน
            hitBox.PerformAttack();
        }
    }

    // แยก Logic การเช็คชนออกมา
    private void CheckHitLogic(GameObject user, GameObject hitboxObj, Collider[] hits, float stunDuration, float bounceForce, float bounceTime)
    {
        foreach (var col in hits)
        {
            if (((1 << col.gameObject.layer) & wallLayer) != 0)
            {
                if (col.TryGetComponent(out FragileWall fragileWall))
                {
                    fragileWall.BreakWall();
                }

                if (user.TryGetComponent(out IWallCollidable wallCollidable))
                {
                    // *** ส่งค่าต่อไปให้ Combat ***
                    wallCollidable.OnHitWall(stunDuration, bounceForce, bounceTime);
                }
                return;
            }
        }
    }
}

[System.Serializable]
public class StunSelfAction : SkillAction
{
    public float stunDuration = 3f;
    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (user.TryGetComponent(out BaseEnemyAI BaseEnemyAI))
        {
            BaseEnemyAI.ApplyStun(stunDuration);
        }

        Debug.Log($"{user.name} Applied Stun to Self!");
    }
}

[System.Serializable]
public class JumpAction : SkillAction
{
    [Header("Diration Set")]
    public DirMethod dirType = DirMethod.LoockTarget;

    [Header("Jump Settings")]
    public float jumpHeight = 5f;
    public float jumpDuration = 1.0f; // เวลาที่ลอยอยู่

    public bool randomPos = false;
    public float range;

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (user.TryGetComponent(out BaseEnemyMovement movement))
        {
            // Save currentDiractionSkill คำนวณทิศทาง (ใช้ Helper ที่เราทำไว้)
            Vector3 targetVector = CalculateTargetVector(user, target, directionSkill, dirType);
            
            Vector3 jumpCenter = Vector3.zero;  
            Vector3 jumpTargetPos = Vector3.zero;  
            // สมมติว่ากระโดดไปข้างหน้านิดนึง หรือกระโดดอยู่กับที่
            // ถ้าอยากให้อยู่กับที่ ก็ใช้ user.transform.position
            switch (dirType)
            {
                case DirMethod.SkillDiraction:
                    jumpCenter = user.transform.position + directionSkill;
                    break;
                case DirMethod.LoockTarget:
                    if (target != null)
                        jumpCenter = target.transform.position;
                    else
                        jumpCenter = user.transform.position + user.transform.forward;
                    break;
                case DirMethod.Origin:
                    jumpCenter = user.transform.position;
                    break;
            }

            if (randomPos)
            {
                // 1. สุ่มจุดในวงกลมรัศมี 1 หน่วย แล้วคูณด้วย range
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * range;

                // 2. เอาค่าที่สุ่มได้มาบวกกับ jumpCenter โดยใส่ในแกน X และ Z (ให้ Y คงเดิม)
                jumpTargetPos = jumpCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);

                // --- (Pro Tip: แนะนำให้ใส่เพิ่มเพื่อกันกระโดดทะลุกำแพง) ---
                // ใช้ NavMesh.SamplePosition เพื่อดึงจุดที่สุ่มได้ ให้กลับมาอยู่บนพื้นที่เดินได้ (NavMesh)
                if (UnityEngine.AI.NavMesh.SamplePosition(jumpTargetPos, out UnityEngine.AI.NavMeshHit hit, range, UnityEngine.AI.NavMesh.AllAreas))
                {
                    jumpTargetPos = hit.position; // ใช้จุดที่ปลอดภัยบน NavMesh
                }
            }
            else
            {
                jumpTargetPos = jumpCenter;
            }

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

[System.Serializable]
public class SplitSelfAction : SkillAction
{
    [Header("Split Settings")]
    public List<GameObject> minionPrefabs;
    public float spawnRadius = 2.0f;
    public float timeSpawnDelay = 0.2f; // เปลี่ยนชื่อตัวสะกดนิดนึงให้ถูกหลัก
    public bool killUserAfterSplit = true;
    public GameObject spawnVFX;

    public override void Execute(GameObject user, GameObject target, Vector3 directionSkill, float speedMultiplier = 1.0f, LayerMask? layerTarget = null)
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0) return;

        Vector3 centerPos = user.transform.position;

        // 1. เล่น Effect (ถ้ามี)
        if (spawnVFX != null)
        {
            ObjectPoolingManager.Instance.Spawn(spawnVFX, centerPos);
        }

        // 2. สั่งยืมตัว ObjectPoolingManager (ซึ่งไม่ถูก Destroy แน่นอน) ในการรัน Coroutine เสกมอน
        if (ObjectPoolingManager.Instance != null)
        {
            ObjectPoolingManager.Instance.StartCoroutine(SpawnMinion(centerPos, target));
        }
        else
        {
            Debug.LogError("หา ObjectPoolingManager.Instance ไม่เจอ สั่งแยกร่างไม่ได้!");
        }

        // 3. จัดการฆ่าตัวต้น (User) ได้อย่างสบายใจ! เพราะ Coroutine ไม่ได้ฝากไว้ที่ตัวมันแล้ว
        if (killUserAfterSplit)
        {
            if (user.TryGetComponent(out EnemyHealth health))
            {
                health.isRespawnNow = true;
                health.setHp(0);
            }
            else
            {
                user.SetActive(false);
            }
        }
    }

    // แก้ไข: ไม่ต้องรับ GameObject user แล้ว ให้รับแค่ Vector3 ตำแหน่งมาแทน 
    // เพราะถ้าส่ง user มา แล้ว user ถูกปิดไป การอ้างอิงตำแหน่งจะพัง
    private IEnumerator SpawnMinion(Vector3 centerPos, GameObject target)
    {
        yield return new WaitForSeconds(timeSpawnDelay);

        // 1. วนลูปสร้างมอนสเตอร์แต่ละตัว
        for (int i = 0; i < minionPrefabs.Count; i++)
        {
            if (minionPrefabs[i] == null) continue;

            float angle = i * (360f / minionPrefabs.Count);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * spawnRadius;
            Vector3 spawnPos = centerPos + offset;

            // Spawn มอนสเตอร์
            GameObject minion = ObjectPoolingManager.Instance.Spawn(minionPrefabs[i], spawnPos);

            if (target != null)
            {
                minion.transform.LookAt(new Vector3(target.transform.position.x, minion.transform.position.y, target.transform.position.z));
            }

            if (minion.TryGetComponent(out BaseEnemyAI ai))
            {
                ai.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);
            }

            // ถ้ามี Effect ตอนลูกเกิด ก็เล่นที่นี่
            if (spawnVFX != null)
            {
                ObjectPoolingManager.Instance.Spawn(spawnVFX, minion.transform.position);
            }
        }
    }
}