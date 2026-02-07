using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BearCombat : BaseEnemyCombat
{
    [SerializeField] private float skillSpeedMultiplier = 1;
    private BearAI _bearAI;
    private bool _isAngry = false;

    [Header("Mushroom Ability")]
    [SerializeField] private GameObject mushroomPrefab; // ลาก Prefab เห็ดมาใส่ตรงนี้
    [SerializeField] private float mushroomSpawnRadius = 4f; // รัศมี 4 หน่วย

    protected override void Awake()
    {
        base.Awake();

        _bearAI = GetComponent<BearAI>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        _bearAI.OnAngryChang += HandleAngryChang;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _bearAI.OnAngryChang -= HandleAngryChang;
    }

   
    protected override void Update()
    {
        base.Update();
        //if (_enemyHealth.isDead) return;

        //if (attackTimer > 0 && _attackSequenceCoroutine == null)
        //{
        //    attackTimer -= Time.deltaTime;

        //    if (attackTimer <= 0)
        //    {
        //        _agent.isStopped = false;
        //    }
        //    else
        //    {
        //        _agent.isStopped = true;
        //    }
        //}

        //// ---  State Logi ---
        //switch (_aiController.currentState)
        //{
        //    case BaseEnemyAI.EnemyState.Attack:

        //        if (attackTimer <= 0 && _attackSequenceCoroutine == null)
        //        {
        //            HandleStartAttackSequence();
        //            attackTimer = attackCooldown;
        //        }

        //        if (_attackSequenceCoroutine == null) _aiController.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);

        //        break;
        //}
    }

    protected override IEnumerator AttackLogic()
    {
        
        // ================================================================
        // เงื่อนไขใหม่: ถ้าเป็นการโจมตีเพราะ "ไล่นานเกินไป" (Chase Timeout)
        // ================================================================
        if (_bearAI != null && _bearAI.isChaseTimeoutAttack)
        {
            // สุ่มเลือกระหว่างสกิล 1 หรือ 3
            // สร้าง Array เลข 1 กับ 3 แล้วสุ่มหยิบมา 1 ตัว
            int[] specialSkills = { 1, 3 };
            int selectedSkill = specialSkills[UnityEngine.Random.Range(0, specialSkills.Length)];

            // ** แทรก Logic เสกเห็ดตรงนี้ **
            CheckAndSpawnMushroom(selectedSkill);

            TriggerSkillUesd(selectedSkill, skillSpeedMultiplier);
            yield return enemySkills[selectedSkill].UseSkill(this.gameObject, _aiController.playerTarget, skillSpeedMultiplier);

            // จบการทำงานของ Function นี้เลย (ไม่ไปทำ Logic ข้างล่างต่อ)
            yield break;
        }

        // ================================================================
        // เงื่อนไขเดิม: การโจมตีปกติเมื่อถึงระยะ (Normal Attack Logic)
        // ================================================================

        // 2. ถ้า AI ไม่ได้สั่งบังคับ ให้สุ่ม 1 ใน 3
        //SkillType firstSkill = (SkillType)UnityEngine.Random.Range(0, 3);
        //int firstSkillIndex = UnityEngine.Random.Range(0, 4);
        int[] firstSkills = { 0 , 1, 2, 3 };
        int firstSkillIndex = firstSkills[UnityEngine.Random.Range(0, firstSkills.Length)];

        // ** แทรก Logic เสกเห็ดตรงนี้ (Skill สุ่ม) **
        CheckAndSpawnMushroom(firstSkillIndex);

        TriggerSkillUesd(firstSkillIndex, skillSpeedMultiplier);
        yield return enemySkills[firstSkillIndex].UseSkill(this.gameObject, _aiController.playerTarget, skillSpeedMultiplier);

        // 3. ใช้สกิลที่สุ่มได้คือ Skill 3 ให้ทำต่อ
        if (firstSkillIndex == 2)
        {
            //int randomSkillIndex = UnityEngine.Random.Range(0, 2);
            int[] nextSkills = { 0, 1 };
            int randomSkillIndex = nextSkills[UnityEngine.Random.Range(0, nextSkills.Length)];

            // ** แทรก Logic เสกเห็ดตรงนี้ (Combo Skill) **
            CheckAndSpawnMushroom(randomSkillIndex);

            TriggerSkillUesd(randomSkillIndex, skillSpeedMultiplier);
            yield return enemySkills[randomSkillIndex].UseSkill(this.gameObject, _aiController.playerTarget, skillSpeedMultiplier);
        }

        yield break;
    }

    //======================================

    private void CheckAndSpawnMushroom(int skillIndex)
    {
        if (mushroomPrefab == null) return;

        bool shouldSpawn = false;

        // เงื่อนไขที่ 1: ถ้าโกรธ (isAngry = true) -> เสกทุกสกิล
        if (_bearAI.isAngry)
        {
            shouldSpawn = true;
        }
        // เงื่อนไขที่ 2: ถ้าไม่โกรธ -> เสกเฉพาะตอนใช้ Skill Index 2
        else if (skillIndex == 2)
        {
            shouldSpawn = true;
        }

        if (shouldSpawn)
        {
            SpawnMushroom();
        }
    }

    private void SpawnMushroom()
    {
        // 1. สุ่มจุดรอบตัวในวงกลม 1 หน่วย แล้วคูณด้วยรัศมี (4 หน่วย)
        Vector2 randomPoint = Random.insideUnitCircle * mushroomSpawnRadius;
        Vector3 randomPos = transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);

        // 2. (Optional) ใช้ NavMesh.SamplePosition เพื่อให้แน่ใจว่าเห็ดเกิดบนพื้นและไม่จมดิน
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPos, out hit, 2.0f, NavMesh.AllAreas))
        {
            // Spawn บน NavMesh
            // แนะนำให้ใช้ ObjectPoolingManager ถ้ามี แต่ถ้าไม่มีใช้ Instantiate ไปก่อนได้ครับ
            //Instantiate(mushroomPrefab, hit.position, Quaternion.identity);
            GameObject clone = ObjectPoolingManager.Instance.Spawn(mushroomPrefab, hit.position);

        }
        else
        {
            // ถ้าหาพื้นไม่เจอ ให้ Spawn ตรงจุดที่สุ่มได้เลย (แต่อาจจะลอยหรือจม)
            //Instantiate(mushroomPrefab, randomPos, Quaternion.identity);
            GameObject clone = ObjectPoolingManager.Instance.Spawn(mushroomPrefab, randomPos);
        }
    }

    private void HandleAngryChang(bool isAngry)
    {
        if (isAngry)
        {
            // ทำงานเฉพาะตอนเริ่มโกรธ (isAngry = true) และต้องไม่ตาย
            if (!_isAngry && !_enemyHealth.isDead)
            {
                skillSpeedMultiplier = 1.2f;
                _isAngry = true;
                // 1. หยุดการโจมตีเดิมทิ้งทันที (ถ้ากำลังร่ายสกิลอื่นอยู่)
                if (_attackSequenceCoroutine != null) StopCoroutine(_attackSequenceCoroutine);

                // 2. บังคับ AI เปลี่ยนเป็น Attack State ทันที (เพื่อให้หยุดเดิน)
                _aiController.TriggerChangeState(BaseEnemyAI.EnemyState.Attack);

                // 3. เริ่มการโจมตีสวนกลับด้วย Skill 2 ทันที
                _attackSequenceCoroutine = StartCoroutine(ForceSkill2CounterAttack());
            }
        }
        else
        {
            if (_isAngry && !_enemyHealth.isDead)
            {
                skillSpeedMultiplier = 1;
                _isAngry = false;
            }
        }
    }

    private IEnumerator ForceSkill2CounterAttack()
    {
        // แจ้ง Animator ว่าใช้สกิล 2 (index 2)
        CheckAndSpawnMushroom(2);

        TriggerSkillUesd(2, skillSpeedMultiplier);

        // รอจนกว่าจะหันหน้าเสร็จ (Optional: ถ้าอยากให้หันหาคนเล่นก่อนใช้สกิลแบบเป๊ะๆ)
        // yield return FaceTargetCoroutine(_aiController.playerTarget.position);

        // *** ใช้สกิล 2 ทันที ***
        if (enemySkills.Length > 2)
        {
            yield return enemySkills[2].UseSkill(this.gameObject, _aiController.playerTarget, skillSpeedMultiplier);
        }
        else
        {
            Debug.LogError("ลืมใส่ Enemy Skills ช่องที่ 2 หรือเปล่า?");
        }

        // จบการทำงาน แจ้ง AI ว่าว่างแล้ว (AI จะกลับไป Chase หรือทำอย่างอื่นต่อ)
        TriggerAttackFinished();
        _attackSequenceCoroutine = null;
    }

}
