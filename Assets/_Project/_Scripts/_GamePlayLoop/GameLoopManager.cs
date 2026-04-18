using UnityEngine;
using System.Collections;
using DG.Tweening; // ถ้ามี DOTween ใช้ทำจอดำตอนวาปได้

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("Player & Locations")]
    public GameObject player;
    public Transform baseWarpPoint; // จุดที่ผู้เล่นจะไปโผล่ตอนอยู่ฐาน
    public Transform mapWarpPoint;  // จุดที่ผู้เล่นจะไปโผล่ตอนอยู่แมพฟาร์ม

    [Header("UI Transition (Option)")]
    public CanvasGroup blackScreen; // แผ่นสีดำเอาไว้บังตาตอนโหลด

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
    }

    // ==========================================
    // 1. วาปออกจากฐาน ไปลุย!
    // ==========================================
    public void GoToMap()
    {
        StartCoroutine(WarpSequence(mapWarpPoint, () =>
        {
            // พอมืดสนิท ค่อยสั่งเสกมอนสเตอร์!
            if (SpawnerManager.Instance != null)
            {
                SpawnerManager.Instance.TriggerAllSpawns();
            }
        }));
    }

    // ==========================================
    // 2. วาปกลับฐาน กลับบ้าน!
    // ==========================================
    public void ReturnToBase()
    {
        StartCoroutine(WarpSequence(baseWarpPoint, () =>
        {
            // พอมืดสนิท ค่อยสั่งลบมอนสเตอร์ทิ้งให้หมด!
            if (SpawnerManager.Instance != null)
            {
                SpawnerManager.Instance.ResetAllSpawns();
            }
        }));
    }

    // --- ระบบทำจอมืด แล้วย้ายตัวละคร ---
    private IEnumerator WarpSequence(Transform targetDestination, System.Action onMiddleOfWarp)
    {
        // 1. ปิดการบังคับผู้เล่น (เชื่อมกับ Manager ของคุณ)
        PlayerInputActionsManager.instance.playerControls.Disable();

        // 2. Fade จอดำ
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            yield return blackScreen.DOFade(1f, 0.5f).WaitForCompletion();
        }

        // 3. ทำงานตรงกลาง (เช่น เสกมอน / ลบมอน)
        onMiddleOfWarp?.Invoke();

        // ==========================================
        // 4. ย้ายตัวผู้เล่น (สำหรับ CharacterController)
        // ==========================================
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false; // ปิด CC ก่อนไม่ให้มันฝืนการย้าย

        // ย้ายตำแหน่ง และจับหันหน้าไปทางเดียวกับจุดวาปด้วยเลย!
        player.transform.position = targetDestination.position;
        player.transform.rotation = targetDestination.rotation;

        if (cc != null) cc.enabled = true;  // เปิด CC กลับมาใช้งานต่อ
        // ==========================================

        yield return new WaitForSeconds(0.5f); // รอโหลดแป๊บนึง

        // 5. Fade จอสว่าง
        if (blackScreen != null)
        {
            yield return blackScreen.DOFade(0f, 0.5f).WaitForCompletion();
            blackScreen.gameObject.SetActive(false);
        }

        // 6. คืนการบังคับ
        PlayerInputActionsManager.instance.playerControls.Enable();
    }
}