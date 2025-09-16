using UnityEngine;

public class BulletMove : MonoBehaviour , ISpeed
{
    // �������Ǣͧ����ع
    public float speed = 20f;

    // �������ҷ�����ع�з���µ���ͧ
    public float destroyTime = 10f;

    float ISpeed._speed { get => speed; set => speed = value; }

    void Start()
    {
        // ���������ع����µ���ͧ����ͼ�ҹ仵�����ҷ���˹�
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // ��������ع����͹���仢�ҧ˹�����ҧ������ͧ㹷�ȷҧ Z-axis �ͧ�ѹ
        // �¨л�Ѻ����������餧����� Time.deltaTime
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}