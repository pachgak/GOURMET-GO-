using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour , IKnockbackable
{
    public bool canMove;
    [Header("Enemy Stats")]
    [SerializeField] private float lookRadius = 10f; // ระยะการมองเห็นของผู้เล่น

    //[SerializeField] private float StillThreshold = 0.05f;
    //[SerializeField] private float MaxKnockbackTime = 0.5f;
    [SerializeField] private float knockbackMultiplier = 1f;
    float IKnockbackable._knockbackMultiplier { get => knockbackMultiplier; set => knockbackMultiplier = value; }
    //[SerializeField] private float knockbackTime = 0.15f;

    [SerializeField] private Transform target; // เป้าหมาย (ผู้เล่น)
    [SerializeField] private NavMeshAgent agent; // Component NavMeshagent
    private Rigidbody rb;

    [SerializeField] private bool canKnockback = true;
    bool IKnockbackable._canKnockback { get => canKnockback; set => canKnockback = value; }

    [SerializeField] private Coroutine KnockbackCoroutine;

    protected virtual void Start()
    {
        // หา GameObject ของผู้เล่น
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        else
        {
            Debug.LogWarning("PlayerMovement instance not found. Enemy will not have a target.");
        }

        // รับ NavMeshagent Component
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        if (target == null || !agent.enabled || !canMove) return;

        // คำนวณระยะห่างระหว่าง Enemy กับ ผู้เล่น
        float distance = Vector3.Distance(target.position, transform.position);

        // ถ้าผู้เล่นอยู่ในระยะมองเห็น
        if (distance <= lookRadius)
        {
            // ให้ NavMeshagent เคลื่อนที่ไปหาผู้เล่น
            agent.SetDestination(target.position);

            // ถ้าผู้เล่นอยู่ในระยะหยุดแล้ว
            if (distance <= agent.stoppingDistance)
            {
                // โค้ดสำหรับการโจมตีหรือการกระทำอื่น ๆ ที่นี่
                FaceTarget();
            }
        }
    }

    // ฟังก์ชันสำหรับทำให้ Enemy หันหน้าไปหาผู้เล่น
    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void GetKnockedBack(Vector3 direction, float force, float time)
    {

        Debug.Log($"direction : {direction} | force : {force} | time : {time}");

        if (!canKnockback) return;

        if(KnockbackCoroutine != null) StopCoroutine(KnockbackCoroutine);
        KnockbackCoroutine = StartCoroutine(ApplyKnockback(direction,force, time));
    }

    private IEnumerator ApplyKnockback(Vector3 direction, float force, float time)
    {
        /*
        //Debug.Log($"ApplyKnockback : {direction} | {force}");

        //yield return null;
        //agent.enabled = false;
        //rb.useGravity = false;
        //rb.isKinematic = false;

        //rb.AddForce(direction * force, ForceMode.Impulse);

        //yield return new WaitForFixedUpdate();
        //float knockbackTime = Time.time;
        //yield return new WaitUntil(
        //    () => rb.linearVelocity.magnitude < StillThreshold || Time.time > knockbackTime + MaxKnockbackTime
        //);
        //yield return new WaitForSeconds(0.25f);

        //rb.linearVelocity = Vector3.zero;
        //rb.angularVelocity = Vector3.zero;
        //rb.useGravity = false;
        //rb.isKinematic = true;
        //agent.Warp(transform.position);
        //agent.enabled = true;

        //yield return null;


        ////กลับไป stest เดิน
        ////if (Player != null)
        ////{
        ////    KnockbackCoroutine = StartCoroutine(ChasePlayer(Player));
        ////}
        ////else
        ////{
        ////    KnockbackCoroutine = StartCoroutine(Roam());
        ////}
        ///
        // 1. คำนวณแรงตาม Multiplier ของศัตรูตัวนี้

        float finalForce = force * knockbackMultiplier;

        // ถ้า multiplier เป็น 0 หรือแรงเป็น 0 ไม่ต้องทำอะไร
        if (finalForce <= 0) yield break;

        agent.enabled = false;
        rb.isKinematic = false;
        // ปิด Gravity ชั่วคราวเพื่อให้กระเด็นในแนวราบได้อย่างแม่นยำ
        rb.useGravity = false;

        float timer = 0;
        Vector3 knockbackVelocity = direction.normalized * finalForce;

        while (timer < time)
        {
            // คุมความเร็วให้คงที่ตลอดเวลาที่กำหนด
            rb.linearVelocity = knockbackVelocity;

            timer += Time.deltaTime;
            yield return null;
        }

        // จบการพุ่ง
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // วาง Agent ลงตำแหน่งปัจจุบันก่อนเปิดใช้งาน
        agent.Warp(transform.position);
        agent.enabled = true;

        KnockbackCoroutine = null;

        Debug.Log("Eemy ApplyKnockback");
        */
        // 1. คำนวณแรงตาม Multiplier (ความหนักเบาของมอนสเตอร์)
        float finalForce = force * knockbackMultiplier;
        if (finalForce <= 0) yield break;

        agent.enabled = false;
        rb.isKinematic = false;
        rb.useGravity = true; // เปิดไว้เพื่อให้ดูสมจริงเวลาโดนดีด

        // 2. ใช้ AddForce แบบ Impulse (ดีดทีเดียวแล้วปล่อยให้ Drag ทำงาน)
        // หรือจะใช้ ForceMode.VelocityChange เพื่อไม่ให้มวล (Mass) มาเกี่ยว
        rb.AddForce(direction.normalized * finalForce, ForceMode.Impulse);

        // 3. ช่วงเวลาที่ "เสียหลัก" (Stun/Knockback Time)
        // แทนที่จะรอจนนิ่ง (StillThreshold) เราจะรอตาม duration ที่ส่งมา
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 4. การหยุด (เพื่อให้กลับไปใช้ NavMesh ได้อย่างปลอดภัย)
        // ค่อยๆ ลดความเร็วที่เหลืออยู่ให้เป็น 0
        float stopTimer = 0;
        Vector3 currentVel = rb.linearVelocity;
        while (stopTimer < 0.1f) // ใช้เวลา 0.1 วิในการเบรกให้สนิท
        {
            rb.linearVelocity = Vector3.Lerp(currentVel, Vector3.zero, stopTimer / 0.1f);
            stopTimer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        agent.Warp(transform.position);
        agent.enabled = true;

        KnockbackCoroutine = null;
    }

    void OnDrawGizmosSelected()
    {
        // วาดวงกลมสีแดงใน Scene เพื่อให้มองเห็นระยะการมองเห็นของ Enemy
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }

}