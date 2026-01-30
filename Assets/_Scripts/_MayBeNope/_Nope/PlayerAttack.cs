using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public float dashOnAttackDistance = 1.5f; // ระยะทางที่ต้องการให้พุ่งไป
    public float dashOnAttackTime = 0.1f;    // ระยะเวลาที่ใช้ในการพุ่ง
    public float attackStateChangeTime = 0.1f;    // ระยะเวลาที่ใช้ในการพุ่ง

    private Rigidbody rb;
    private bool isAttacking = false;
    private float attackDashTimeCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        //PlayerInputActionsManager.instance.OnMeleeAttack += HandleMeleeAttack;
    }

    private void OnDisable()
    {
        //PlayerInputActionsManager.instance.OnMeleeAttack -= HandleMeleeAttack;
    }

    private void Update()
    {
        // ถ้ากำลังพุ่งโจมตีอยู่
        if (isAttacking)
        {
            attackDashTimeCounter -= Time.deltaTime;
            if (attackDashTimeCounter <= 0)
            {
                isAttacking = false;
                //rb.velocity = Vector3.zero;

                Invoke(nameof(ReSetAttackState), attackStateChangeTime);
            }
        }
    }

    private void ReSetAttackState()
    {
        // ส่งสัญญาณว่าหยุดโจมตีแล้ว
        PlayerInputActionsManager.instance.OnAttackStateChange?.Invoke(false);
    }

    private void HandleMeleeAttack(Vector3 mousePosition)
    {
        if (isAttacking) return; // ป้องกันการโจมตีซ้ำซ้อนขณะพุ่งอยู่

        Debug.Log("Player is attacking and dashing!");

        // ส่งสัญญาณว่ากำลังโจมตีอยู่
        PlayerInputActionsManager.instance.OnAttackStateChange?.Invoke(true);

        // คำนวณทิศทางจากตำแหน่งผู้เล่นไปยังตำแหน่งเมาส์
        Vector3 playerPosition = new Vector2(transform.position.x, transform.position.y);
        Vector3 direction = (mousePosition - playerPosition).normalized;

        // เริ่มการพุ่งโจมตี
        isAttacking = true;
        attackDashTimeCounter = dashOnAttackTime;

        // คำนวณแรงที่ใช้พุ่ง
        float dashForce = dashOnAttackDistance / dashOnAttackTime;
        rb.AddForce(direction * dashForce, ForceMode.Impulse);

        // คุณสามารถใส่โค้ดการโจมตีอื่นๆ ที่นี่
        // เช่น สั่งให้ Animator เล่นท่าโจมตี
        // if (animator != null)
        // {
        //     animator.SetTrigger("Attack");
        // }
    }
}