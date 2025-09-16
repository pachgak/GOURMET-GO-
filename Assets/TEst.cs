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
        Debug.Log("กำลังทำงาน...");

        // ถ้าเงื่อนไขเป็นจริง ให้หยุด Coroutine ทันที
        if (someCondition == true)
        {
            yield break; // Coroutine นี้จะจบลงที่นี่
        }

        // โค้ดด้านล่างนี้จะไม่ถูกเรียกใช้ถ้า someCondition เป็นจริง
        Debug.Log("ทำงานต่อ... เพราะเงื่อนไขไม่เป็นจริง");
    }
}
