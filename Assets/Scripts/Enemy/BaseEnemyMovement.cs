// EnemyMovement.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemyMovement : MonoBehaviour
{
    [Header("Roaming")]
    public float roamRadius = 20f;
    public float waitRoamTime = 2;
    public float roamSpeed = 3.6f;
    public float chaseSpeed = 5f;

    private Vector3 _roamPoint;
    private BaseEnemyAI.EnemyState _currentState;
    private NavMeshAgent _agent;
    private BaseEnemyAI _aiController;
    private bool _isWaiting;
    private float _timerWaiting;

    private void Awake()
    {
        // *** จัดการตัวเอง: หา Reference ที่จำเป็นทั้งหมด ***
        _agent = GetComponent<NavMeshAgent>();
        _aiController = GetComponent<BaseEnemyAI>();

        // Safety Check
        if (_agent == null || _aiController == null)
        {
            Debug.LogError($"{GetType().Name} requires NavMeshAgent and BaseEnemyAI on the same GameObject.");
            enabled = false;
            return;
        }

    }

    private void OnEnable()
    {
        // *** สมัครรับ Events จาก BaseEnemyAI เพื่อรับคำสั่ง ***
        _aiController.OnStartChase += HandleStartChase;
        _aiController.OnStopMovement += HandlStopMovement;
        _aiController.OnStateChange += HandlStateChange;
    }

    private void OnDisable()
    {
        // ยกเลิกการสมัครรับ Event เมื่อ Object ถูกปิดการใช้งาน
        if (_aiController != null)
        {
            _aiController.OnStartChase -= HandleStartChase;
            _aiController.OnStopMovement -= HandlStopMovement;
        }
    }

    private void HandlStateChange(BaseEnemyAI.EnemyState state)
    {
        _currentState = state;
        if (state == BaseEnemyAI.EnemyState.Roaming)
        {
            _isWaiting = false;
            _agent.speed = roamSpeed;
        }

        if(state == BaseEnemyAI.EnemyState.Chase) _agent.speed = chaseSpeed;
    }

    private void HandleStartChase(Vector3 targetPosition)
    {
        MoveToTarget(targetPosition);
    }

    private void HandlStopMovement()
    {
        StopMovement();
    }

    private void Start()
    {
        _roamPoint = transform.position;
    }

    void Update()
    {
        // Logic การ Roaming: ถ้าอยู่ใน Roaming State และถึงจุดหมายแล้ว ให้สุ่มจุดใหม่
        if (_aiController.currentState == BaseEnemyAI.EnemyState.Roaming && IsAtDestination())
        {
            if (!_isWaiting)
            {
                _isWaiting = true;
                _timerWaiting = waitRoamTime;
            }
            else
            {
                if (_timerWaiting > 0)
                {
                    _timerWaiting -= Time.deltaTime;
                }
                else
                {
                    SetNewRoamPoint(_roamPoint);

                    _isWaiting = false;
                }
            }

            //if (_timerWaiting > 0)
            //{
            //    _timerWaiting -= Time.deltaTime;
            //}
            //else
            //{
            //    SetNewRoamPoint(_roamPoint);
            //    _timerWaiting = waitRoamTime;
            //}
        }
    }

    // --- Event Handlers (ถูกเรียกโดย BaseEnemyAI ผ่าน Events) ---

    private void MoveToTarget(Vector3 targetPosition)
    {
        _agent.isStopped = false;
        _agent.SetDestination(targetPosition);
        Debug.LogWarning($"MoveToTarget {targetPosition}");
    }

    private void StopMovement()
    {
        _agent.isStopped = true;
    }

    // --- Roaming Logic ---

    public void StartRoaming(Vector3 centerPosition)
    {
        _agent.isStopped = false;
        SetNewRoamPoint(centerPosition);
    }

    private bool IsAtDestination()
    {
        // ตรวจสอบว่าถึงจุดหมายแล้วหรือไม่
        return _agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending;
    }

    private void SetNewRoamPoint(Vector3 centerPosition)
    {


        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * roamRadius;
        randomDirection += centerPosition;
        NavMeshHit hit;

        // หาจุดที่ถูกต้องบน NavMesh
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_roamPoint, roamRadius);
    }
}