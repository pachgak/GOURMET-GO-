using UnityEngine;

public class MatchYRotation : MonoBehaviour
{
    [Header("ใส่ Directional Light ตรงนี้")]
    public Transform targetLight;

    void LateUpdate()
    {
        // เช็คก่อนว่ามีการใส่ Target Light ไว้หรือเปล่า จะได้ไม่เกิด Error
        if (targetLight != null)
        {
            // ดึงค่าองศาปัจจุบัน (Euler Angles) ของตัวเงามาก่อน
            Vector3 currentAngles = transform.eulerAngles;

            // ดึงค่าองศาของดวงอาทิตย์
            Vector3 targetAngles = targetLight.eulerAngles;

            // แทนที่เฉพาะค่าแกน Y ของเงา ให้เท่ากับแกน Y ของดวงอาทิตย์
            currentAngles.y = targetAngles.y;

            // อัปเดตค่าองศากลับเข้าไปที่ตัวเงา
            transform.eulerAngles = currentAngles;
        }
    }
}