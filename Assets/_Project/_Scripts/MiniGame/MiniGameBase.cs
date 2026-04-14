using UnityEngine;
using System;
using Inventory.Model;
using System.Collections.Generic;

// Abstract class หมายถึง คลาสนี้เป็นแค่ "แม่แบบ" เอาไปแปะใน Unity ตรงๆ ไม่ได้ ต้องให้ลูกสืบทอดเท่านั้น
public abstract class MiniGameBase : MonoBehaviour
{
    [Header("Base Game UI")]
    public GameObject gameplayPanel;

    [Header("Base Game Settings")]
    public int currentScore = 0;
    public int maxScore = 100;
    public bool isPlaying = false;
    public bool isReady = false;

    // --- Event พื้นฐานที่ทุกเกมต้องมี ---
    public Action<int> OnScoreUpdated;
    public Action<Sprite, int> OnGameFinished;

    // ตัวแปรเก็บของรางวัล
    [SerializeField] protected Sprite rewardSprite;
    protected int rewardCount;

    [HideInInspector] public CookingRecipeSO currentRecipe;
    [HideInInspector] public int cookCount; // เก็บจำนวนชิ้นไว้ด้วย

    // --- ฟังก์ชัน Setup พื้นฐาน ---
    // ใช้ virtual เพื่อให้คลาสลูกสามารถ "เขียนทับ (override)" เพื่อเพิ่มลูกเล่นเฉพาะตัวได้
    public virtual void SetupFromRecipe(CookingRecipeSO recipe, int targetMaxScore, int cookCount)
    {
        if (recipe == null) return;

        this.currentRecipe = recipe;
        this.cookCount = cookCount;

        maxScore = targetMaxScore;
        rewardCount = cookCount;
        currentScore = 0;

        if (recipe.resultItem != null)
        {
            rewardSprite = recipe.resultItem.ItemImage;
        }

        OnScoreUpdated?.Invoke(currentScore);
    }

    // บังคับให้คลาสลูกต้องมีฟังก์ชัน StartGame และ EndGame เป็นของตัวเอง
    public abstract void StartGame();
    public abstract void EndGame();

    public virtual void ShowGameUI()
    {
        if (gameplayPanel != null) gameplayPanel.SetActive(true);

        isReady = true;
    }

    public virtual void HideGameUI()
    {
        if (gameplayPanel != null) gameplayPanel.SetActive(false);

        isReady = false; // <--- 3. ปิดจอ ก็ห้ามควบคุม
        isPlaying = false; // กันเหนียว
    }
}