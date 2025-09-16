using UnityEngine;
using System;

public class PlayerAimGuide : MonoBehaviour
{
    private Vector3 _mousePosition;

    private void OnEnable()
    {
        // รับฟัง Event จาก PlayerInputActionsManager เพื่อรับตำแหน่งเมาส์
        PlayerInputActionsManager.instance.OnMountPosition += HandleMountPosition;
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnMountPosition -= HandleMountPosition;
    }

    private void HandleMountPosition(Vector3 position)
    {
        // เก็บตำแหน่งเมาส์ล่าสุด
        _mousePosition = position;
    }

    private void Update()
    {
        // หากตำแหน่งเมาส์มีค่า (มีการคลิกเกิดขึ้น)
        if (_mousePosition != Vector3.zero)
        {
            // คำนวณทิศทางจากตัวไกด์ไปยังตำแหน่งเมาส์
            Vector3 direction = _mousePosition - transform.position;

            // ทำให้ทิศทางอยู่บนระนาบแนวนอนเท่านั้น (แกน X, Z)
            direction.y = 0;

            // หากทิศทางมีขนาดมากกว่า 0 ให้ทำการหมุน
            if (direction != Vector3.zero)
            {
                // หมุน GameObject ของไกด์ให้หันไปตามทิศทางที่คำนวณได้
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}