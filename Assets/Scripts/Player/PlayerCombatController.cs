// � PlayerCombatController.cs
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

    // ��駤�� Prefab �ͧ�������� Inspector
    public AttackComboSet[] attackCombo;
    public float attackFart = 1f;
    public float attackHight = 1f;
    public float comboWindow = 0.5f;
    public LayerMask enemyLayer;

    [Header("Attack Forward")]
    public float attackForwardSpeed = 5f; // ��������㹡�þ��仢�ҧ˹�����������
    public float attackForwardTime = 0.1f; // ��������㹡�þ��仢�ҧ˹��

    [Header("Sprinte Attack")]
    public AttackComboSet sprinteAttact;
    public AttackComboSet sonicAttact;
    public Vector2 snapDirectionAttact;
    // ������Ҵ�ͧ OverlapBox
    public Vector3 overlapBoxHalfExtents; // <-- ��������ù�����͡�˹���Ҵ���ͧ
    public float overlapBoxFart;
    [Header("Jun System")]
    public float resetAttackClick = 0.3f;

    [Header("System")]
    [SerializeField] private int _attackIndex = 0;
    [SerializeField] private float _lastAttackTime;

    // ���������Ѻ�红����ŷ������ OnDrawGizmos
    private Vector3 _attackDirection;
    private Vector3 _mousePosition;

    // �������������Ѻ�Ѵ��� Cooldown
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

        //// �� Coroutine ������ PlayerMovement
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
        // �ͨ����� PlayerMovement.instance ������� null
        while (PlayerMovement.instance == null)
        {
            yield return null;
        }
        // ����� instance ��������� �֧�ӡ����Ѥ��Ѻ�ѧ
        PlayerMovement.instance.OnDashStateChange += HandleDashStateChange;
        PlayerMovement.instance.OnSprinteStateChange += HandleSprinteStateChange;
        PlayerMovement.instance.OnSonicStateChange += HandleSonicStateChange;
    }

    private System.Collections.IEnumerator WaitForPlayerSkillControllerInstance()
    {
        // �ͨ����� PlayerCombatController.instance ������� null
        while (PlayerSkillController.instance == null)
        {
            yield return null;
        }
        // ����� instance ��������� �֧�ӡ����Ѥ��Ѻ�ѧ
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
            _attackIndex = 0; // ���絤���
            _attackCooldownTimer = 0; // ���� Cooldown
            _canAttack = false; // ��駤�����������ѹ��
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

    // ���ҧ Prefab �������
    public void HandleMeleeAttack()
    {
        if (_isUiOpening) return;
        if (_isSkilling) return;

        // �ӹǳ��ȷҧ�ҡ��������ѧ�����
        Vector3 directionToMouse = (_mousePosition - transform.position).normalized;
        // �ӹǳ���˹觡�觡�ҧ�ͧ���ͧ OverlapBox
        Vector3 overlapCenter = transform.position + directionToMouse * (overlapBoxFart);

        // �� OverlapBox 㹡�õ�Ǩ�Ѻ�ѵ��
        Collider[] hitColliders = Physics.OverlapBox(overlapCenter, overlapBoxHalfExtents, Quaternion.LookRotation(directionToMouse), enemyLayer);

        // ������ѵ��
        if (hitColliders.Length > 0)
        {
            // �ҵ�Ƿ��������ش
            Collider nearestEnemyCollider = hitColliders.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).FirstOrDefault();

            if (nearestEnemyCollider != null)
            {
                _nearestEnemyCollider = nearestEnemyCollider.gameObject;
            }
        }
        else
        {
            // ���������ѵ�� ����� null
            _nearestEnemyCollider = null;
        }

        //ShowPointClicker(_mousePosition);
        _attackClick = true;
        _resetAttackClickTimer = resetAttackClick;

        if (!_attackClick) return;

        // ��Ǩ�ͺ����������������
        if (!_canAttack || _isDashing) return;

        _attackClick = false;

        if (attackCombo.Length != 0)
        {
            // �ӹǳ�ӴѺ������ջѨ�غѹ
            int currentAttackIndex = _attackIndex % attackCombo.Length;

            // ���ѭ�ҳ������Фþ��仢�ҧ˹��
            OnAttackForward?.Invoke(directionToMouse, attackForwardSpeed, attackForwardTime);

            // ���ҧ GameObject �ͧ�������
            InstallAttackHit(attackCombo[currentAttackIndex].attackPrefabs, directionToMouse, attackCombo[currentAttackIndex].damage, attackCombo[currentAttackIndex].knockbackForce);

            // �ѻവʶҹ�����Ѻ���⺶Ѵ�
            _attackIndex++;
            _lastAttackTime = Time.time;

            if (!_isComboing)
            {
                _isComboing = true;
                OnComboingStateChange?.Invoke(_isComboing);
            }
            _canAttack = false;
            // ���ѭ�ҳ����������������
            OnAttackStateChange?.Invoke(_canAttack);
            // ��駤�� Cooldown ����
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

        // ��Ǩ�ͺ��Ҥ��⺢Ҵ�͹�������
        if (Time.time - _lastAttackTime > comboWindow && _attackIndex != 0)
        {
            _attackIndex = 0;

            _isComboing = false;
            OnComboingStateChange?.Invoke(_isComboing);
        }

        // �Ѻ���Ҷ����ѧ����Ѻ Cooldown
        if (!_canAttack)
        {
            _attackCooldownTimer -= Time.deltaTime;
            if (_attackCooldownTimer <= 0)
            {
                _canAttack = true;
                // ���ѭ�ҳ�������������
                OnAttackStateChange?.Invoke(_canAttack);
            }
        }
    }

    private void InstallAttackHit(GameObject attackPrefabs, Vector3 directionToMouse, float damage, float knockbackForce)
    {
        // ���ҧ GameObject �ͧ�������
        GameObject attackInstance = Instantiate(attackPrefabs, transform);

        // �ӹǳ��ȷҧ�������
        //Vector3 playerPosition = transform.position;
        // Vector3 directionToMouse = (_mousePosition - playerPosition).normalized;
        attackInstance.transform.position = transform.position + (directionToMouse * attackFart);

        // �ӹǳ�����ع (Rotation)
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

    // �������ʴ�������� Scene View ��ҹ��
    private void OnDrawGizmos()
    {
        // �Ҵ�ش��ᴧ�����˹������
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_mousePosition, 0.2f);

        // �Ҵ��������Ǩҡ����Ф���ѧ���˹������
        Gizmos.color = Color.green;
        Vector3 playerPosition = transform.position;
        Vector3 directionToMouse = (_mousePosition - playerPosition).normalized;

        // Gizmos.DrawRay(startPoint, direction)
        Gizmos.DrawRay(playerPosition, directionToMouse * 2f);

        // --- ������Ѻ�ʴ��ѵ�ٷ��������ش ---
        if (_nearestEnemyCollider != null)
        {
            // �ӹǳ���˹觷���ͧ����Ҵ (������ѵ��)
            Vector3 headPosition = _nearestEnemyCollider.transform.position + Vector3.up * 1.5f; // + Vector3.up * 1.5f ��͡��¡���� 1.5 ˹���

            // �Ҵ��鹨ҡ��������ѧ�ѵ�ٷ��������ش
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, headPosition);

            // �Ҵ�ͤ͹�繷ç���������ѵ��
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(headPosition + Vector3.up, 0.3f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // �Ҵ OverlapBox ����Ѻ��õ�Ǩ�Ѻ�ѵ��
        Gizmos.color = Color.yellow;
        Vector3 overlapCenter = transform.position + Vector3.forward * (overlapBoxFart);
        Gizmos.DrawWireCube(overlapCenter, overlapBoxHalfExtents * 2);
    }
}