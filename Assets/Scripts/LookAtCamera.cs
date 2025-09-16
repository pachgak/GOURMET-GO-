using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCameraTransform;
    private Quaternion originalRotation;
    private bool isFollowingCamera = true;

    private void Awake()
    {
        // เก็บค่าการหมุนเริ่มต้นไว้
        originalRotation = transform.rotation;

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("ไม่พบกล้องหลักใน Scene! โปรดตรวจสอบว่ามีกล้องหลักและมี Tag 'MainCamera' ถูกต้อง");
        }
    }

    private void OnEnable()
    {
        // สมัครรับฟัง Action จาก LookAtCameraManager
        if (LookAtCameraManager.instance != null)
        {
            LookAtCameraManager.instance.OnCameraChange += HandleCameraChange;
            LookAtCameraManager.instance.OnCameraFollowChange += HandleCameraFollowChange;
        }
    }

    private void HandleCameraFollowChange(bool obj)
    {
        ToggleCameraFollow(obj);
    }

    private void OnDisable()
    {
        // ยกเลิกการรับฟังเมื่อสคริปต์ถูกปิดการใช้งาน
        if (LookAtCameraManager.instance != null)
        {
            LookAtCameraManager.instance.OnCameraChange -= HandleCameraChange;
            LookAtCameraManager.instance.OnCameraFollowChange -= HandleCameraFollowChange;
        }
    }

    private void HandleCameraChange(Quaternion cameraRotation)
    {
        // ถ้าอยู่ในโหมดตามกล้อง ให้หมุน GameObject
        if (isFollowingCamera)
        {
            transform.LookAt(transform.position + cameraRotation * Vector3.forward, cameraRotation * Vector3.up);
            //transform.rotation = Quaternion.Euler(cameraRotation.eulerAngles.x, cameraRotation.eulerAngles.y, 0f);
        }
    }

    // เมธอดสำหรับสลับโหมดการทำงาน
    public void ToggleCameraFollow(bool enableFollow)
    {
        isFollowingCamera = enableFollow;
        if (!isFollowingCamera)
        {
            // ถ้าปิดโหมดตามกล้อง ให้กลับไปที่การหมุนเริ่มต้น
            transform.rotation = originalRotation;
        }
    }
}