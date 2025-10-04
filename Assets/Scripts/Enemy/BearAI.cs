using UnityEngine;

public class BearAI : BaseEnemyAI
{
    public float chaseDurationForSkill3 = 5f; // เวลาที่ต้องอยู่ใน ChaseState (5 วินาที)

    // FSM Variables
    private float chaseTimer = 0f;
    private bool forceUseSkill3 = false;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void ChaseChangeStateLogic()
    {
        if (_playerInAttackRange)
        {
            chaseTimer = 0f;
            ChangeState(EnemyState.Attack); // เข้า Attack State ตามปกติ
            return;
        }

        // 1. เงื่อนไขพิเศษ: Chase State นานเกินไป
        chaseTimer += Time.deltaTime;
        if (chaseTimer >= chaseDurationForSkill3)
        {
            forceUseSkill3 = true; // <--- ตั้งค่าแฟล็ก!
            chaseTimer = 0f;
            ChangeState(EnemyState.Attack); // เข้า Attack State (เพื่อใช้ Skill 3 พิเศษ)
            return;
        }

        if (!_playerInSightRange)
        {
            chaseTimer = 0f;
            ChangeState(EnemyState.Roaming);
        }
        else
        {
            TriggerStartChase(playerTarget.position);
        }
    }
}
