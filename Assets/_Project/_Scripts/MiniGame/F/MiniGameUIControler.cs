using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening; // ต้องมี DOTween ติดตั้งในโปรเจกต์แล้ว

public class MiniGameUIControler : MonoBehaviour
{
    [Header("UI Panels")]
    public CanvasGroup blackOverlay; // พื้นหลังดำโปร่งแสง
    public GameObject startPanel;    // หน้าจอเริ่มเกม
    public GameObject endPanel;      // หน้าจอจบเกม

    [Header("UI Elements")]
    public TMP_Text countdownText; // ข้อความ 3 2 1
    public Button startButton;            // ปุ่มเริ่ม
    public Button closeButton;            // ปุ่มปิด

    [Header("Settings")]
    public float popDuration = 0.3f; // ความเร็วตอนข้อความเด้ง

    private void Start()
    {
        // 1. ผูกปุ่มเข้ากับฟังก์ชัน
        if (startButton != null) startButton.onClick.AddListener(StartGameSequence);
        if (startButton != null) closeButton.onClick.AddListener(CloseMiniGame);

        // 2. ดักฟัง Event ตอนจบเกมจาก Manager
        if (MiniGameFManager.Instance != null)
        {
            MiniGameFManager.Instance.OnGameFinished += HeadleGameFinished;
        }

        // 3. ตั้งค่าเริ่มต้น (โชว์หน้า Start ซ่อนหน้าอื่นๆ)
        ResetUI();
    }

    private void OnDestroy()
    {
        if (MiniGameFManager.Instance != null)
        {
            MiniGameFManager.Instance.OnGameFinished -= HeadleGameFinished;
        }
    }

    private void HeadleGameFinished()
    {
        ShowEndScreen();
    }

    private void ResetUI()
    {
        blackOverlay.alpha = 1f;
        blackOverlay.gameObject.SetActive(true);

        startPanel.SetActive(true);
        endPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);

        // รีเซ็ตข้อความให้มองเห็น 100%
        Color c = countdownText.color;
        c.a = 1f;
        countdownText.color = c;
    }

    [ContextMenu("StartGameSequence")]
    // --- ลำดับการเริ่มเกม ---
    private void StartGameSequence()
    {
        startPanel.SetActive(false); // ซ่อนปุ่มเริ่ม
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
        // 2. เฟดพื้นหลังดำหายไป
        //yield return blackOverlay.DOFade(0f, 0.5f).OnComplete(() =>
        //{
        //    // ปิด Object เมื่อเฟดเสร็จ เพื่อไม่ให้มันบังการกดฟันของเกม
        //    blackOverlay.gameObject.SetActive(false);
        //    countdownText.gameObject.SetActive(false);

        //});

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
    private void ShowEndScreen()
    {
        StartCoroutine(CountdownEndScreen()); // เริ่มนับถอยหลัง
    }

    private IEnumerator CountdownEndScreen()
    {
        yield return new WaitForSeconds(1.5f);

        // โชว์พื้นหลังดำและเฟดความมืดกลับมา
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.DOFade(1f, 0.5f);

        yield return new WaitForSeconds(0.5f);

        // โชว์หน้าจอ End Panel และทำให้มันเด้งขึ้นมา
        endPanel.SetActive(true);
        endPanel.transform.localScale = Vector3.zero;
        endPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetDelay(0.3f);
    }

        [ContextMenu("CloseMiniGame")]
    // --- ตอนกดปุ่มปิดเกม ---
    private void CloseMiniGame()
    {
        Debug.Log("ปิดมินิเกม กลับหน้าหลัก หรือ ปิด UI นี้ทิ้ง");
        ResetUI();
        // ถ้าคุณมี SceneManager หรือระบบอื่นๆ สามารถเขียนโค้ดกลับหน้าหลักตรงนี้ได้เลยครับ
        // เช่น SceneManager.LoadScene("MainMenu");
        // หรือถ้าปิดแค่ UI นี้ทิ้งก็ gameObject.SetActive(false);
    }
}
