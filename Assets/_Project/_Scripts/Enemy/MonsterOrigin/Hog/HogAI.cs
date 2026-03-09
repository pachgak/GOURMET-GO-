using UnityEngine;
using System.Collections;

public class HogAI : BaseEnemyAI
{
    // Override Update เพื่อเช็คว่าถ้ามึนอยู่ ห้ามคิดอะไร (หยุดทำงานชั่วคราว)
    protected override void Update()
    {
        if (IsStunned) return;

        base.Update();
    }
}