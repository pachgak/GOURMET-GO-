using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraControllerManager : MonoBehaviour
{
    public static CameraControllerManager instance;

    [Tooltip("ตัวละครที่เราต้องการให้กล้องติดตาม (ถ้าปล่อยว่าง จะพยายามหา Tag 'Player' ให้อัตโนมัติ)")]
    public Transform target;
    public float smoothTime = 0.1F;
    private Vector3 velocity = Vector3.zero;

    public Vector3 offset = new Vector3(0, 7, -10);

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        // 1. --- ดักจับ Null Reference และพยายามหา Target อัตโนมัติ ---
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
                Debug.Log("CameraControllerManager: ดึง Target จาก Tag 'Player' ให้อัตโนมัติแล้วครับ");
            }
            else
            {
                Debug.LogWarning("CameraControllerManager: หา Target ไม่เจอ! โปรดลากใส่ Inspector หรือใส่ Tag 'Player' ให้ตัวละครด้วยครับ");
                return; // ถ้าหาไม่เจอจริงๆ ให้หยุดทำงานไปก่อน จะได้ไม่ฟ้อง Error เพิ่ม
            }
        }

        JumpToTarget();
    }

    void LateUpdate() // แนะนำให้ใช้ LateUpdate สำหรับกล้องครับ
    {
        // 2. --- ดักจับ Null Reference ในลูป เพื่อกัน Error รัวๆ ---
        if (target == null) return;

        // คำนวณตำแหน่งเป้าหมายใหม่ของกล้อง
        Vector3 targetCamPos = target.position + offset;

        // การเคลื่อนที่กล้องไปยังตำแหน่งเป้าหมายอย่างนุ่มนวล
        transform.position = Vector3.SmoothDamp(transform.position, targetCamPos, ref velocity, smoothTime);
    }

    public void JumpToTarget()
    {
        // 3. --- ดักจับ Null Reference ก่อนสั่งกระโดด ---
        if (target == null) return;

        // คำนวณตำแหน่งเป้าหมายใหม่ของกล้อง
        Vector3 targetCamPos = target.position + offset;

        // กำหนดตำแหน่งกล้องใหม่ทันทีโดยไม่ใช้ SmoothDamp
        transform.position = targetCamPos;

        // **สำคัญ:** รีเซ็ต velocity เป็น Vector3.zero 
        // เพื่อป้องกันไม่ให้ SmoothDamp คำนวณความเร็วเดิมในเฟรมถัดไป
        velocity = Vector3.zero;
    }
}