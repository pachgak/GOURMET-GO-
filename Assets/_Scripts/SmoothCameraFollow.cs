using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraControllerManager : MonoBehaviour
{
    public static CameraControllerManager instance;

    public Transform target; // ตัวละครที่เราต้องการให้กล้องติดตาม
    public float smoothTime = 0.1F;
    private Vector3 velocity = Vector3.zero;

    public Vector3 offset = new Vector3(0,7,-10);

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
        JumpToTarget();

        // คำนวณระยะห่างเริ่มต้นระหว่างกล้องกับตัวละคร
        //offset = transform.position - target.position;
        //offset = new Vector3(0, transform.position.y, 0);
    }

    void Update()
    {
        // คำนวณตำแหน่งเป้าหมายใหม่ของกล้อง
        Vector3 targetCamPos = target.position + offset;

        // การเคลื่อนที่กล้องไปยังตำแหน่งเป้าหมายอย่างนุ่มนวล
        transform.position = Vector3.SmoothDamp(transform.position, targetCamPos, ref velocity, smoothTime);
    }

    public void JumpToTarget()
    {
        // คำนวณตำแหน่งเป้าหมายใหม่ของกล้อง
        Vector3 targetCamPos = target.position + offset;

        // กำหนดตำแหน่งกล้องใหม่ทันทีโดยไม่ใช้ SmoothDamp
        transform.position = targetCamPos;

        // **สำคัญ:** รีเซ็ต velocity เป็น Vector3.zero 
        // เพื่อป้องกันไม่ให้ SmoothDamp คำนวณความเร็วเดิมในเฟรมถัดไป
        velocity = Vector3.zero;

        //Debug.Log("Camera JUMPED to new position.");
    }
}