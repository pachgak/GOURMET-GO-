using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShamakiriSquadController : MonoBehaviour
{
    [System.Serializable]
    public class MovingSlashConfig
    {
        public GameObject hitPrefab;
        public float delay = 0.4f;
        public float moveDuration = 0.15f;
        public float distance = 8f;

        [Header("Combat Stats")]
        public float damage = 10f;
        public float knockbackForce = 5f;
    }

    [System.Serializable]
    public class StaticSlashConfig
    {
        public GameObject hitPrefab;
        public float delay = 0.4f;

        [Header("Combat Stats")]
        public float damage = 15f;
        public float knockbackForce = 0;
    }

    public List<GameObject> graphicesParent;

    [Header("Squad Status")]
    public List<BaseEnemyAI> activeClones = new List<BaseEnemyAI>();

    [Header("Turn Sequence Settings")]
    public float turnCooldown = 4f;
    private float _turnTimer;
    private bool _isDoingCombo = false;

    private int _currentAttackerIndex = 0;
    private BaseEnemyAI _currentAttackingClone = null;
    private bool _isAttacking = false;

    [Header("Formation Settings")]
    public float formationRadius = 6f;
    public float formationRotateSpeed = 20f;
    private float _formationAngle = 0f;

    [Header("Stalker & Reset Settings")]
    public float stalkerSpeed = 5f;
    public float loseSightDistance = 25f;
    public float loseSightTime = 5f;
    private float _loseSightTimer = 0f;

    [Header("Jump Back Settings")]
    public float jumpBackHeight = 4f;    // ความสูงตอนกระโดดกลับ
    public float jumpBackDuration = 0.8f;// ความเร็วในการกระโดดกลับ (ค่ายิ่งน้อย ยิ่งพุ่งไว)

    // ตัวแปรสำหรับจำว่า ใครแอบวิ่งหลุดวงไปบ้าง (เพื่อที่พอกลับเข้าวงจะได้สั่งให้กระโดด)
    private Dictionary<BaseEnemyAI, bool> _wasOutsideRadius = new Dictionary<BaseEnemyAI, bool>();

    [Header("True Form Spawn Settings")]
    public float spawnMinRadius = 3f;     // ห้ามเกิดใกล้กว่านี้ (กันซ้อนผู้เล่น)
    public float spawnMaxRadius = 8f;     // ห้ามเกิดไกลกว่านี้
    public LayerMask obstacleMask;        // เลเยอร์กำแพง/ฉาก (เพื่อใช้ Raycast เช็คว่ามีอะไรบังไหม)
    public GameObject trueFormVFX;        // เอฟเฟกต์ตอนร่างจริงปรากฏตัว

    [Header("Cinematic Timing")]
    public float delayBeforeVanish = 1.0f;    // ร่างแยกตัวสุดท้ายยืนนิ่งกี่วิ ก่อนระเบิดควัน
    public float emptyScreenDuration = 1.5f;  // จอดำ (ไม่มีบอส) นานกี่วิให้ผู้เล่นระแวง
    public float delayBeforeAttack = 1.5f;    // ร่างแม่โผล่มาแล้ว ยืนขู่กี่วิก่อนเข้าโจมตี

    [Header("Ultimate Combo Settings")]
    public GameObject smokeVFX;             // ควันตอนกระโดดหายไป และตอนวาร์ปกลับมา
    public float attackTime = 1.0f;         // เวลาก่อนเริ่มคอมโบ
    public GameObject warningVFX;           // วงแดงเตือนเป้าหมายที่พื้น
    public float telegraphTime = 1.0f;      // เวลาโชว์วงแดงก่อนเริ่มปาด
    public GameObject slashVFXPrefab;       // เสก VFX แสงดาบ

    [Header("--- Slash Left (ปาดซ้าย) ---")]
    public MovingSlashConfig slashLeft = new MovingSlashConfig { delay = 0f };

    [Header("--- Slash Right (ปาดขวา) ---")]
    public MovingSlashConfig slashRight = new MovingSlashConfig { delay = 0.4f };

    [Header("--- Cross Slash (กากบาท) ---")]
    public StaticSlashConfig crossSlash = new StaticSlashConfig { delay = 0.4f };

    // --- Refs ของตัวแม่ (ดึงมาเก็บไว้ใน Awake) ---
    private BaseEnemyAI _myAI;
    private NavMeshAgent _myAgent;
    private bool _isActiveSquad = false;

    private void Awake()
    {
        _myAI = GetComponent<BaseEnemyAI>();
        _myAgent = GetComponent<NavMeshAgent>();
    }

    public void InitializeSquad(List<GameObject> clones, GameObject target)
    {
        activeClones.Clear();
        _wasOutsideRadius.Clear(); // *** เพิ่มบรรทัดนี้ ***
        _isActiveSquad = true;
        _loseSightTimer = 0f;

        if (clones.Count >= 3)
        {
            foreach (var cloneObj in clones)
            {
                if (cloneObj.TryGetComponent(out BaseEnemyAI cloneAI))
                {
                    activeClones.Add(cloneAI);
                    _wasOutsideRadius[cloneAI] = false;

                    if (cloneObj.TryGetComponent(out EnemyHealth health))
                    {
                        health.OnDie += () => HandleCloneDeath(cloneAI);
                    }
                    cloneAI.TriggerChangeState(BaseEnemyAI.EnemyState.Standby);
                }
            }
        }

        // ปิดการทำงานตัวแม่ ยกเว้นการเดินสะกดรอย
        foreach (var graphice in graphicesParent) graphice.SetActive(false);
        if (TryGetComponent(out Collider col)) col.enabled = false;
        if (_myAI != null) _myAI.enabled = false;
        if (TryGetComponent(out BaseEnemyMovement move)) move.enabled = false;
        if (TryGetComponent(out BaseEnemyCombat combat)) combat.enabled = false;
        if (TryGetComponent(out EnemyHealth healthUser)) healthUser.enabled = false;

        // เปิด Agent ให้เดินตามผู้เล่น
        if (_myAgent != null)
        {
            _myAgent.enabled = true;
            _myAgent.isStopped = false;
            _myAgent.speed = stalkerSpeed;
        }

        _turnTimer = turnCooldown;
        _currentAttackerIndex = 0;
        Debug.Log("Shamakiri Puppet Master: เริ่มสะกดรอย และกางค่ายกล 3 เหลี่ยม!");
    }

    private void Update()
    {
        if (!_isActiveSquad || activeClones.Count == 0 || _isDoingCombo) return;

        // 1. จัดการสะกดรอยและเช็คระยะหนี (ครอบเป็น Method ให้แล้ว!)
        HandleStalkerMode();

        // 2. จัดค่ายกล 3 เหลี่ยม
        MaintainTriangleFormation();

        // 3. เช็คสถานะตัวที่กำลังเข้าไปตี
        if (_isAttacking)
        {
            if (_currentAttackingClone == null || _currentAttackingClone.currentState == BaseEnemyAI.EnemyState.Standby)
            {
                // *** เพิ่มตรงนี้: ตีเสร็จแล้ว! สั่งให้กระโดดกลับค่ายกลเลย ***
                if (_currentAttackingClone != null && activeClones.Contains(_currentAttackingClone))
                {
                    JumpToFormationPoint(_currentAttackingClone);
                }

                _isAttacking = false;
                _currentAttackingClone = null;
                _currentAttackerIndex++;
                _turnTimer = turnCooldown;
            }
            return;
        }

        // 4. รันคิว
        _turnTimer -= Time.deltaTime;
        if (_turnTimer <= 0)
        {
            ExecuteNextTurn();
        }
    }

    // ==========================================
    // แยก Method: การเดินตามและเช็คระยะ
    // ==========================================
    private void HandleStalkerMode()
    {
        // ดึง playerTarget จาก _myAI ตรงๆ ไม่ต้องรับค่าซ้ำซ้อน
        if (_myAI != null && _myAI.playerTarget != null && _myAgent != null && _myAgent.isOnNavMesh)
        {
            _myAgent.SetDestination(_myAI.playerTarget.position);

            float distToPlayer = Vector3.Distance(transform.position, _myAI.playerTarget.position);
            if (distToPlayer > loseSightDistance)
            {
                _loseSightTimer += Time.deltaTime;
                if (_loseSightTimer >= loseSightTime)
                {
                    ResetBoss();
                }
            }
            else
            {
                _loseSightTimer = 0f;
            }
        }
    }

    // ==========================================
    // รีเซ็ตบอสกลับไปเป็นปกติ
    // ==========================================
    private void ResetBoss()
    {
        Debug.Log("[Shamakiri] ผู้เล่นหนีพ้น! ลบค่ายกลทิ้ง แล้วกลับสู่สภาพเดิม");

        _isActiveSquad = false;
        _isAttacking = false;
        _currentAttackingClone = null;

        // ส่งร่างแยกกลับ Pool ตามระบบของคุณ
        foreach (var clone in activeClones)
        {
            if (clone != null)
            {
                ObjectPoolingManager.Instance.Respawn(clone.gameObject);
            }
        }
        activeClones.Clear();
        _wasOutsideRadius.Clear();

        // คืนร่างให้ Container
        foreach (var graphice in graphicesParent) graphice.SetActive(true);
        if (TryGetComponent(out Collider col)) col.enabled = true;

        if (_myAI != null)
        {
            _myAI.enabled = true;
            _myAI.TriggerChangeState(BaseEnemyAI.EnemyState.Roaming);
        }

        if (TryGetComponent(out BaseEnemyMovement move)) move.enabled = true;
        if (TryGetComponent(out BaseEnemyCombat combat)) combat.enabled = true;
        if (TryGetComponent(out EnemyHealth healthUser)) healthUser.enabled = true;

        if (_myAgent != null && move != null)
        {
            _myAgent.speed = move.roamSpeed;
            // ล้างความจำเส้นทางที่เคยวางไว้เดินตาม Player (แก้บั๊ก Agent ค้าง)
            _myAgent.ResetPath();
        }
    }

    private void MaintainTriangleFormation()
    {
        if (_myAI == null || _myAI.playerTarget == null) return;

        _formationAngle += formationRotateSpeed * Time.deltaTime;
        float angleStep = 360f / activeClones.Count;

        for (int i = 0; i < activeClones.Count; i++)
        {
            BaseEnemyAI clone = activeClones[i];
            if (clone == null || clone.currentState != BaseEnemyAI.EnemyState.Standby) continue;

            if (!clone.TryGetComponent(out NavMeshAgent agent) || !agent.isActiveAndEnabled) continue;

            //// *** สำคัญ: ถ้า Agent ปิดอยู่ หรือ "เท้ายังไม่แตะพื้น NavMesh" ห้ามสั่งเดินเด็ดขาด! ***
            //if (!clone.TryGetComponent(out NavMeshAgent agent) || !agent.isActiveAndEnabled || !agent.isOnNavMesh) continue;

            float angle = _formationAngle + (i * angleStep);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * formationRadius;
            Vector3 formationPos = _myAI.playerTarget.position + offset;

            float distToPlayer = Vector3.Distance(clone.transform.position, _myAI.playerTarget.position);

            if (distToPlayer > formationRadius + 1f)
            {
                // ถ้าหลุดขอบวง: ให้วิ่งตรงเข้าหาผู้เล่น และจดจำไว้ว่า "ฉันแอบอยู่ข้างนอกนะ"
                _wasOutsideRadius[clone] = true;
                agent.isStopped = false;
                agent.SetDestination(_myAI.playerTarget.position);
            }
            else
            {
                // ถ้า "ก่อนหน้านี้อยู่ข้างนอก" แล้วตอนนี้ "เพิ่งกลับเข้ามาในระยะได้" -> กระโดดเลย!
                if (_wasOutsideRadius.ContainsKey(clone) && _wasOutsideRadius[clone])
                {
                    JumpToFormationPoint(clone);
                    continue; // ข้ามบรรทัดเดินด้านล่างไป เพราะมันกำลังจะกระโดด
                }

                // ถ้าอยู่ในวงปกติ ก็เดินหมุนค่ายกลตามจุดสีแดงต่อไป
                agent.isStopped = false;
                agent.SetDestination(formationPos);
            }
        }
    }

    private void JumpToFormationPoint(BaseEnemyAI clone)
    {
        if (clone == null || _myAI.playerTarget == null) return;

        int index = activeClones.IndexOf(clone);
        if (index == -1) return;

        // 1. คำนวณจุดที่มันต้องไปยืนตามค่ายกล
        float angleStep = 360f / activeClones.Count;
        float angle = _formationAngle + (index * angleStep);
        Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * formationRadius;

        Vector3 centerPos = _myAI.playerTarget.position;
        Vector3 targetPos = centerPos + offset;

        // ==========================================
        // *** ระบบกันกระโดดทะลุกำแพง (Dynamic Formation Shrink) ***
        // ==========================================
        Vector3 dirToTarget = (targetPos - centerPos).normalized;
        float distToTarget = formationRadius;

        // ยกระดับจุดยิง Raycast ขึ้นมา 1 หน่วย (ระดับอก) เพื่อไม่ให้ยิงชนพื้น
        Vector3 rayStart = centerPos + Vector3.up * 1f;

        // ถ้ายิงแล้วชนกำแพง (obstacleMask)
        if (Physics.Raycast(rayStart, dirToTarget, out RaycastHit hit, distToTarget, obstacleMask))
        {
            // ถอยร่นจากจุดที่ชนกำแพงกลับมาหาผู้เล่น 1 หน่วย (เพื่อไม่ให้โมเดลมอนสเตอร์จมกำแพง)
            Vector3 safePos = hit.point - (dirToTarget * 1f);

            // เช็คซ้ำด้วย NavMesh เพื่อความชัวร์ว่าจุดนั้นยืนได้จริงๆ และเดินต่อได้
            if (NavMesh.SamplePosition(safePos, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                targetPos = navHit.position;
            }
            else
            {
                targetPos = safePos;
            }

            Debug.Log($"[{clone.name}] ค่ายกลติดกำแพง! ร่นระยะกระโดดกลับมาที่ปลอดภัย");
        }
        // ==========================================

        // 2. สั่งกระโดด!
        if (clone.TryGetComponent(out BaseEnemyMovement move))
        {
            move.SkillJump(targetPos, jumpBackHeight, jumpBackDuration);
        }

        // 3. รีเซ็ตค่าว่ามันอยู่ในวงแล้วนะ จะได้ไม่โดนสั่งกระโดดซ้ำซ้อน
        if (_wasOutsideRadius.ContainsKey(clone))
        {
            _wasOutsideRadius[clone] = false;
        }
    }

    private void ExecuteNextTurn()
    {
        if (_currentAttackerIndex >= activeClones.Count)
        {
            _currentAttackerIndex = 0;
            if (activeClones.Count == 3)
            {
                StartCoroutine(UltimateComboAttack());
                return;
            }
        }

        if (_currentAttackerIndex < activeClones.Count)
        {
            _currentAttackingClone = activeClones[_currentAttackerIndex];
            _isAttacking = true;
            _currentAttackingClone.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);
        }
    }

    private void HandleCloneDeath(BaseEnemyAI deadClone)
    {
        // ถ้ากำลังรันคัทซีนเปิดตัวร่างจริงอยู่ ห้ามทำงานซ้ำซ้อน
        if (!_isActiveSquad) return;

        if (activeClones.Contains(deadClone))
        {
            // เสกควันตอนร่างแยกตาย (ตัวที่ 1 และ 2)
            if (trueFormVFX != null && deadClone != null)
            {
                ObjectPoolingManager.Instance.Spawn(trueFormVFX, deadClone.transform.position);
            }

            activeClones.Remove(deadClone);
            if (_wasOutsideRadius.ContainsKey(deadClone)) _wasOutsideRadius.Remove(deadClone);
            if (_currentAttackerIndex >= activeClones.Count) _currentAttackerIndex = 0;

            // เหลือตัวสุดท้าย (ตายไป 2 ตัว) -> เริ่มคัทซีนเปิดตัวร่างจริง
            if (activeClones.Count == 1)
            {
                StartCoroutine(RevealTrueFormRoutine()); // เปลี่ยนมาเรียก Coroutine แทน
            }
        }
    }

    private void RevealTrueFormPhase()
    {
        Debug.Log("[Shamakiri] ร่างแยกถูกทำลาย 2 ตัว! ร่างจริงกำลังจะปรากฏตัว!");

        // 1. ปิดระบบ Controller ค่ายกล
        _isActiveSquad = false;
        _isAttacking = false;
        _currentAttackingClone = null;

        // 2. ลบร่างแยกตัวสุดท้ายทิ้ง
        if (activeClones.Count > 0 && activeClones[0] != null)
        {
            // สามารถใส่ VFX ระเบิดร่างแยกตรงนี้ได้ก่อนลบ
            ObjectPoolingManager.Instance.Respawn(activeClones[0].gameObject);
        }
        activeClones.Clear();
        _wasOutsideRadius.Clear();

        // 3. หาจุดเกิดที่ปลอดภัย (Safe Spawn)
        Vector3 safeSpawnPos = GetSafeSpawnPosition();

        // 4. วาร์ปตัวแม่ที่ล่องหนอยู่ ไปที่จุดปลอดภัย
        if (_myAgent != null)
        {
            _myAgent.Warp(safeSpawnPos);
            _myAgent.isStopped = true;
            _myAgent.ResetPath();
        }
        else
        {
            transform.position = safeSpawnPos;
        }

        // 5. เล่นเอฟเฟกต์ปรากฏตัว
        if (trueFormVFX != null)
        {
            ObjectPoolingManager.Instance.Spawn(trueFormVFX, safeSpawnPos);
        }

        // 6. คืนร่างให้ตัวแม่ และเปิดระบบต่อสู้ของมัน!
        foreach (var graphice in graphicesParent) graphice.SetActive(true);
        if (TryGetComponent(out Collider col)) col.enabled = true;

        if (_myAI != null)
        {
            _myAI.enabled = true;
            _myAI.TriggerChangeState(BaseEnemyAI.EnemyState.Chase); // ออกมาปุ๊บ สั่งไล่ล่าเลย!
        }

        if (TryGetComponent(out BaseEnemyMovement move)) move.enabled = true;
        if (TryGetComponent(out EnemyHealth healthUser)) healthUser.enabled = true;

        if (TryGetComponent(out BaseEnemyCombat combat))
        {
            combat.enabled = true;

            // *** เพิ่มบรรทัดนี้: ถ้าคอมแบทเป็นร่างแม่ ให้เปิดโหมดโกรธ! ***
            if (combat is ShamakiriContainerCombat shamakiriCombat)
            {
                shamakiriCombat.SetEnragePhase(true);
            }
        }


    }

    private IEnumerator RevealTrueFormRoutine()
    {
        Debug.Log("[Shamakiri] ร่างแยกถูกทำลาย 2 ตัว! เริ่ม Sequence ร่างจริง...");

        // 1. ปิดระบบ Controller ค่ายกล
        _isActiveSquad = false;
        _isAttacking = false;
        _currentAttackingClone = null;

        // 2. สั่งร่างแยกตัวสุดท้ายให้ "หยุดนิ่ง"
        BaseEnemyAI lastClone = null;
        if (activeClones.Count > 0)
        {
            lastClone = activeClones[0];
            if (lastClone != null)
            {
                lastClone.TriggerChangeState(BaseEnemyAI.EnemyState.Standby);
                if (lastClone.TryGetComponent(out NavMeshAgent agent)) agent.isStopped = true;
            }
        }

        // --- หน่วงเวลาที่ 1: ให้ร่างแยกยืนนิ่งๆ ให้ผู้เล่นงง ---
        yield return new WaitForSeconds(delayBeforeVanish);

        // 3. ระเบิดร่างแยกตัวสุดท้ายทิ้ง พร้อม VFX ควัน
        if (lastClone != null)
        {
            if (trueFormVFX != null)
            {
                ObjectPoolingManager.Instance.Spawn(trueFormVFX, lastClone.transform.position);
            }
            ObjectPoolingManager.Instance.Respawn(lastClone.gameObject);
        }
        activeClones.Clear();
        _wasOutsideRadius.Clear();

        // --- หน่วงเวลาที่ 2: ปล่อยฉากให้ว่างเปล่า สร้างความระแวง ---
        yield return new WaitForSeconds(emptyScreenDuration);

        // 4. หาจุดเกิดที่ปลอดภัย และวาร์ปร่างแม่ไปรอ
        Vector3 safeSpawnPos = GetSafeSpawnPosition();

        if (_myAgent != null)
        {
            _myAgent.Warp(safeSpawnPos);
            _myAgent.isStopped = true;
            _myAgent.ResetPath();
        }
        else
        {
            transform.position = safeSpawnPos;
        }

        // 5. ร่างแม่ปรากฏตัวพร้อมควัน!
        if (trueFormVFX != null)
        {
            ObjectPoolingManager.Instance.Spawn(trueFormVFX, safeSpawnPos);
        }

        // เปิดโชว์กราฟิกของร่างแม่
        foreach (var graphice in graphicesParent) graphice.SetActive(true);
        if (TryGetComponent(out Collider col)) col.enabled = true;

        // คืนสมองให้แม่ แต่สั่งให้ "รอดูเชิง (Standby)" ไปก่อน
        if (_myAI != null)
        {
            _myAI.enabled = true;
            _myAI.TriggerChangeState(BaseEnemyAI.EnemyState.Standby);
        }

        if (TryGetComponent(out BaseEnemyMovement move)) move.enabled = true;
        if (TryGetComponent(out EnemyHealth healthUser)) healthUser.enabled = true;

        // --- หน่วงเวลาที่ 3: ยืนขู่ผู้เล่นก่อนเข้าโจมตี ---
        yield return new WaitForSeconds(delayBeforeAttack);

        // 6. เปิดโหมดโกรธ แล้ววิ่งเข้าใส่ผู้เล่นเลย!
        if (TryGetComponent(out BaseEnemyCombat combat))
        {
            combat.enabled = true;
            if (combat is ShamakiriContainerCombat shamakiriCombat)
            {
                shamakiriCombat.SetEnragePhase(true); // เปลี่ยนสกิลเป็นโหมดโกรธ
            }
        }

        if (_myAI != null)
        {
            _myAI.TriggerChangeState(BaseEnemyAI.EnemyState.Chase); // สั่งไล่ล่า!
        }
    }

    // ==========================================
    // ฟังก์ชัน: หาจุดเกิดที่ไม่ติดกำแพง และไม่ซ้อนผู้เล่น (Safe Spawn)
    // ==========================================
    private Vector3 GetSafeSpawnPosition()
    {
        if (_myAI == null || _myAI.playerTarget == null) return transform.position;

        Transform player = _myAI.playerTarget;
        Vector3 bestPos = transform.position;
        bool foundSafeSpot = false;

        // ลองสุ่มหา 10 ครั้ง เพื่อหาจุดที่ดีที่สุด (ป้องกันเกมค้างถ้าหาทางออกไม่ได้)
        for (int i = 0; i < 10; i++)
        {
            // 1. สุ่มระยะห่างและมุม
            float randomDist = Random.Range(spawnMinRadius, spawnMaxRadius);
            float randomAngle = Random.Range(0f, 360f);

            Vector3 offset = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward * randomDist;
            Vector3 potentialPos = player.position + offset;

            NavMeshHit hit;
            // 2. เช็คว่าจุดสมมตินั้น อยู่บน NavMesh ไหม
            if (NavMesh.SamplePosition(potentialPos, out hit, 2f, NavMesh.AllAreas))
            {
                // 3. เช็คว่ามี "กำแพง" บังระหว่างจุดเกิดกับผู้เล่นไหม
                Vector3 dirToPlayer = player.position - hit.position;

                // ยกจุดยิง Raycast ขึ้นมานิดนึง (เช่น 1 หน่วย หรือระดับอก) จะได้ไม่ยิงชนพื้น
                Vector3 rayStart = hit.position + Vector3.up * 1f;

                // ถ้ายิง Raycast ไปหาผู้เล่น แล้วไม่โดน Layer สิ่งกีดขวางเลย แปลว่าจุดนั้นโล่ง!
                if (!Physics.Raycast(rayStart, dirToPlayer.normalized, dirToPlayer.magnitude, obstacleMask))
                {
                    bestPos = hit.position;
                    foundSafeSpot = true;
                    break; // เจอจุดที่ปลอดภัย 100% แล้ว หยุดหาทันที
                }
            }
        }

        // ถ้าดวงซวยจริงๆ โดนต้อนเข้ามุมแคบ หาจุดโล่งๆ 10 ครั้งไม่เจอเลย 
        if (!foundSafeSpot)
        {
            Debug.LogWarning("[Shamakiri] หาจุดเกิดที่ปลอดภัยไม่เจอ! สุ่มเกิดขอบนอกสุดเลยละกัน");
            // สุ่มเกิดวงนอกสุดไปเลย จะได้ไม่ซ้อนผู้เล่น
            Vector3 fallbackPos = player.position + (Random.insideUnitSphere.normalized * spawnMaxRadius);
            if (NavMesh.SamplePosition(fallbackPos, out NavMeshHit fallbackHit, spawnMaxRadius, NavMesh.AllAreas))
            {
                bestPos = fallbackHit.position;
            }
        }

        return bestPos;
    }

    //private void TriggerEnrageMode(BaseEnemyAI lastClone)
    //{
    //    lastClone.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);
    //}

    private IEnumerator UltimateComboAttack()
    {
        _isDoingCombo = true;
        Debug.Log("[Shamakiri] เริ่มท่าไม้ตาย: กระโดดขึ้นฟ้า!");

        float jumpHeight = 10f;
        float jumpDur = 1.5f;

        // 1. สั่งให้ทั้ง 3 ตัวกระโดด
        foreach (var clone in activeClones)
        {
            if (clone == null) continue;
            clone.TriggerChangeState(BaseEnemyAI.EnemyState.Standby);
            if (clone.TryGetComponent(out NavMeshAgent agent)) agent.isStopped = true;

            if (clone.TryGetComponent(out BaseEnemyMovement movement))
            {
                movement.SkillJump(clone.transform.position, jumpHeight, jumpDur);
            }
        }

        // 2. รอถึงจุดสูงสุด แล้วเสกควัน+ซ่อนตัว
        yield return new WaitForSeconds(jumpDur / 2f);
        foreach (var clone in activeClones)
        {
            if (clone != null)
            {
                if (smokeVFX != null) ObjectPoolingManager.Instance.Spawn(smokeVFX, clone.transform.position);
                if (clone.TryGetComponent(out NavMeshAgent agent)) agent.enabled = false;

                // *** เพิ่มตรงนี้: สั่งให้ร่างแยกล่องหน (ปิดกราฟิกและกล่องชน) ***
                if (clone.TryGetComponent(out ShamakiriCloneAI cloneAI)) cloneAI.SetVisibility(false);

                // เอาลงใต้ดินด้วย (ที่คุณทำไว้ดีแล้วครับ ช่วยกันบั๊กได้อีกชั้น)
                clone.transform.position = new Vector3(clone.transform.position.x, -50f, clone.transform.position.z);
            }
        }

        // รอจนหมดเวลาการกระโดด
        yield return new WaitForSeconds(jumpDur / 2f);

        // ล็อคเป้าผู้เล่น

        // 3. หน่วงเวลารอ (attackTime) และโชว์วงแดง (telegraphTime)
        yield return new WaitForSeconds(attackTime);

        GameObject warningInstance = null;

        if (warningVFX != null)
        {
            warningInstance = ObjectPoolingManager.Instance.Spawn(warningVFX, _myAI.playerTarget.position);

            if (warningInstance.TryGetComponent(out IDurationable trackable))
            {
                // ถ้าสคริปต์นั้นรองรับเรื่องเวลา ก็ส่งเวลาไปให้ด้วย
                trackable.SetDurationTime(telegraphTime * 0.5f);
            }
            
            if (warningInstance.TryGetComponent(out ITargetable targetable))
            {
                Debug.Log($"ITargetable : Do");
                // ถ้าสคริปต์เป็นพวกกระสุนหรืออะไรที่ตามตลอดกาล (ไม่มีตัวแปรเวลา) ก็ทำแค่เซ็ตเป้าหมาย
                Debug.Log($"playerTarget : {_myAI.playerTarget.gameObject.name}");

                targetable.SetTarget(_myAI.playerTarget);
            }
        }

        yield return new WaitForSeconds(telegraphTime);

        Vector3 attackTargetPos = warningInstance.transform.position;

        if (slashVFXPrefab != null) ObjectPoolingManager.Instance.Spawn(slashVFXPrefab, attackTargetPos);

        // ==========================================
        // Sequence การโจมตี
        // ==========================================

        // Hit 1: ปาดซ้าย (Left มาก่อนตาม VFX)
        yield return new WaitForSeconds(slashLeft.delay);
        if (slashLeft.hitPrefab != null)
        {
            Vector3 startPos = attackTargetPos + new Vector3(-slashLeft.distance, 0, slashLeft.distance);
            Vector3 endPos = attackTargetPos + new Vector3(slashLeft.distance, 0, -slashLeft.distance);
            SpawnMovingSlash(slashLeft, startPos, endPos);
        }

        // Hit 2: ปาดขวา (Right ตามหลัง)
        yield return new WaitForSeconds(slashRight.delay);
        if (slashRight.hitPrefab != null)
        {
            Vector3 startPos = attackTargetPos + new Vector3(slashRight.distance, 0, slashRight.distance);
            Vector3 endPos = attackTargetPos + new Vector3(-slashRight.distance, 0, -slashRight.distance);
            SpawnMovingSlash(slashRight, startPos, endPos);
        }

        // Hit 3: สับกากบาท
        yield return new WaitForSeconds(crossSlash.delay);
        if (crossSlash.hitPrefab != null)
        {
            SpawnStaticSlash(crossSlash, attackTargetPos);
        }

        // หน่วงเวลาหลังจบคอมโบก่อนวาร์ปกลับมาโชว์ตัว
        yield return new WaitForSeconds(1.0f);

        // ==========================================
        // กลับค่ายกล สลับ HP และ สลับตำแหน่ง!
        // ==========================================
        SwapClonesHP();          // สลับเลือดให้งง
        ShuffleClonesPositions(); // *** เพิ่มตรงนี้: สลับตำแหน่งตัวมอนสเตอร์ให้สับสน ***

        float angleStep = 360f / activeClones.Count;
        for (int i = 0; i < activeClones.Count; i++)
        {
            BaseEnemyAI clone = activeClones[i];
            if (clone == null) continue;

            // เนื่องจาก activeClones ถูกสับเปลี่ยนแล้ว ตำแหน่ง angle ของแต่ละตัวก็จะเปลี่ยนไปจากเดิม
            float angle = _formationAngle + (i * angleStep);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * formationRadius;
            Vector3 formPos = _myAI.playerTarget.position + offset;

            clone.transform.position = formPos;
            if (clone.TryGetComponent(out NavMeshAgent agent))
            {
                agent.enabled = true;
                agent.Warp(formPos);
            }

            if (smokeVFX != null) ObjectPoolingManager.Instance.Spawn(smokeVFX, formPos);

            // สั่งให้ร่างแยกปรากฏตัวกลับมา!
            if (clone.TryGetComponent(out ShamakiriCloneAI cloneAI)) cloneAI.SetVisibility(true);

            clone.TriggerChangeState(BaseEnemyAI.EnemyState.Standby);
        }

        _isDoingCombo = false;
        _turnTimer = turnCooldown;
    }

    // ฟังก์ชันผู้ช่วยสำหรับเสกและสั่งวิ่ง Hitbox แบบเส้นตรง
    // ฟังก์ชันผู้ช่วยสำหรับเสกและสั่งวิ่ง Hitbox แบบเส้นตรง (ปาด)
    // ฟังก์ชันผู้ช่วยสำหรับเสกและสั่งวิ่ง Hitbox แบบเส้นตรง (ปาด)
    private void SpawnMovingSlash(MovingSlashConfig config, Vector3 startPos, Vector3 endPos)
    {
        GameObject slashObj = ObjectPoolingManager.Instance.Spawn(config.hitPrefab, startPos);

        // 1. สั่งให้เคลื่อนที่
        if (slashObj.TryGetComponent(out MovingSlashHitbox mover))
        {
            mover.Setup(startPos, endPos, config.moveDuration);
        }
        else
        {
            Debug.LogWarning("Prefab การปาด ไม่มีสคริปต์ MovingSlashHitbox ติดอยู่!");
        }

        // 2.1 เพิ่มใหม่: อัปเดตเวลาเคลียร์ตัวเองลง Pool (ITimeDestroy) ***
        if (slashObj.TryGetComponent(out ITimeDestroy timeDestroy))
        {
            timeDestroy._lifeTime = config.moveDuration;
            // ต้องเรียก StartLifeTime() ซ้ำ เพื่อล้างคำสั่ง Invoke เก่าของ OnEnable แล้วเริ่มนับเวลาใหม่
            timeDestroy.StartLifeTime();
        }

        // 2.2 ป้อนค่า ดาเมจ และ น็อคแบ็ค
        if (slashObj.TryGetComponent(out IHitBox hitBox))
        {
            hitBox._ownerHit = gameObject; // ให้รู้ว่า Shamakiri เป็นคนตี
            hitBox._damage = config.damage;
            hitBox._knockbackDirection = (endPos - startPos).normalized; // กระเด็นไปตามทิศที่ปาด!
            hitBox._knockbackForce = config.knockbackForce;
            hitBox._targetLayer = LayerMask.GetMask("Player");

            hitBox.PerformAttack();
        }
    }

    // ฟังก์ชันผู้ช่วยสำหรับเสก Hitbox อยู่กับที่ (กากบาท)
    private void SpawnStaticSlash(StaticSlashConfig config, Vector3 spawnPos)
    {
        GameObject slashObj = ObjectPoolingManager.Instance.Spawn(config.hitPrefab, spawnPos);

        if (slashObj.TryGetComponent(out IHitBox hitBox))
        {
            hitBox._ownerHit = gameObject;
            hitBox._damage = config.damage;
            hitBox._knockbackDirection = Vector3.zero; // อาจจะไม่มีทิศกระเด็น หรือปรับแต่งเองได้
            hitBox._knockbackForce = config.knockbackForce;
            hitBox._targetLayer = LayerMask.GetMask("Player");

            hitBox.PerformAttack();
        }
    }

    // ฟังก์ชันผู้ช่วย: สลับค่า HP ของร่างแยกแบบสุ่ม
    private void SwapClonesHP()
    {
        List<EnemyHealth> healthList = new List<EnemyHealth>();
        List<float> hpValues = new List<float>();

        // 1. ดึงเลือดของทุกคนมาเก็บไว้
        foreach (var clone in activeClones)
        {
            if (clone != null && clone.TryGetComponent(out EnemyHealth hp))
            {
                healthList.Add(hp);
                //hpValues.Add(hp.getHp()); // เก็บค่าเลือดปัจจุบัน
                hpValues.Add(hp.currentHealth); // เก็บค่าเลือดปัจจุบัน
            }
        }

        // 2. สับไพ่ (Shuffle) ค่าเลือดใน List
        for (int i = 0; i < hpValues.Count; i++)
        {
            float temp = hpValues[i];
            int randomIndex = Random.Range(i, hpValues.Count);
            hpValues[i] = hpValues[randomIndex];
            hpValues[randomIndex] = temp;
        }

        // 3. จ่ายเลือดที่สลับแล้ว คืนให้แต่ละตัว
        for (int i = 0; i < healthList.Count; i++)
        {
            healthList[i].setHp(hpValues[i]);
        }

        Debug.Log("<color=magenta>[Shamakiri] เล่นมายากล สลับเลือดร่างแยกเรียบร้อย!</color>");
    }

    // ฟังก์ชันผู้ช่วย: สลับตำแหน่งของร่างแยกใน List (ทำให้ยืนสลับที่ และคิว/สกิลโจมตีเปลี่ยน)
    private void ShuffleClonesPositions()
    {
        for (int i = 0; i < activeClones.Count; i++)
        {
            BaseEnemyAI temp = activeClones[i];
            int randomIndex = Random.Range(i, activeClones.Count);
            activeClones[i] = activeClones[randomIndex];
            activeClones[randomIndex] = temp;
        }
        Debug.Log("<color=yellow>[Shamakiri] เล่นมายากล สลับตำแหน่งและคิวการโจมตี!</color>");
    }

    private void OnDrawGizmosSelected()
    {
        // 1. หาจุดศูนย์กลาง (อิงตามผู้เล่นถ้ากด Play อยู่ หรืออิงตามตัวแม่ถ้ายังไม่ได้กด Play)
        Vector3 centerPos = transform.position;
        if (Application.isPlaying && _myAI != null && _myAI.playerTarget != null)
        {
            centerPos = _myAI.playerTarget.position;
        }

        // 2. วาดวงกลมรัศมีค่ายกล (สีฟ้า)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centerPos, formationRadius);

        // ป้องกันการหาร 0 (ถ้ายังไม่กด Play ให้จำลองว่ามี 3 ตัวไปก่อน)
        int currentCloneCount = Application.isPlaying ? activeClones.Count : 3;
        if (currentCloneCount == 0) return;

        float angleStep = 360f / currentCloneCount;

        // 3. จำลองการคำนวณและวาดจุดเป้าหมายทั้ง 3 จุด
        for (int i = 0; i < currentCloneCount; i++)
        {
            float angle = _formationAngle + (i * angleStep);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * formationRadius;
            Vector3 targetPos = centerPos + offset;

            // วาดเส้นโยงจากศูนย์กลางไปหาเป้าหมาย (สีเหลือง)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(centerPos, targetPos);

            // วาดลูกบอลตรงจุดที่ Agent ต้องเดินไปให้ถึง (สีแดง)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPos, 0.5f);

            // (ออปชันเสริม) ถ้ากด Play อยู่ ให้วาดเส้นโยงจากตัวโคลนไปหาจุดเป้าหมายด้วย
            if (Application.isPlaying && i < activeClones.Count && activeClones[i] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(activeClones[i].transform.position, targetPos);
            }
        }
    }
}