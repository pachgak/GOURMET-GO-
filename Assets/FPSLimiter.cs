using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    void Awake()
    {
        // ��˹� Target Frame Rate ����ͧ���
        Application.targetFrameRate = 120; // ���ͤ�ҷ��س��ͧ��� �� 30, 90, 120
    }
}