using System;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCameraTransform;
    private Quaternion originalRotation;
    private bool isFollowingCamera = true;

    private void Awake()
    {
        // �纤�ҡ����ع����������
        originalRotation = transform.rotation;

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("��辺���ͧ��ѡ� Scene! �ô��Ǩ�ͺ����ա��ͧ��ѡ����� Tag 'MainCamera' �١��ͧ");
        }
    }

    private void OnEnable()
    {
        // ��Ѥ��Ѻ�ѧ Action �ҡ LookAtCameraManager
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
        // ¡��ԡ����Ѻ�ѧ�����ʤ�Ի��١�Դ�����ҹ
        if (LookAtCameraManager.instance != null)
        {
            LookAtCameraManager.instance.OnCameraChange -= HandleCameraChange;
            LookAtCameraManager.instance.OnCameraFollowChange -= HandleCameraFollowChange;
        }
    }

    private void HandleCameraChange(Quaternion cameraRotation)
    {
        // ������������������ͧ �����ع GameObject
        if (isFollowingCamera)
        {
            transform.LookAt(transform.position + cameraRotation * Vector3.forward, cameraRotation * Vector3.up);
            //transform.rotation = Quaternion.Euler(cameraRotation.eulerAngles.x, cameraRotation.eulerAngles.y, 0f);
        }
    }

    // ���ʹ����Ѻ��Ѻ������÷ӧҹ
    public void ToggleCameraFollow(bool enableFollow)
    {
        isFollowingCamera = enableFollow;
        if (!isFollowingCamera)
        {
            // ��һԴ����������ͧ ����Ѻ价������ع�������
            transform.rotation = originalRotation;
        }
    }
}