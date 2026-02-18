using UnityEngine;
using System.Collections;

public class HogAI : BaseEnemyAI
{
    [Header("Hog Settings")]
    public float stunDuration = 3.0f; // ระยะเวลาที่จะมึนหลังจากชนกำแพง

    [SerializeField] private bool _isStunned = false; // เอาไว้ดูสถานะใน Inspector
    private Coroutine _stunCoroutine;

    // Override Update เพื่อเช็คว่าถ้ามึนอยู่ ห้ามคิดอะไร (หยุดทำงานชั่วคราว)
    protected override void Update()
    {
        if (_isStunned) return;

        base.Update();
    }

    // ฟังก์ชันสั่งให้มึน (จะถูกเรียกจาก HogCombat เมื่อชนกำแพง)
    public void ApplyStun()
    {
        if (_stunCoroutine != null) StopCoroutine(_stunCoroutine);
        _stunCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        _isStunned = true;
        // สั่งหยุดเดินผ่าน Event ของ BaseEnemyAI
        TriggerStopMovement();

        Debug.Log($"<color=orange>{gameObject.name} is Stunned!</color>");

        // (Optional) ถ้ามี Animation มึน ให้ Trigger ตรงนี้
        // GetComponentInChildren<Animator>().SetTrigger("Stun");

        yield return new WaitForSeconds(stunDuration);

        _isStunned = false;

        // พอมึนเสร็จ กลับไปไล่ล่าผู้เล่นต่อทันที
        ChangeState(EnemyState.Chase);
    }
}