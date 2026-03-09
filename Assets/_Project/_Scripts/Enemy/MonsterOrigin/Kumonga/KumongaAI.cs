using UnityEngine;
using System.Collections;

public class KumongaAI : BaseEnemyAI
{

    protected override void Update()
    {
        // ถ้ามึนอยู่ ห้ามคิด ห้ามเปลี่ยน State ห้ามเดิน
        if (IsStunned) return;

        // ถ้าไม่มึน ก็รัน AI ปกติ (ไล่ล่า / เล็ง / โจมตี)
        base.Update();
    }
}