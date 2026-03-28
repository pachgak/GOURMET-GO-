using System.Collections.Generic; // อย่าลืม using System.Collections.Generic;
using UnityEngine;

public class ShamakiriCloneAI : BaseEnemyAI
{
    [Header("Graphics Reference")]
    public List<GameObject> graphicsParents; // เปลี่ยนเป็น List สำหรับซ่อนหลายๆ ชิ้น (โมเดล, เงา)
    public Collider mainCollider;

    // =========================================================
    // เอาไว้เปิด/ปิด การมองเห็นและการชน
    // =========================================================
    public void SetVisibility(bool isVisible)
    {
        if (graphicsParents != null && graphicsParents.Count > 0)
        {
            foreach (var gfx in graphicsParents)
            {
                if (gfx != null) gfx.SetActive(isVisible);
            }
        }
        if (mainCollider != null) mainCollider.enabled = isVisible;
    }

    // =========================================================
    // 1. ล้างสมองตอนเกิด
    // =========================================================
    protected override void Start()
    {
        ChangeState(EnemyState.Standby);
        SetVisibility(true); // ป้องกันบั๊กดึงมาจาก Pool แล้วล่องหนอยู่
    }

    protected override void StandbyChangeStateLogic() { }

    protected override void ChaseChangeStateLogic()
    {
        if (_playerInAttackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        else if (playerTarget != null)
        {
            TriggerStartChase(playerTarget.position);
        }
    }

    protected override void HandleAttackFinished()
    {
        ChangeState(EnemyState.Standby);
    }
}