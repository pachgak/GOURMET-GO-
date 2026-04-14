using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System;

public class MiniGameUIManager : MonoBehaviour
{
    public static MiniGameUIManager Instance { get; private set; }

    [Header("Reward Settings")]
    public Inventory.Model.InventorySO playerInventory; // ลาก Inventory หลักของ Player มาใส่

    [Header("UI Panels")]
    public GameObject readyPanel;
    public CanvasGroup blackOverlay; // พื้นหลังดำโปร่งแสง
    public GameObject startPanel;    // หน้าจอเริ่มเกม
    public GameObject endPanel;      // หน้าจอจบเกม

    [Header("UI Elements")]
    public TMP_Text countdownText; // ข้อความ 3 2 1
    public TMP_Text finishedText;  // <--- เพิ่มตัวแปรนี้สำหรับข้อความ "Finished!" หรือ "Done!"
    public Button startButton;            // ปุ่มเริ่ม
    public Button closeButton;            // ปุ่มปิด

    [Header("UI Controller Reference")]
    public newOpenUIController uiController; // ลาก GameObject ตัวเองที่มีสคริปต์นี้มาใส่

    [Header("Settings")]
    public float popDuration = 0.3f; // ความเร็วตอนข้อความเด้ง

    [Header("Reward UI")]
    public Image rewardCookImage;
    public TMP_Text rewardCookCountText;

    [Header("System")]
    private MiniGameBase _activeGame; // จำไว้ว่ากำลังเล่นเกมไหนอยู่

    public Action OnCloseMiniGame;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 1. เปลี่ยนหน้าที่ของปุ่มปิด ให้ไปสั่ง Global Manager แทน
        if (startButton != null) startButton.onClick.AddListener(StartGameSequence);
        if (closeButton != null) closeButton.onClick.AddListener(RequestClosePanel);

        ResetUI();
        readyPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // 2. ดักฟัง Event ว่า "ถ้าหน้าต่างนี้ถูกปิด (ไม่ว่าจะด้วยปุ่ม E, Esc หรือกดปุ่มกากบาท) ให้ล้างค่ามินิเกมด้วยนะ"
        if (uiController != null)
        {
            uiController.OnPanelClosed.AddListener(CleanUpMiniGame);
        }
    }

    private void OnDisable()
    {
        // เลิกดักฟังเมื่อ Object ถูกทำลาย
        if (uiController != null)
        {
            uiController.OnPanelClosed.RemoveListener(CleanUpMiniGame);
        }
    }


    private void HandleGameFinished(Sprite rewardSprite, int cookCount)
    {
        _activeGame.OnGameFinished -= HandleGameFinished; // ยกเลิกดักฟัง
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

        // รีเซ็ต finishedText ด้วย
        if (finishedText != null)
        {
            finishedText.gameObject.SetActive(false);
            Color fc = finishedText.color;
            fc.a = 1f;
            finishedText.color = fc;
        }
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

        // 2. เฟดพื้นหลังดำหายไป และบังคับให้ Coroutine "หยุดรอจนกว่าจะเฟดเสร็จ"
        yield return blackOverlay.DOFade(0f, 0.5f).WaitForCompletion();

        // ปิด Object เมื่อเฟดเสร็จ เพื่อไม่ให้มันบังการกดฟันของเกม
        blackOverlay.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        // ! สั่งเริ่มเกมของจริง !
        _activeGame.StartGame();
    }

    // --- ลำดับตอนจบเกม ---
    private void ShowEndScreen(Sprite rewardSprite, int cookCount)
    {
        StartCoroutine(CountdownEndScreen(rewardSprite, cookCount));
    }

    private IEnumerator CountdownEndScreen(Sprite rewardSprite, int cookCount)
    {
        // 1. เด้งข้อความ Finished! ทันทีโดยไม่ต้องรอ
        if (finishedText != null)
        {
            finishedText.gameObject.SetActive(true);
            finishedText.transform.localScale = Vector3.zero;

            // Pop ข้อความขึ้นมา
            finishedText.transform.DOScale(Vector3.one, popDuration).SetEase(Ease.OutBack);

            // รอให้ผู้เล่นอ่านสัก 1 วินาที (ปรับเวลาได้ตามชอบ)
            yield return new WaitForSeconds(2f);

            // เฟดข้อความ Finished ทิ้งให้สวยงาม
            finishedText.DOFade(0f, 0.3f);
        }
        else
        {
            // ถ้าลืมใส่ Text ไว้ ก็ให้รอเฉยๆ แบบเดิมกัน Error
            yield return new WaitForSeconds(1.0f);
        }

        // เตรียมข้อมูลของรางวัล
        if (rewardCookImage != null && rewardSprite != null)
        {
            rewardCookImage.sprite = rewardSprite;
        }

        if (rewardCookCountText != null)
        {
            rewardCookCountText.text = (cookCount > 1) ? $"x{cookCount}" : "";
        }

        // 2. โชว์พื้นหลังดำและเฟดความมืดกลับมา
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.DOFade(1f, 0.5f);

        yield return new WaitForSeconds(0.75f);

        // 3. โชว์หน้าจอ End Panel และทำให้มันเด้งขึ้นมา
        endPanel.SetActive(true);
        endPanel.transform.localScale = Vector3.zero;
        endPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetDelay(0.3f);
    }

    public void OpenMiniGame(MiniGameBase gameToPlay)
    {
        _activeGame = gameToPlay; // จำเกมที่จะเล่น

        // ดักฟัง Event จบเกมจากเกมนั้น
        _activeGame.OnGameFinished += HandleGameFinished;

        // 1. สั่งให้เกมเป้าหมาย "เปิดหน้าต่าง UI ของตัวเอง"
        if (_activeGame != null)
        {
            _activeGame.ShowGameUI();
        }

        // 2. จัดระเบียบหน้าจอให้สะอาด
        ResetUI();

        readyPanel.SetActive(true);

        StartGameSequence();
    }

    //[ContextMenu("CloseMiniGame")]
    //// --- ตอนกดปุ่มปิดเกม ---
    //private void CloseMiniGame()
    //{
    //    Debug.Log("ปิดมินิเกม กลับหน้าหลัก หรือ ปิด UI นี้ทิ้ง");

    //    ResetUI(); // เคลียร์ค่าเผื่อเปิดรอบหน้า

    //    // 1. สั่งให้เกมที่เพิ่งเล่นจบ "ปิดหน้าต่าง UI ของตัวเอง" ลงไป
    //    if (_activeGame != null)
    //    {
    //        _activeGame.HideGameUI();
    //        _activeGame = null; // ล้างความจำว่าไม่ได้เล่นเกมไหนอยู่
    //    }

    //    readyPanel.SetActive(false);

    //    OnCloseMiniGame?.Invoke();
    //}

    // --- ฟังก์ชันสำหรับให้ปุ่มกากบาทเรียกใช้ ---
    private void RequestClosePanel()
    {
        // สั่งให้ Global Manager ปิดหน้าต่างบนสุด (ซึ่งก็คือตัวมันเอง)
        if (newOpenUIManager.instance != null)
        {
            newOpenUIManager.instance._CloseTopPanel();
        }
    }

    [ContextMenu("CleanUpMiniGame")]
    // --- เปลี่ยนชื่อจาก CloseMiniGame มาเป็น CleanUpMiniGame ให้สื่อความหมายว่าล้างกระดาน ---
    private void CleanUpMiniGame()
    {
        // 1. จัดการเรื่องไอเทมก่อนล้างค่าอื่นๆ
        ProcessItemRewards();

        Debug.Log("ล้างค่าและซ่อนมินิเกม (เพราะหน้าต่างถูกปิดแล้ว)");

        ResetUI(); // เคลียร์ค่าเผื่อเปิดรอบหน้า

        // สั่งให้เกมที่เพิ่งเล่นจบ "ปิดหน้าต่าง UI ของตัวเอง" ลงไป
        if (_activeGame != null)
        {
            _activeGame.HideGameUI();
            _activeGame = null; // ล้างความจำว่าไม่ได้เล่นเกมไหนอยู่
        }

        readyPanel.SetActive(false);

        OnCloseMiniGame?.Invoke(); // ตะโกนบอกคนอื่นเผื่อมีใครรอฟังอยู่

    }

    // --- Method ใหม่สำหรับจัดการรางวัลและคืนวัตถุดิบ ---
    private void ProcessItemRewards()
    {
        Debug.Log($"#ProcessItemRewards");
        // ตรวจสอบความพร้อมของข้อมูล
        if (_activeGame == null || _activeGame.currentRecipe == null || playerInventory == null)
        {
            Debug.Log($"#return {_activeGame == null} , {_activeGame.currentRecipe == null} , {playerInventory == null}");
            return;
        }

        Debug.Log($"#Do");
        var recipe = _activeGame.currentRecipe;
        int count = _activeGame.cookCount;

        // เช็คเงื่อนไข: ชนะ (คะแนนถึงเป้า) หรือ แพ้/ออกกลางคัน (คะแนนไม่ถึง)
        if (_activeGame.currentScore >= _activeGame.maxScore)
        {
            // --- แบบที่ 1: เล่นจบ (Success) -> มอบไอเทมผลลัพธ์ ---
            playerInventory.AddItem(recipe.resultItem, count);
            Debug.Log($"[Reward] Cooking Success! Received: {recipe.resultItem.ItemName} x{count}");
        }
        else
        {
            // --- แบบที่ 2: เล่นไม่จบ (Cancel/Exit) -> คืนวัตถุดิบตามจำนวนที่ใช้ไป ---
            Debug.Log($"[Refund] Cooking Incomplete. Returning ingredients for {count} sets.");

            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.item != null)
                {
                    // คืนของตามจำนวนที่คำนวณไว้ (จำนวนต่อชุด * จำนวนครั้งที่สั่งทำ)
                    int refundAmount = ingredient.quantity * count;
                    playerInventory.AddItem(ingredient.item, refundAmount);
                    Debug.Log($"[Refund] Returned: {ingredient.item.ItemName} x{refundAmount}");
                }
            }
        }
    }
}