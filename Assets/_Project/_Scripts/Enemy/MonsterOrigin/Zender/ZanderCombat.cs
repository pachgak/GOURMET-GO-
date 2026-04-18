using System;
using System.Collections;
using UnityEngine;

public class ZanderCombat : BaseEnemyCombat
{
    [System.Serializable]
    public class ThunderStormConfig
    {
        public GameObject hitPrefab;
        [Header("Combat Stats")]
        public float damage = 5f;
        public float knockbackForce = 5f;
    }

    [Header("Overcharge Settings")]
    public bool isOvercharge = false;
    public int lightningHitCount = 0;
    public int hitsToOvercharge = 2;
    public float overchargeDuration = 15f;

    // *** เปลี่ยนจาก GameObject ธรรมดามาเป็น Prefab และเพิ่มตัวแปรควบคุมระยะเวลา/ระยะสุ่ม ***
    [Tooltip("ใส่ Prefab ของสายฟ้าที่จะให้ผ่ารอบๆ ตัวบอส")]
    public GameObject overchargeStrikeVFXPrefab;
    public float strikeOverchargeTime = 5f; // ความถี่ในการสุ่มสายฟ้า (วินาที)
    public float strikeOverchargeRange = 3f;  // ระยะวงกว้างในการสุ่มรอบตัวบอส

    [Header("ThunderStorms (Skill 2) Settings")]
    public ThunderStormConfig thunderStorm = new ThunderStormConfig();
    public float stormRadius = 6f;
    public float stormInterval = 1f;

    //[Header("event")]
    public event Action<bool> OnOverchargeChanged;

    // ฟังก์ชันนี้ถูกเรียกโดย SelfLightningDetector
    public void TakeSelfLightning()
    {
        if (isOvercharge || _enemyHealth.isDead) return;

        lightningHitCount++;
        Debug.Log($"<color=cyan>[Zander] รับประจุไฟฟ้า! {lightningHitCount}/{hitsToOvercharge}</color>");

        if (_aiController != null)
        {
            _aiController.ApplyStun(1.5f);
        }

        if (lightningHitCount >= hitsToOvercharge)
        {
            TriggerOvercharge();
        }
    }

    private void TriggerOvercharge()
    {
        isOvercharge = true;
        lightningHitCount = 0;
        Debug.Log("<color=yellow>[Zander] OVERCHARGE MODE ACTIVATED!!</color>");

        // *** แจ้งเตือนทุกคนว่าเข้าโหมด Overcharge แล้ว! ***
        OnOverchargeChanged?.Invoke(true);

        if (_enemyMovement != null) _enemyMovement.chaseSpeed += 2.5f;

        StartCoroutine(ThunderStormsRoutine());
        StartCoroutine(OverchargeTimer());

        // *** เริ่ม Coroutine สำหรับสุ่มสายฟ้ารอบตัวบอส ***
        StartCoroutine(OverchargeStrikeRoutine());
    }

    // *** Coroutine ใหม่: สำหรับสุ่มเสก VFX สายฟ้ารอบๆ ตัวบอส ***
    private IEnumerator OverchargeStrikeRoutine()
    {
        while (isOvercharge && !_enemyHealth.isDead) // ทำงานวนไปเรื่อยๆ ตราบใดที่ยัง Overcharge และบอสยังไม่ตาย
        {
            if (overchargeStrikeVFXPrefab != null)
            {
                // สุ่มตำแหน่งวงกลมในแนวราบรอบๆ ตัวบอส (transform.position)
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * strikeOverchargeRange;
                Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                // ใช้ ObjectPoolingManager เสก VFX ออกมา
                ObjectPoolingManager.Instance.Spawn(overchargeStrikeVFXPrefab, spawnPos);
            }

            // รอเวลาตามที่คุณกำหนดก่อนจะเสกสายฟ้าเส้นต่อไป
            yield return new WaitForSeconds(strikeOverchargeTime);
        }
    }

    private IEnumerator ThunderStormsRoutine()
    {
        float timer = 0f;
        while (timer < overchargeDuration && !_enemyHealth.isDead)
        {
            if (thunderStorm.hitPrefab != null && _aiController.playerTarget != null)
            {
                // ของ ThunderStorm สุ่มรอบตัว Player Target
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * stormRadius;
                Vector3 spawnPos = _aiController.playerTarget.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                GameObject storm = ObjectPoolingManager.Instance.Spawn(thunderStorm.hitPrefab, spawnPos);

                if (storm.TryGetComponent(out IHitBox hitBox))
                {
                    hitBox._ownerHit = gameObject;
                    hitBox._targetLayer = attackMask;
                    hitBox._damage = thunderStorm.damage;
                    hitBox._knockbackForce = thunderStorm.knockbackForce;
                    hitBox.PerformAttack();
                }
            }
            yield return new WaitForSeconds(stormInterval);
            timer += stormInterval;
        }
    }

    private IEnumerator OverchargeTimer()
    {
        yield return new WaitForSeconds(overchargeDuration);

        isOvercharge = false;

        // *** แจ้งเตือนทุกคนว่าหมดเวลา Overcharge แล้ว! ***
        OnOverchargeChanged?.Invoke(false);

        if (_enemyMovement != null) _enemyMovement.chaseSpeed -= 2.5f;
        Debug.Log("[Zander] พลังงานไฟฟ้าหมดลง กลับสู่โหมดปกติ...");
    }

    protected override IEnumerator AttackLogic()
    {
        if (enemySkills == null || enemySkills.Length == 0) yield break;

        if (!isOvercharge)
        {
            // โหมดปกติ: สลับใช้สกิล 0 กับ 1
            if (_currentSkillIndex > 1) _currentSkillIndex = 0;
            yield return UseSkill(_currentSkillIndex);
            _currentSkillIndex = (_currentSkillIndex == 0) ? 1 : 0;
        }
        else
        {
            // โหมด Overcharge: สลับใช้สกิล 2 , 3, 4
            if (_currentSkillIndex < 2 || _currentSkillIndex > 4) _currentSkillIndex = 2;
            yield return UseSkill(_currentSkillIndex, 1f); // ใช้สกิลไวขึ้น

            _currentSkillIndex++;
            if (_currentSkillIndex > 4) _currentSkillIndex = 2;
        }
    }
}