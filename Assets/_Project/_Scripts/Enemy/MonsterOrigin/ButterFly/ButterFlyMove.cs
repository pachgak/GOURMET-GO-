using UnityEngine;

public class ButterFlyMove : BaseEnemyMovement
{
    protected override void OnEnable()
    {
        base.OnEnable();

        if (_enemyHealth != null) _enemyHealth.OnTakeDamage += resetWhaitTime;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (_enemyHealth != null) _enemyHealth.OnTakeDamage -= resetWhaitTime;
    }

    private void resetWhaitTime(float none, GameObject customHitVFX = null)
    {
        _timerWaiting = 0f;
    }
}
