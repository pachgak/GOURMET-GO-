using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NoneAttackEnemyCombat : BaseEnemyCombat
{
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

    protected override void Update()
    {
        if (_enemyHealth != null && _enemyHealth.isDead) return;
        switch (_aiController.currentState)
        {
            case BaseEnemyAI.EnemyState.Attack:
                if (_attackSequenceCoroutine == null) _aiController.TriggerChangeState(BaseEnemyAI.EnemyState.Chase);
                break;

        }
    }
}
