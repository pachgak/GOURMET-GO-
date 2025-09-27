// ใน PlayerCombatController.cs
using UnityEngine;
using System;
using System.Linq;

public class PlayerCombatController : MonoBehaviour
{
    public static PlayerCombatController instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // ตั้งค่า Prefab ของการโจมตีใน Inspector
    public AttackComboSet[] attackCombo;
    public float attackFart = 1f;
    public float attackHight = 1f;
    public float comboWindow = 0.5f;
    public LayerMask enemyLayer;

    [Header("Attack Forward")]
    public float attackForwardSpeed = 5f; // ความเร็วในการพุ่งไปข้างหน้าเมื่อโจมตี
    public float attackForwardTime = 0.1f; // ระยะเวลาในการพุ่งไปข้างหน้า

    [Header("Sprinte Attack")]
    public AttackComboSet sprinteAttact;
    public AttackComboSet sonicAttact;
    public Vector2 snapDirectionAttact;
    // เพิ่มขนาดของ OverlapBox
    public Vector3 overlapBoxHalfExtents; // <-- เพิ่มตัวแปรนี้เพื่อกำหนดขนาดกล่อง
    public float overlapBoxFart;
    [Header("Jun System")]
    public float resetAttackClick = 0.3f;

    [Header("System")]
    [SerializeField] private int _attackIndex = 0;
    [SerializeField] private float _lastAttackTime;

    // ตัวแปรสำหรับเก็บข้อมูลที่จะใช้ใน OnDrawGizmos
    private Vector3 _attackDirection;
    private Vector3 _mousePosition;

    // ตัวแปรใหม่สำหรับจัดการ Cooldown
    [SerializeField] private bool _attackClick = false;
    private float _attackCooldownTimer;
    private float _resetAttackClickTimer;
    [SerializeField] private bool _canAttack = true;
    [SerializeField] private bool _isComboing = false;

    [SerializeField] private bool _isDashing = false;
    [SerializeField] private bool _isSprinte = false;
    [SerializeField] private bool _isSonic = false;
    [SerializeField] private bool _isSkilling = false;

    private bool _isUiOpening = false;


    [Header("Action")]
    public Action<Vector3, float, float> OnAttackForward;
    public Action<bool> OnAttackStateChange;
    public Action<bool> OnComboingStateChange;

    [System.Serializable]
    public class AttackComboSet
    {
        public GameObject attackPrefabs;
        public float comboCooldown;
        public float damage;
        public float knockbackForce;
    }

    private void OnEnable()
    {
        //PlayerInputActionsManager.instance.OnMeleeAttack += HandleMeleeAttack;
        //PlayerInputActionsManager.instance.OnMountPosition += HandleGetMountPos;

        //OpenUiManager.instance.OnUiOpeningStateChange += HandleUiOpeningStateChange;

        //// ใช้ Coroutine เพื่อรอ PlayerMovement
        ////StartCoroutine(WaitForMovementInstance());
        //PachonTool.WaitForInstance(() => PlayerMovement.instance,
        //        (playerMovement) =>
        //        {
        //            playerMovement.OnDashStateChange += HandleDashStateChange;
        //            playerMovement.OnSprinteStateChange += HandleSprinteStateChange;
        //            playerMovement.OnSonicStateChange += HandleSonicStateChange;
        //        }
        //);

        ////StartCoroutine(WaitForPlayerSkillControllerInstance());
        //PachonTool.WaitForInstance(() => PlayerSkillController.instance,
        //        (playerSkillController) =>
        //        {
        //            playerSkillController.OnSkillingStateChange += HandleSkillingStateChange;
        //        }
        //);
    }

    private void OnDisable()
    {
        //PlayerInputActionsManager.instance.OnMeleeAttack -= HandleMeleeAttack;
        //PlayerInputActionsManager.instance.OnMountPosition -= HandleGetMountPos;

        //OpenUiManager.instance.OnUiOpeningStateChange -= HandleUiOpeningStateChange;

        //if (PlayerMovement.instance != null) PlayerMovement.instance.OnDashStateChange -= HandleDashStateChange;
        //if (PlayerMovement.instance != null) PlayerMovement.instance.OnSprinteStateChange -= HandleSprinteStateChange;
        //if (PlayerMovement.instance != null) PlayerMovement.instance.OnSonicStateChange -= HandleSonicStateChange;

        //if (PlayerSkillController.instance != null) PlayerSkillController.instance.OnSkillingStateChange -= HandleSkillingStateChange;
    }

    private System.Collections.IEnumerator WaitForMovementInstance()
    {
        // รอจนกว่า PlayerMovement.instance จะไม่เป็น null
        while (PlayerMovement.instance == null)
        {
            yield return null;
        }
        // เมื่อ instance พร้อมแล้ว จึงทำการสมัครรับฟัง
        PlayerMovement.instance.OnDashStateChange += HandleDashStateChange;
        PlayerMovement.instance.OnSprinteStateChange += HandleSprinteStateChange;
        PlayerMovement.instance.OnSonicStateChange += HandleSonicStateChange;
    }

    private System.Collections.IEnumerator WaitForPlayerSkillControllerInstance()
    {
        // รอจนกว่า PlayerCombatController.instance จะไม่เป็น null
        while (PlayerSkillController.instance == null)
        {
            yield return null;
        }
        // เมื่อ instance พร้อมแล้ว จึงทำการสมัครรับฟัง
        PlayerSkillController.instance.OnSkillingStateChange += HandleSkillingStateChange;
    }

    internal void HandleUiOpeningStateChange(bool isUiOpeningState)
    {
        _isUiOpening = isUiOpeningState;
    }

    internal void HandleSonicStateChange(bool isSonicState)
    {
        _isSonic = isSonicState;
    }

    internal void HandleSprinteStateChange(bool isSprinteState)
    {
        _isSprinte = isSprinteState;
    }

    internal void HandleSkillingStateChange(bool isState, float skillLifeTime)
    {
        _isSkilling = isState;
    }

    internal void HandleDashStateChange(bool isDashingState, Vector3 actionDirection)
    {
        _isDashing = isDashingState;

        if (_isDashing)
        {
            _attackIndex = 0; // รีเซ็ตคอมโบ
            _attackCooldownTimer = 0; // รีเซ็ต Cooldown
            _canAttack = false; // ตั้งค่าให้โจมตีได้ทันที
            OnAttackStateChange?.Invoke(_canAttack);

            _isComboing = false;
            OnComboingStateChange?.Invoke(_isComboing);
        }
        else
        {
            _canAttack = true;
        }
    }

    internal void HandleGetMountPos(Vector3 mousePosition)
    {
        _mousePosition = mousePosition;

    }

    [SerializeField] private GameObject _nearestEnemyCollider;

    // สร้าง Prefab การโจมตี
    public void HandleMeleeAttack()
    {
        if (_isUiOpening) return;
        if (_isSkilling) return;

        // คำนวณทิศทางจากผู้เล่นไปยังเมาส์
        Vector3 directionToMouse = (_mousePosition - transform.position).normalized;
        // คำนวณตำแหน่งกึ่งกลางของกล่อง OverlapBox
        Vector3 overlapCenter = transform.position + directionToMouse * (overlapBoxFart);

        // ใช้ OverlapBox ในการตรวจจับศัตรู
        Collider[] hitColliders = Physics.OverlapBox(overlapCenter, overlapBoxHalfExtents, Quaternion.LookRotation(directionToMouse), enemyLayer);

        // ถ้าเจอศัตรู
        if (hitColliders.Length > 0)
        {
            // หาตัวที่ใกล้ที่สุด
            Collider nearestEnemyCollider = hitColliders.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).FirstOrDefault();

            if (nearestEnemyCollider != null)
            {
                _nearestEnemyCollider = nearestEnemyCollider.gameObject;
            }
        }
        else
        {
            // ถ้าไม่เจอศัตรู ให้เป็น null
            _nearestEnemyCollider = null;
        }

        //ShowPointClicker(_mousePosition);
        _attackClick = true;
        _resetAttackClickTimer = resetAttackClick;

        if (!_attackClick) return;

        // ตรวจสอบว่าโจมตีได้หรือไม่
        if (!_canAttack || _isDashing) return;

        _attackClick = false;

        if (attackCombo.Length != 0)
        {
            // คำนวณลำดับการโจมตีปัจจุบัน
            int currentAttackIndex = _attackIndex % attackCombo.Length;

            // ส่งสัญญาณให้ตัวละครพุ่งไปข้างหน้า
            OnAttackForward?.Invoke(directionToMouse, attackForwardSpeed, attackForwardTime);

            // สร้าง GameObject ของการโจมตี
            InstallAttackHit(attackCombo[currentAttackIndex].attackPrefabs, directionToMouse, attackCombo[currentAttackIndex].damage, attackCombo[currentAttackIndex].knockbackForce);

            // อัปเดตสถานะสำหรับคอมโบถัดไป
            _attackIndex++;
            _lastAttackTime = Time.time;

            if (!_isComboing)
            {
                _isComboing = true;
                OnComboingStateChange?.Invoke(_isComboing);
            }
            _canAttack = false;
            // ส่งสัญญาณว่าโจมตีไม่ได้แล้ว
            OnAttackStateChange?.Invoke(_canAttack);
            // ตั้งค่า Cooldown ใหม่
            _attackCooldownTimer = attackCombo[currentAttackIndex].comboCooldown;
        }


    }
    private void Update()
    {
        if (_attackClick && _canAttack) HandleMeleeAttack();
        if (_attackClick)
        {
            _resetAttackClickTimer -= Time.deltaTime;
            if (_resetAttackClickTimer <= 0)
            {
                _attackClick = false;
            }
        }

        // ตรวจสอบว่าคอมโบขาดตอนหรือไม่
        if (Time.time - _lastAttackTime > comboWindow && _attackIndex != 0)
        {
            _attackIndex = 0;

            _isComboing = false;
            OnComboingStateChange?.Invoke(_isComboing);
        }

        // นับเวลาถอยหลังสำหรับ Cooldown
        if (!_canAttack)
        {
            _attackCooldownTimer -= Time.deltaTime;
            if (_attackCooldownTimer <= 0)
            {
                _canAttack = true;
                // ส่งสัญญาณว่าโจมตีได้แล้ว
                OnAttackStateChange?.Invoke(_canAttack);
            }
        }
    }

    private void InstallAttackHit(GameObject attackPrefabs, Vector3 directionToMouse, float damage, float knockbackForce)
    {
        // สร้าง GameObject ของการโจมตี
        GameObject attackInstance = Instantiate(attackPrefabs, transform);

        // คำนวณทิศทางการโจมตี
        //Vector3 playerPosition = transform.position;
        // Vector3 directionToMouse = (_mousePosition - playerPosition).normalized;
        attackInstance.transform.position = transform.position + (directionToMouse * attackFart);

        // คำนวณการหมุน (Rotation)
        Vector3 targetVecter = _mousePosition - transform.position;
        targetVecter.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(targetVecter);
        attackInstance.transform.rotation = targetRotation;

        if (attackInstance.TryGetComponent(out IHurtBox iHurtBox))
        {
            iHurtBox._damage = damage;
            iHurtBox._knockbackDirection = directionToMouse;
            iHurtBox._knockbackForce = knockbackForce;
        }
    }

    public GameObject showPoitPrefab;
    private GameObject showPoitLast;


    private void ShowPointClicker(Vector3 point)
    {
        if (showPoitLast != null) Destroy(showPoitLast);
        showPoitLast = Instantiate(showPoitPrefab, point, Quaternion.identity);
    }

    // ใช้เพื่อแสดงข้อมูลใน Scene View เท่านั้น
    private void OnDrawGizmos()
    {
        // วาดจุดสีแดงที่ตำแหน่งเมาส์
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_mousePosition, 0.2f);

        // วาดเส้นสีเขียวจากตัวละครไปยังตำแหน่งเมาส์
        Gizmos.color = Color.green;
        Vector3 playerPosition = transform.position;
        Vector3 directionToMouse = (_mousePosition - playerPosition).normalized;

        // Gizmos.DrawRay(startPoint, direction)
        Gizmos.DrawRay(playerPosition, directionToMouse * 2f);

        // --- โค้ดสำหรับแสดงศัตรูที่ใกล้ที่สุด ---
        if (_nearestEnemyCollider != null)
        {
            // คำนวณตำแหน่งที่ต้องการวาด (บนหัวศัตรู)
            Vector3 headPosition = _nearestEnemyCollider.transform.position + Vector3.up * 1.5f; // + Vector3.up * 1.5f คือการยกขึ้นไป 1.5 หน่วย

            // วาดเส้นจากผู้เล่นไปยังศัตรูที่ใกล้ที่สุด
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, headPosition);

            // วาดไอคอนเป็นทรงกลมบนหัวศัตรู
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(headPosition + Vector3.up, 0.3f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // วาด OverlapBox สำหรับการตรวจจับศัตรู
        Gizmos.color = Color.yellow;
        Vector3 overlapCenter = transform.position + Vector3.forward * (overlapBoxFart);
        Gizmos.DrawWireCube(overlapCenter, overlapBoxHalfExtents * 2);
    }
}