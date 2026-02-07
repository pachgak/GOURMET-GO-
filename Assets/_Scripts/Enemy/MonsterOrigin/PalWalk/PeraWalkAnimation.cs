using UnityEngine;
using UnityEngine.AI;

public class PeraWalkAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        // 2. ดึงความเร็วในพิกัดโลก (World Space Velocity)
        Vector3 worldVelocity = _agent.velocity;

        float totalSpeed = worldVelocity.magnitude;

        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity.normalized);
        // 4b. ดึงค่าสำหรับ Blend Tree (แกน X คือด้านข้าง, แกน Z คือเดินหน้า/ถอยหลัง)
        // ใช้ Math.Clamp เพื่อจำกัดค่าให้อยู่ระหว่าง -1 ถึง 1
        float moveX = localVelocity.x; // ด้านข้าง (Strafe Left/Right)
        float moveZ = localVelocity.z; // เดินหน้า/ถอยหลัง (Forward/Backward)

        if(moveX > 0) spriteRenderer.flipX = true;
        if(moveX < 0) spriteRenderer.flipX = false;
        //spriteRenderer.flipX = (moveX >= 0) ? true : false;
    }
}
