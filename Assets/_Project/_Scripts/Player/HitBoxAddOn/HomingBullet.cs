using UnityEngine;

public class HomingBullet : MonoBehaviour, ISpeed, ITargetable
{
    [Header("Movement Settings")]
    [Tooltip("ความเร็วในการพุ่งไปข้างหน้า")]
    public float speed = 10f;
    [Tooltip("ความเร็วในการเลี้ยว (หันพวงมาลัย) ยิ่งเยอะยิ่งเลี้ยวคม")]
    public float turnSpeed = 10f;

    [Header("Movement Restrictions")]
    [Tooltip("ติ๊กถูกเพื่อบังคับให้เลี้ยวแค่บนพื้น (X, Z)")]
    public bool flatMovement = true;

    [Header("Stop Settings")]
    [Tooltip("ระยะห่างที่จะให้สายฟ้าหยุดซ้อนทับกับ Player (แนะนำ 0.2 - 0.5)")]
    public float stopDistance = 0.1f;

    [SerializeField] private Transform target;

    public float _speed { get => speed; set => speed = value; }

    public void SetTarget(Transform targetTransform)
    {
        this.target = targetTransform;
    }

    private void Update()
    {
        if (target != null)
        {
            // 1. หาความห่างและทิศทาง
            Vector3 directionToTarget = target.position - transform.position;

            if (flatMovement)
            {
                directionToTarget.y = 0;
            }

            float currentDistance = directionToTarget.magnitude;

            // 2. เช็คว่า "ผู้เล่นอยู่ไกลกว่าระยะหยุดไหม?"
            if (currentDistance > stopDistance)
            {
                // --- จังหวะไล่ล่า (ผู้เล่นเดินหนี หรือยังเข้าไม่ถึงตัว) ---
                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }

                // สั่งพุ่งไปข้างหน้า
                transform.position += transform.forward * speed * Time.deltaTime;
            }
            else
            {
                // --- จังหวะซ้อนทับ (ผู้เล่นยืนนิ่งๆ) ---
                // ค่อยๆ หันหน้าจ้องผู้เล่นไว้เสมอ (เผื่อผู้เล่นวิ่งสวน มันจะได้หันหน้าเตรียมพุ่งตามถูกทิศ)
                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }

                // *** สังเกตว่าเราไม่สั่ง transform.position ให้พุ่งไปข้างหน้าแล้ว มันเลยหยุดนิ่งซ้อนกับตัว Player พอดี ***
                // และถ้าเฟรมถัดไป Player เดินหนีจน currentDistance > stopDistance มันก็จะกลับไปเข้าเงื่อนไขบนอัตโนมัติ!
            }
        }
        else
        {
            // ถ้าเป้าหมายหายไป (เช่น Player ตาย) ให้มันพุ่งตรงลอยๆ ต่อไปจนกว่าจะหมดเวลา
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}