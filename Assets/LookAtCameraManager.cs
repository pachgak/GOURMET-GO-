using com.cyborgAssets.inspectorButtonPro;
using System;
using UnityEngine;

public class LookAtCameraManager : MonoBehaviour
{
    public Transform mainCameraTransform;

    // ������� Singleton ���������Ҷ֧����¨ҡʤ�Ի�����
    public static LookAtCameraManager instance;

    // Action ����Ѻ�觢����š����ع�ͧ���ͧ�͡�
    public Action<Quaternion> OnCameraChange;
    public Action<bool> OnCameraFollowChange;

    private Quaternion lastCameraRotation;

    private bool _isFollowingCamera = true;

    private void Awake()
    {
        // ������ҧ Singleton
        if (instance == null)
        {
            instance = this;
            // ��Ǩ�ͺ����ա��ͧ��ѡ�������
            if (Camera.main != null)
            {
                //mainCameraTransform = Camera.main.transform;
                // ��駤�ҡ����ع����������������º��º
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
        // ��Ǩ�ͺ�������¹�ŧ�ͧ�����ع�ͧ���ͧ
        if (mainCameraTransform != null)
        {
            if (mainCameraTransform.rotation != lastCameraRotation)
            {
                // ����ա������¹�ŧ ������¡�� Action ������觤�ҡ����ع�����
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