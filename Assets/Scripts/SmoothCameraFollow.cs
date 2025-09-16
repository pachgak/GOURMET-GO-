using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // ตัวละครที่เราต้องการให้กล้องติดตาม
    public float smoothTime = 0.1F;
    private Vector3 velocity = Vector3.zero;

    public Vector3 offset = new Vector3(0,7,-10);

    void Start()
    {
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
}