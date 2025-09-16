using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public float dashOnAttackDistance = 1.5f; // ���зҧ����ͧ���������
    public float dashOnAttackTime = 0.1f;    // �������ҷ����㹡�þ��
    public float attackStateChangeTime = 0.1f;    // �������ҷ����㹡�þ��

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
        // ��ҡ��ѧ�����������
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
        // ���ѭ�ҳ�����ش��������
        PlayerInputActionsManager.instance.OnAttackStateChange?.Invoke(false);
    }

    private void HandleMeleeAttack(Vector3 mousePosition)
    {
        if (isAttacking) return; // ��ͧ�ѹ������ի�ӫ�͹��о������

        Debug.Log("Player is attacking and dashing!");

        // ���ѭ�ҳ��ҡ��ѧ��������
        PlayerInputActionsManager.instance.OnAttackStateChange?.Invoke(true);

        // �ӹǳ��ȷҧ�ҡ���˹觼�������ѧ���˹������
        Vector3 playerPosition = new Vector2(transform.position.x, transform.position.y);
        Vector3 direction = (mousePosition - playerPosition).normalized;

        // �������þ������
        isAttacking = true;
        attackDashTimeCounter = dashOnAttackTime;

        // �ӹǳ�ç�������
        float dashForce = dashOnAttackDistance / dashOnAttackTime;
        rb.AddForce(direction * dashForce, ForceMode.Impulse);

        // �س����ö����鴡���������� �����
        // �� ������ Animator ��蹷������
        // if (animator != null)
        // {
        //     animator.SetTrigger("Attack");
        // }
    }
}