using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // ����Ф÷����ҵ�ͧ��������ͧ�Դ���
    public float smoothTime = 0.1F;
    private Vector3 velocity = Vector3.zero;

    public Vector3 offset = new Vector3(0,7,-10);

    void Start()
    {
        // �ӹǳ������ҧ������������ҧ���ͧ�Ѻ����Ф�
        //offset = transform.position - target.position;
        //offset = new Vector3(0, transform.position.y, 0);
    }

    void Update()
    {
        // �ӹǳ���˹������������ͧ���ͧ
        Vector3 targetCamPos = target.position + offset;

        // �������͹�����ͧ��ѧ���˹�����������ҧ�������
        transform.position = Vector3.SmoothDamp(transform.position, targetCamPos, ref velocity, smoothTime);
    }
}