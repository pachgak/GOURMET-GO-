using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class MiniGameUIControler : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject miniGamePanel; // <--- เพิ่มตัวนี้ เอาไว้คุม Canvas หรือ Panel หลักของมินิเกม
    public CanvasGroup blackOverlay; // พื้นหลังดำโปร่งแสง
    public GameObject startPanel;    // หน้าจอเริ่มเกม
    public GameObject endPanel;      // หน้าจอจบเกม

    [Header("UI Elements")]
    public TMP_Text countdownText; // ข้อความ 3 2 1
    public Button startButton;            // ปุ่มเริ่ม
    public Button closeButton;            // ปุ่มปิด

    [Header("Settings")]
    public float popDuration = 0.3f; // ความเร็วตอนข้อความเด้ง

    [Header("Reward UI")]
    public Image rewardCookImage;
    public TMP_Text rewardCookCountText;

    private void Awake()
    {
        // 1. ผูกปุ่มเข้ากับฟังก์ชัน
        if (startButton != null) startButton.onClick.AddListener(StartGameSequence);
        if (closeButton != null) closeButton.onClick.AddListener(CloseMiniGame); // แก้เป็น closeButton นะครับ

        // 3. ตั้งค่าเริ่มต้น (โชว์หน้า Start ซ่อนหน้าอื่นๆ)
        ResetUI();
        miniGamePanel.SetActive(false);
    }

    private void OnEnable()
    {
        // 2. ดักฟัง Event ตอนจบเกมจาก Manager
        if (MiniGameFManager.Instance != null)
        {
            MiniGameFManager.Instance.OnGameFinished += HeadleGameFinished;
        }
    }

    private void OnDestroy()
    {
        if (MiniGameFManager.Instance != null)
        {
            MiniGameFManager.Instance.OnGameFinished -= HeadleGameFinished;
        }
    }

    private void HeadleGameFinished(Sprite rewardSprite, int cookCount)
    {
        ShowEndScreen(rewardSprite, cookCount);
    }

    private void ResetUI()
    {
        blackOverlay.alpha = 1f;
        blackOverlay.gameObject.SetActive(true);

        if (startPanel != null) startPanel.SetActive(false); // ซ่อนหน้า Start เพราะเราเริ่มออโต้
        endPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);

        Color c = countdownText.color;
        c.a = 1f;
        countdownText.color = c;
    }

    [ContextMenu("StartGameSequence")]
    // --- ลำดับการเริ่มเกม ---
    public void StartGameSequence()
    {
        if (startPanel != null) startPanel.SetActive(false); // ซ่อนปุ่มเริ่ม
        StartCoroutine(CountdownRoutine()); // เริ่มนับถอยหลัง
    }

    private IEnumerator CountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);

        // ชุดคำนวณเพื่อนับ 3 -> 2 -> 1
        string[] countWords = { "3", "2", "1", "Cook!!" };

        foreach (string word in countWords)
        {
            countdownText.text = word;

            // รีเซ็ตขนาดกลับเป็น 0 แล้วเด้ง (Pop) เป็นขนาด 1 ด้วย DOTween
            countdownText.transform.localScale = Vector3.zero;
            countdownText.transform.DOScale(Vector3.one, popDuration).SetEase(Ease.OutBack);

            // รอ 1 วินาทีก่อนเปลี่ยนคำ (ยกเว้นคำว่า Cook!! ไม่ต้องรอเต็มวิ)
            if (word != "Cook!!")
            {
                yield return new WaitForSeconds(1f);
            }
        }

        // รอค้างคำว่า Cook!! ไว้แป๊บนึงให้คนอ่านทัน
        yield return new WaitForSeconds(0.5f);

        // --- ทำ Transition Fade หายไปพร้อมกัน ---
        // 1. เฟดตัวอักษรหายไป
        countdownText.DOFade(0f, 0.5f);

        Debug.Log($"blackOverlay {Time.time}");

        // 2. เฟดพื้นหลังดำหายไป และบังคับให้ Coroutine "หยุดรอจนกว่าจะเฟดเสร็จ"
        yield return blackOverlay.DOFade(0f, 0.5f).WaitForCompletion();

        // บรรทัดข้างล่างนี้จะทำงานก็ต่อเมื่อผ่านไปแล้ว 0.5 วินาที (รอเฟดเสร็จ)
        // ปิด Object เมื่อเฟดเสร็จ เพื่อไม่ให้มันบังการกดฟันของเกม
        blackOverlay.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        Debug.Log($"Complaet blackOverlay {Time.time}");

        yield return new WaitForSeconds(1f);

        Debug.Log($"StartGame {Time.time}");

        // ! สั่งเริ่มเกมของจริง !
        MiniGameFManager.Instance.StartGame();
    }

    // --- ลำดับตอนจบเกม ---
    private void ShowEndScreen(Sprite rewardSprite, int cookCount)
    {
        StartCoroutine(CountdownEndScreen(rewardSprite,cookCount)); // เริ่มนับถอยหลัง
    }

    private IEnumerator CountdownEndScreen(Sprite rewardSprite, int cookCount)
    {
        yield return new WaitForSeconds(1.5f);

        if (rewardCookImage != null && rewardSprite != null)
        {
            rewardCookImage.sprite = rewardSprite;

            // สั่งให้ขนาดภาพพอดีกับต้นฉบับ (จะได้ไม่เบี้ยว)
            //rewardCookImage.SetNativeSize();
        }

        if (rewardCookCountText != null)
        {
            rewardCookCountText.text = (cookCount > 1) ? $"x{cookCount}" : "";
        }

        // โชว์พื้นหลังดำและเฟดความมืดกลับมา
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.DOFade(1f, 0.5f);

        yield return new WaitForSeconds(0.5f);

        // โชว์หน้าจอ End Panel และทำให้มันเด้งขึ้นมา
        endPanel.SetActive(true);
        endPanel.transform.localScale = Vector3.zero;
        endPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetDelay(0.3f);
    }

    // --- ฟังก์ชันใหม่: เอาไว้ให้คนอื่นเรียกเพื่อ "เปิด" มินิเกม ---
    public void OpenMiniGame()
    {
        // 1. เปิดเฉพาะตัว UI Panel (ตัวสคริปต์จะได้ไม่ต้องโดนปิดๆ เปิดๆ)
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(true);
        }

        // 2. จัดระเบียบหน้าจอให้สะอาด
        ResetUI();

        // 3. สั่งเริ่มนับถอยหลัง 3 2 1 ทันที
        StartGameSequence();
    }

    [ContextMenu("CloseMiniGame")]
    // --- ตอนกดปุ่มปิดเกม ---
    private void CloseMiniGame()
    {
        Debug.Log("ปิดมินิเกม กลับหน้าหลัก หรือ ปิด UI นี้ทิ้ง");

        ResetUI(); // เคลียร์ค่าเผื่อเปิดรอบหน้า

        // ปิดการแสดงผลของ UI Panel ทั้งหมด
        if (miniGamePanel != null)
        {
            miniGamePanel.SetActive(false);
        }
    }
}