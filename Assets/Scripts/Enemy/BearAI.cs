using System;
using UnityEngine;

public class BearAI : BaseEnemyAI
{
    [Header("Chase Settings")]
    public float chaseTimeoutDuration = 4f; // นานแค่ไหนถึงจะตัดบทใช้สกิล (เช่น 4 วินาที)
    public float chaseTimeoutMinAttack = 2f; // นานแค่ไหนถึงจะตัดบทใช้สกิล (เช่น 4 วินาที)
    [SerializeField] private float chaseTimer = 0f;
    
    // ตัวแปรนี้จะให้ Combat มาเช็คว่าเป็นการโจมตีแบบไหน
    public bool isChaseTimeoutAttack = false;

    // FSM Variables
    private bool forceUseSkill3 = false;

    public bool isAngry = false;

    public Action<bool> OnAngryChang;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        _enemyHealth.OnCurrentChang += HandleCurrentChang;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _enemyHealth.OnCurrentChang -= HandleCurrentChang;
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

    private void HandleCurrentChang(float hp)
    {
        if (hp <= _enemyHealth.maxHealth / 2)
        {
            isAngry = true;
            
        }
        else if (hp > _enemyHealth.maxHealth / 2)
        {
            isAngry = false;
        }

        OnAngryChang?.Invoke(isAngry);
    }

    protected override void ChaseChangeStateLogic()
    {
        // 1. ถ้าเข้าระยะโจมตีปกติ -> ให้โจมตีแบบปกติ (Reset Flag)
        if (_playerInAttackRange)
        {
            if(chaseTimer >= chaseTimeoutDuration - chaseTimeoutMinAttack) chaseTimer = chaseTimeoutDuration - chaseTimeoutMinAttack;
            isChaseTimeoutAttack = false; // บอกว่านี่คือการตีปกติ
            ChangeState(EnemyState.Attack);
            return;
        }

        // 2. ถ้ายังไม่ถึงระยะ แต่ "ไล่นานเกินไป" -> บังคับโจมตีด้วยสกิล 1 หรือ 3
        chaseTimer += Time.deltaTime;
        if (chaseTimer >= chaseTimeoutDuration)
        {
            chaseTimer = 0f;
            isChaseTimeoutAttack = true; // บอกว่านี่คือการตีเพราะหมดเวลาไล่
            ChangeState(EnemyState.Attack); // สั่งเข้า Attack State ทั้งที่ยังไม่ถึงระยะ
            return;
        }

        // 3. Logic การไล่ตามปกติ
        if (!_playerInSightRange)
        {
            //chaseTimer = 0f;
            ChangeState(EnemyState.Roaming);
        }
        else
        {
            TriggerStartChase(playerTarget.position);
        }
    }
}
