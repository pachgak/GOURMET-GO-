using System.Collections;
using UnityEngine;
using DG.Tweening; // <-- ต้องมีบรรทัดนี้

public class Teleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("ตำแหน่งปลายทางที่ผู้เล่นจะถูกวาร์ปไป")]
    public Transform destination;

    [Tooltip("ระยะเวลาดีเลย์รวม (รวมเวลาเฟด) ก่อนการวาร์ปจริง")]
    public float delayDuration = 1.0f;

    [Header("UI Control")]
    [Tooltip("CanvasGroup UI ที่ใช้บังหน้าจอ (ต้องมี CanvasGroup component)")]
    public CanvasGroup screenFader;

    [Tooltip("ระยะเวลาที่ใช้ในการเฟด (Fade Duration)")]
    public float fadeDuration = 0.5f;

    [Header("System References")]
    private CharacterController playerController;
    [SerializeField] private CameraControllerManager cameraController;

    private bool isTeleporting = false;

    private void Start()
    {
        // ... (โค้ดค้นหา CharacterController เหมือนเดิม)
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<CharacterController>();
            }
        }

        if (playerController == null)
        {
            Debug.LogError("Teleporter: ไม่พบ CharacterController ของผู้เล่น.");
        }

        // กำหนดให้ UI Fader โปร่งใสและปิดการทำงานไว้ตั้งแต่เริ่มต้น
        if (screenFader != null)
        {
            screenFader.alpha = 0f;
            screenFader.gameObject.SetActive(false);
        }

        if (cameraController == null)
        {
            cameraController = CameraControllerManager.instance;
        }
    }

    public void TeleportPlayer()
    {
        if (isTeleporting) return;

        if (playerController == null || destination == null || screenFader == null)
        {
            Debug.LogError("Teleporter: ต้องกำหนดผู้เล่น, ปลายทาง, และ CanvasGroup Fader.");
            return;
        }

        StartCoroutine(ProcessTeleport());
    }

    private IEnumerator ProcessTeleport()
    {
        isTeleporting = true;

        // 1. เปิดใช้งาน GameObject ของ Fader และเริ่มเฟดเข้าสู่สีดำ (Alpha = 1)
        screenFader.gameObject.SetActive(true);
        yield return StartCoroutine(FadeScreen(1f, fadeDuration)); // เฟดเข้าสู่ Alpha 1

        // 3. วาร์ปผู้เล่น (ดำเนินการวาร์ปในขณะที่หน้าจอเป็นสีดำทึบ)
        playerController.enabled = false;

        // ตั้งตำแหน่งใหม่ โดยยกสูงขึ้นเล็กน้อย (0.2f)
        Vector3 newPosition = new Vector3(destination.position.x, destination.position.y + 0.2f, destination.position.z);
        playerController.transform.position = newPosition;

        playerController.enabled = true;

        Debug.Log("ผู้เล่นถูกวาร์ปไปยัง: " + destination.position);

        if (cameraController != null)
        {
            cameraController.JumpToTarget();
        }

        // 2. ระยะเวลารอเพิ่มเติม (ถ้ามี) หลังจากเฟดเข้าเสร็จแล้ว
        // เช่น ถ้า delayDuration = 1.0f และ fadeDuration = 0.5f จะรอเพิ่มอีก 0.5f

        float remainingDelay = delayDuration - fadeDuration;
        if (remainingDelay > 0)
        {
            yield return new WaitForSeconds(remainingDelay);
        }

        

        // 4. รอเฟรมเดียวเพื่อให้ตำแหน่งอัปเดตก่อนเริ่มเฟดออก
        yield return null;

        // 5. เฟดออกจากสีดำ (Alpha = 0)
        yield return StartCoroutine(FadeScreen(0f, fadeDuration)); // เฟดออกสู่ Alpha 0

        // 6. ปิด GameObject ของ Fader เมื่อเฟดเสร็จ
        screenFader.gameObject.SetActive(false);

        isTeleporting = false;
    }

    /// <summary>
    /// Coroutine สำหรับการเฟด CanvasGroup ด้วย DOTween
    /// </summary>
    /// <param name="targetAlpha">ค่า Alpha ปลายทาง (0f สำหรับโปร่งใส, 1f สำหรับทึบ)</param>
    /// <param name="duration">ระยะเวลาในการเฟด</param>
    private IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        // ใช้ DOTween เพื่อทำการ Tween ค่า Alpha ของ CanvasGroup
        // SetEase(Ease.InOutSine) เพื่อให้การเฟดดูนุ่มนวล
        Tweener tween = screenFader.DOFade(targetAlpha, duration).SetEase(Ease.InOutSine);

        // รอจนกว่าการ Tween จะเสร็จสมบูรณ์
        yield return tween.WaitForCompletion();
    }
}