using com.cyborgAssets.inspectorButtonPro;
using System;
using UnityEngine;

public class LookAtCameraManager : MonoBehaviour
{
    public Transform mainCameraTransform;

    // ทำให้เป็น Singleton เพื่อให้เข้าถึงได้ง่ายจากสคริปต์อื่น
    public static LookAtCameraManager instance;

    // Action สำหรับส่งข้อมูลการหมุนของกล้องออกไป
    public Action<Quaternion> OnCameraChange;
    public Action<bool> OnCameraFollowChange;

    private Quaternion lastCameraRotation;

    private bool _isFollowingCamera = true;

    private void Awake()
    {
        // การสร้าง Singleton
        if (instance == null)
        {
            instance = this;
            // ตรวจสอบว่ามีกล้องหลักหรือไม่
            if (Camera.main != null)
            {
                //mainCameraTransform = Camera.main.transform;
                // ตั้งค่าการหมุนเริ่มต้นเพื่อใช้เปรียบเทียบ
                //lastCameraRotation = mainCameraTransform.rotation;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        // ตรวจสอบการเปลี่ยนแปลงของการหมุนของกล้อง
        if (mainCameraTransform != null)
        {
            if (mainCameraTransform.rotation != lastCameraRotation)
            {
                // ถ้ามีการเปลี่ยนแปลง ให้เรียกใช้ Action พร้อมส่งค่าการหมุนใหม่ไป
                OnCameraChange?.Invoke(mainCameraTransform.rotation);
                lastCameraRotation = mainCameraTransform.rotation;
            }
        }
    }

    public void CameraFollow(bool isState)
    {
        OnCameraFollowChange?.Invoke(isState);

        OnCameraChange?.Invoke(mainCameraTransform.rotation);
    }

    [ProButton]
    public void TogleCameraFollow()
    {
        _isFollowingCamera = !_isFollowingCamera;
        CameraFollow(_isFollowingCamera);
    }
}