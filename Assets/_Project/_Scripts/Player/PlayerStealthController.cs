using System.Collections.Generic;
using UnityEngine;

public class PlayerStealthController : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;

    [Tooltip("ลาก Sprite Renderer ทุกชิ้นส่วนของตัวละครมาใส่ตรงนี้")]
    public List<SpriteRenderer> spriteRenderers;

    [Header("Stealth Settings")]
    public bool isStealthActive = false;

    [Tooltip("Layer ที่ศัตรูจะตีไม่โดน (ปกติใช้ 0 คือ Default)")]
    public int ghostLayer = 0;

    // --- ตัวแปรสำหรับจำค่าเดิม ---
    private int _originalLayer;
    private List<Sprite> _originalSprites = new List<Sprite>(); // เก็บภาพต้นฉบับไว้คืนค่า

    private void Start()
    {
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();

        // 1. จำ Layer เดิมของตัวละครไว้ (มักจะเป็น Layer "Player")
        _originalLayer = gameObject.layer;

        // 2. จำ Sprite เริ่มต้นของทุกชิ้นส่วนไว้
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
                _originalSprites.Add(sr.sprite);
            else
                _originalSprites.Add(null); // กันเหนียวกรณีช่องว่าง
        }
    }

    // ==========================================
    // คำสั่ง: เปิดการหายตัว
    // ==========================================
    [ContextMenu("Activate Stealth")]
    public void EnableStealth()
    {
        if (isStealthActive) return;
        isStealthActive = true;

        // 1. เปิดโหมดอมตะ
        if (playerHealth != null) playerHealth.isInvincible = true;

        // 2. เปลี่ยน Layer เพื่อหลบ Hitbox ของศัตรู
        gameObject.layer = ghostLayer;

        // 3. ถอด Sprite ออกให้กลายเป็นความว่างเปล่า
        foreach (var sr in spriteRenderers)
        {
            if (sr != null) sr.sprite = null;
        }

        Debug.Log("[Stealth] เข้าสู่โหมดหายตัว! อมตะและตีไม่โดน (ถอด Sprite)");
    }

    // ==========================================
    // คำสั่ง: ปิดการหายตัว (คืนร่าง)
    // ==========================================
    [ContextMenu("Deactivate Stealth")]
    public void DisableStealth()
    {
        if (!isStealthActive) return;
        isStealthActive = false;

        // 1. ปิดโหมดอมตะ
        if (playerHealth != null) playerHealth.isInvincible = false;

        // 2. คืน Layer กลับเป็น "Player" เหมือนเดิม ให้โดนตีได้
        gameObject.layer = _originalLayer;

        // 3. คืน Sprite ต้นฉบับกลับมา
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].sprite = _originalSprites[i];
            }
        }

        Debug.Log("[Stealth] คืนร่างปกติ! (คืนค่า Sprite)");
    }
}