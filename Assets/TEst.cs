using System.Collections;
using UnityEngine;

public class TEst : MonoBehaviour
{
    public bool someCondition;
    private float currentSpeed;
    private float sprintAcceleration;
    private float targetSpeed;

    public float aSpeed = 1;
    public float bSpeed = 10;
    public float cSpeed;
    public float speedTime = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cSpeed = Mathf.Lerp(aSpeed, bSpeed, speedTime);
        //currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * sprintAcceleration);
    }

    private IEnumerator CheckForCondition()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("���ѧ�ӧҹ...");

        // ������͹��繨�ԧ �����ش Coroutine �ѹ��
        if (someCondition == true)
        {
            yield break; // Coroutine ���Ш�ŧ�����
        }

        // �鴴�ҹ��ҧ�������١���¡���� someCondition �繨�ԧ
        Debug.Log("�ӧҹ���... �������͹�����繨�ԧ");
    }
}
