using UnityEngine;
using System.Collections;
using System;

public class LatchController : MonoBehaviour
{
    [Header("Latch Settings")]
    public float requiredHitsToShake = 5f; // เปลี่ยนเป็น float เพื่อความเนียน
    public float damagePerSecond = 5f;
    public Vector3 latchOffset = new Vector3(0, 0.5f, 0.5f); // ตำแหน่งหน้าผู้เล่น

    // *** เพิ่มตรงนี้: ตั้งค่าให้หลอดลดลงกี่หน่วยต่อวินาที ***
    [Header("Difficulty Settings")]
    public float decayPerSecond = 1.5f; // ถ้าตั้ง 1.5 หมายความว่า 1 วิ หลอดจะลดลงเท่ากับการกด 1.5 ครั้ง

    [Header("Shake Off Knockback Settings")]
    public float shakeOffSpeed = 10f;
    public float shakeOffDuration = 0.5f;
    [Range(0, 180)]
    public float shakeOffAngleRange = 60f;

    private bool _isLatched = false;
    private GameObject _targetPlayer;
    private PlayerCombatController _playerCombat;
    private PlayerSkill _playerSkill;
    private EnemyHealth _health;

    // *** เปลี่ยนเป็น float ***
    private float _currentHitCount = 0f;

    public event Action<bool> OnLatchStateChanged;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (!_isLatched) return;

        // 1. ถ้าไก่ตายตอนเกาะ ให้หลุดทันที
        if (_health.isDead)
        {
            Detach();
            return;
        }

        // 2. *** ทำให้หลอดลดลงตามเวลา (Decay) ***
        if (_currentHitCount > 0)
        {
            _currentHitCount -= decayPerSecond * Time.deltaTime; // ลดค่าตามเวลาจริง
            _currentHitCount = Mathf.Max(0, _currentHitCount);   // ห้ามติดลบ

            // อัปเดต UI ให้เห็นว่าหลอดกำลังลดลง
            if (LatchUIManager.instance != null)
            {
                LatchUIManager.instance.UpdateProgress(_currentHitCount);
            }
        }
    }

    public void StartLatch(GameObject player)
    {
        if (_isLatched || _health.isDead) return;

        _targetPlayer = player;
        _playerCombat = player.GetComponent<PlayerCombatController>();
        _playerSkill = player.GetComponent<PlayerSkill>();

        if (_playerCombat == null) return;

        _isLatched = true;
        _currentHitCount = 0f; // รีเซ็ตเป็น 0f

        OnLatchStateChanged?.Invoke(true);

        // 1. ล็อคผู้เล่น
        _playerCombat.isLatched = true;
        if (_playerSkill != null) _playerSkill.isLatched = true;

        // 2. สมัครรับ Event
        _playerCombat.OnShakeInput += HandlePlayerShake;

        // 3. ย้ายไก่ไปเกาะที่หัว
        transform.SetParent(player.transform);
        transform.localPosition = latchOffset;

        if (TryGetComponent(out BaseEnemyCombat combat))
        {
            combat.FinishSkillAnimation();
        }
        if (TryGetComponent(out BaseEnemyMovement enemyMovement))
        {
            enemyMovement.StopDashImmediately();
        }

        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        GetComponent<BaseEnemyAI>().enabled = false;
        GetComponent<BaseEnemyCombat>().enabled = false;

        // 4. เปิด UI Progress Bar
        if (LatchUIManager.instance != null)
        {
            LatchUIManager.instance.ShowLatchUI(requiredHitsToShake, _targetPlayer.transform);
        }

        // 5. เริ่มทำดาเมจ
        StartCoroutine(DamageRoutine()); 
    }

    private void HandlePlayerShake()
    {
        // ทุกครั้งที่คลิก เพิ่มไป 1 ค่า
        _currentHitCount += 1f;

        if (LatchUIManager.instance != null)
        {
            LatchUIManager.instance.UpdateProgress(_currentHitCount);
        }

        if (_currentHitCount >= requiredHitsToShake)
        {
            Detach();
        }
    }

    public void Detach()
    {
        if (!_isLatched) return;
        _isLatched = false;

        OnLatchStateChanged?.Invoke(false);

        if (_playerCombat != null)
        {
            _playerCombat.isLatched = false;
            _playerCombat.OnShakeInput -= HandlePlayerShake;
        }
        if (_playerSkill != null) _playerSkill.isLatched = false;

        if (LatchUIManager.instance != null)
        {
            LatchUIManager.instance.HideLatchUI();
        }

        transform.SetParent(null);
        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
        GetComponent<BaseEnemyAI>().enabled = true;
        GetComponent<BaseEnemyCombat>().enabled = true;

        if (TryGetComponent(out BaseEnemyMovement movement))
        {
            Vector3 backwardDir = -_targetPlayer.transform.forward;
            backwardDir.y = 0;
            backwardDir.Normalize();

            float randomAngle = UnityEngine.Random.Range(-shakeOffAngleRange / 2f, shakeOffAngleRange / 2f);
            Vector3 randomKnockbackDir = Quaternion.Euler(0, randomAngle, 0) * backwardDir;

            movement.SkillDash(randomKnockbackDir, shakeOffSpeed, shakeOffDuration, true);
        }

        StopAllCoroutines();
    }

    IEnumerator DamageRoutine()
    {
        while (_isLatched)
        {
            yield return new WaitForSeconds(1.0f);

            if (_targetPlayer.TryGetComponent(out ITakeDamage hp))
            {
                hp.TakeDamage(damagePerSecond);
                CameraShakeManager.instance.ShakePlayerTakeDamage();
            }
        }
    }
}