using UnityEngine;
using UnityEngine.AI;

public class BunBunnyAI : BaseEnemyAI
{
    [Header("Bunny Flee Settings")]
    public float fleeDistance = 10f;       // หนีไปไกลแค่ไหนจากจุดเดิม
    public float obstacleCheckRadius = 1f; // รัศมีเช็คสิ่งกีดขวาง
    public int searchAngleStep = 30;       // องศาการหมุนหาทาง

    [Header("Bunny Sensitivity")]
    [Tooltip("ระยะสายตาตอน 'กำลังหนี' (ควรมากกว่า Sight Range ปกติใน Base)")]
    public float fleeSightRange = 25f;     // <--- ตัวแปรใหม่ที่คุณต้องการ

    // -------------------------------------------------------------------------
    // 1. Override CheckPlayerDistance เพื่อเปลี่ยนกติกาการมองเห็นตาม State
    // -------------------------------------------------------------------------
    protected override void CheckPlayerDistance()
    {
        if (playerTarget == null)
        {
            _playerInSightRange = false;
            _playerInAttackRange = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        // Logic สำคัญอยู่ตรงนี้:
        // ถ้าสถานะปัจจุบันคือ Chase (หนี) -> ให้ใช้ระยะ fleeSightRange (ระแวงไกลกว่า)
        // ถ้าสถานะอื่น (Roaming)       -> ให้ใช้ sightRange (ระยะปกติจาก Base)
        float currentDetectionRange = (currentState == EnemyState.Roaming) ? sightRange : fleeSightRange ;
        _playerInSightRange = distance <= currentDetectionRange;

        // Attack Range ใช้ logic เดิม (แม้กระต่ายจะไม่ได้โจมตีก็ตาม)
        _playerInAttackRange = distance <= attackRange;
    }

    // -------------------------------------------------------------------------
    // 2. Logic การหนี (เหมือนเดิม แต่ทำงานร่วมกับ CheckPlayerDistance ใหม่)
    // -------------------------------------------------------------------------
    protected override void ChaseChangeStateLogic()
    {
        // เนื่องจากเราแก้ CheckPlayerDistance แล้ว 
        // _playerInSightRange จะเป็น false ยากขึ้นถ้ากำลังหนีอยู่ (ต้องหนีไปไกลมากจริงๆ ถึงจะหยุด)
        if (!_playerInSightRange)
        {
            ChangeState(EnemyState.Roaming);
            return;
        }

        // ถ้ายังเห็นผู้เล่นอยู่ (ในระยะ fleeSightRange) ก็หนีต่อ
        Vector3 fleeDest = GetSmartFleePosition();
        Debug.Log($"fleeDest : {fleeDest}");
        if (fleeDest != Vector3.zero)
        {
            TriggerStartChase(fleeDest);
        }
    }



    // --- (Smart Flee Logic เดิม ไม่มีการเปลี่ยนแปลง) ---
    private Vector3 GetSmartFleePosition()
    {
        if (playerTarget == null) return transform.position;

        Vector3 dirToPlayer = transform.position - playerTarget.position;
        Vector3 baseFleeDir = dirToPlayer.normalized;

        // ลองหมุนหาทางหนี
        for (int i = 0; i < 360; i += searchAngleStep)
        {
            if (TryGetValidFleePoint(baseFleeDir, i, out Vector3 result))
                return result;

            if (i != 0 && TryGetValidFleePoint(baseFleeDir, -i, out Vector3 resultNeg))
                return resultNeg;
        }
        return transform.position;
    }

    private bool TryGetValidFleePoint(Vector3 baseDir, float angle, out Vector3 validPoint)
    {
        validPoint = Vector3.zero;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 checkDir = rot * baseDir;
        Vector3 potentialPos = transform.position + (checkDir * fleeDistance);
        NavMeshHit hit;

        if (NavMesh.SamplePosition(potentialPos, out hit, obstacleCheckRadius, NavMesh.AllAreas))
        {
            if (!NavMesh.Raycast(transform.position, hit.position, out NavMeshHit rayHit, NavMesh.AllAreas))
            {
                validPoint = hit.position;
                return true;
            }
        }
        return false;
    }

    protected override void HandleAttackFinished()
    {
        if (_playerInSightRange) ChangeState(EnemyState.Chase);
        else ChangeState(EnemyState.Roaming);
    }

    // -------------------------------------------------------------------------
    // 3. วาด Gizmos ให้เห็นทั้งสองระยะ
    // -------------------------------------------------------------------------
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // วาดวงเหลือง (Sight) และแดง (Attack) ของเดิม

        // วาดวงสีส้ม สำหรับระยะหนี (Flee Sight Range)
        Gizmos.color = new Color(1f, 0.5f, 0f); // สีส้ม
        Gizmos.DrawWireSphere(transform.position, fleeSightRange);

        // วาดเส้นทางหนีถ้ากำลังทำงาน
        if (Application.isPlaying && currentState == EnemyState.Chase && playerTarget != null)
        {
            Gizmos.color = Color.green;
            Vector3 dir = (transform.position - playerTarget.position).normalized;
            Gizmos.DrawLine(transform.position, transform.position + dir * fleeDistance);
        }
    }
}