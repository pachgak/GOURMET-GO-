using TMPro;
using UnityEngine;

public class ActionRangeController : MonoBehaviour
{
    // กำหนดผู้เล่น (Target) ที่จะใช้ในการคำนวณระยะห่าง
    [Tooltip("ลาก GameObject ของผู้เล่นมาใส่ในช่องนี้")]
    public Transform playerTransform;

    // กำหนดระยะห่างสูงสุดที่ถือว่า 'เข้าใกล้'
    [Tooltip("ระยะห่างสูงสุดที่ผู้เล่นต้องเข้าใกล้เพื่อเรียก SetAction(true)")]
    public float activationRange = 3.0f;

    // สถานะปัจจุบัน: ผู้เล่นอยู่ในระยะหรือไม่
    public bool isInRange = false;
    public float distance;

    public TMP_Text text;

    // ----------------------------------------------------------------------
    // ส่วนของการตั้งค่า (Setup)
    // ----------------------------------------------------------------------

    void Start()
    {

        text = GetComponent<TMP_Text>();

        // ตรวจสอบว่าได้กำหนด playerTransform แล้วหรือไม่
        if (playerTransform == null)
        {
            // พยายามค้นหา Object ที่มีแท็ก "Player"
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogError("กรุณากำหนด Player Transform ใน Inspector หรือตรวจสอบว่าผู้เล่นมี Tag เป็น 'Player'!");
                enabled = false; // ปิดสคริปต์เพื่อป้องกัน Error
            }
        }
        
        if(Vector3.Distance(transform.position, playerTransform.position) > activationRange) text.enabled = false;
    }

    // ----------------------------------------------------------------------
    // ส่วนของการตรวจสอบระยะห่าง (Continuous Check)
    // ----------------------------------------------------------------------

    void Update()
    {
        // คำนวณระยะห่างระหว่าง Object นี้ (this.transform) กับผู้เล่น
        distance = Vector3.Distance(transform.position, playerTransform.position);

        // กรณีที่ 1: ผู้เล่นเข้าสู่ระยะที่กำหนด
        if (distance <= activationRange && !isInRange)
        {
            isInRange = true;
            text.enabled = true;
            Debug.Log("ผู้เล่น **เข้าใกล้** ระยะแล้ว! SetAction(true)");
        }
        // กรณีที่ 2: ผู้เล่นออกจากระยะที่กำหนด
        else if (distance > activationRange && isInRange)
        {
            isInRange = false;
            text.enabled = false;
            Debug.Log("ผู้เล่น **ออกจาก** ระยะแล้ว! SetAction(false)");
        }
    }

    // ----------------------------------------------------------------------
    // ส่วนของ Action ที่ต้องการให้เกิดขึ้น
    // ----------------------------------------------------------------------

    
}