using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Inventory.Model;
using Inventory;
using DG.Tweening; // <--- อย่าลืม! ต้องมี DOTween

public class NPCQuestGiver : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject dialoguePanel;
    public TMP_Text questText;
    public TMP_Text actionText;
    public Button actionButton;

    [Header("Settings")]
    [Tooltip("ระยะที่สามารถกด E และปุ่มทำงานได้ (Interact Zone)")]
    public float interactRange = 3f;
    [Tooltip("ระยะที่หน้าต่างจะเริ่มหดเล็กลง (Buffer Zone)")]
    public float startShrinkRange = 4.5f;
    [Tooltip("ระยะห่างสูงสุดที่จะบังคับปิดหน้าต่าง UI (Close Zone)")]
    public float closeRange = 6.5f;
    [Tooltip("ขนาดของ UI ที่เล็กที่สุดก่อนจะถูกปิดทิ้ง (1 = 100%, 0.5 = 50%)")]
    public float minScale = 0.5f;

    [Header("Quest Data")]
    public List<QuestSO> mainQuestLine;

    private int currentQuestIndex = 0;
    private int currentStepIndex = 0;

    private GameObject _playerObj;
    private InventorySO _playerInventoryData;

    // --- สถานะเพื่อแยกการทำงาน ---
    private bool _isInInteractRange = false;
    private bool _isShrinking = false;
    private bool _isClosing = false;

    private float _inputCooldownTimer = 0f;
    private const float Deley_InputCooldownTimer = 0.1f;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnOpenDialogue;
    public UnityEngine.Events.UnityEvent OnCloseDialogue;

    private void Start()
    {
        _playerObj = GameObject.FindGameObjectWithTag("Player");

        if (_playerObj != null && _playerObj.TryGetComponent(out InventoryController invController))
        {
            _playerInventoryData = invController.InventoryData;
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        actionButton.onClick.AddListener(OnActionButtonPressed);
    }

    private void Update()
    {
        if (_inputCooldownTimer > 0f)
        {
            _inputCooldownTimer -= Time.deltaTime;
        }

        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            CheckDistanceAndHotKey();
        }
    }

    private void CheckDistanceAndHotKey()
    {
        if (_playerObj == null || _isClosing) return;

        float distance = Vector3.Distance(transform.position, _playerObj.transform.position);

        // ==========================================
        // 1. จัดการเรื่องระยะปุ่มกด (Interact Range)
        // ==========================================
        if (distance <= interactRange)
        {
            if (!_isInInteractRange)
            {
                _isInInteractRange = true;
                UpdateActionButtonState(); // เปิดปุ่มเมื่อกลับเข้ามาในระยะ
            }

            if (Input.GetKeyDown(KeyCode.E) && _inputCooldownTimer <= 0f)
            {
                if (actionButton.interactable)
                {
                    OnActionButtonPressed();
                }
            }
        }
        else
        {
            if (_isInInteractRange)
            {
                _isInInteractRange = false;
                actionButton.interactable = false; // ปิดปุ่มเมื่อออกนอกระยะกด
            }
        }

        // ==========================================
        // 2. จัดการเรื่องขนาดหน้าต่าง (Shrink Range & Close Range)
        // ==========================================
        if (distance <= startShrinkRange)
        {
            // ระยะนี้จอจะคงขนาด 100% ไว้เสมอ (รวมถึงตอนอยู่ในระยะ Interact ด้วย)
            if (_isShrinking)
            {
                _isShrinking = false;
                dialoguePanel.transform.DOKill();
                dialoguePanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            }
        }
        else if (distance < closeRange)
        {
            // กำลังเดินออกห่างเกิน startShrinkRange หน้าต่างจะค่อยๆ เล็กลง
            if (!_isShrinking)
            {
                _isShrinking = true;
                dialoguePanel.transform.DOKill(); // หยุดแอนิเมชันเด้งกลับ (ถ้ามี) เพื่อเตรียมหด
            }

            // คำนวณความเล็ก (Lerp) โดยให้เริ่มหดจากระยะ startShrinkRange
            float t = (distance - startShrinkRange) / (closeRange - startShrinkRange);
            float currentScale = Mathf.Lerp(1f, minScale, t);

            dialoguePanel.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
        else
        {
            // ออกนอกระยะปิด (ไกลเกิน closeRange)
            CloseDialogue();
        }
    }

    public void OpenDialogue()
    {
        if (dialoguePanel == null) return;

        dialoguePanel.SetActive(true);

        // รีเซ็ตสถานะทั้งหมดก่อนเปิด
        _isClosing = false;
        _isInInteractRange = false; // ให้ Update ไปคำนวณเอาเองในเฟรมถัดไป
        _isShrinking = false;

        // แอนิเมชันตอนเปิด Pop
        dialoguePanel.transform.DOKill();
        dialoguePanel.transform.localScale = Vector3.zero;
        dialoguePanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        if (_playerInventoryData != null)
        {
            _playerInventoryData.OnInventoryUpdated += UpdateDialogueUI;
        }

        UpdateDialogueUI();

        _inputCooldownTimer = Deley_InputCooldownTimer;

        OnOpenDialogue?.Invoke();
    }

    public void CloseDialogue()
    {
        if (dialoguePanel == null || _isClosing) return;

        _isClosing = true;

        if (_playerInventoryData != null)
        {
            _playerInventoryData.OnInventoryUpdated -= UpdateDialogueUI;
        }

        // รีเซ็ตให้สถานะทั้งหมดหยุดทำงาน
        _isInInteractRange = false;
        _isShrinking = false;
        actionButton.interactable = false;

        // แอนิเมชันตอนปิด ยุบหายไป
        dialoguePanel.transform.DOKill();
        dialoguePanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            dialoguePanel.SetActive(false);
            _isClosing = false;
            OnCloseDialogue?.Invoke();
        });
    }

    private void OnActionButtonPressed()
    {
        if (currentQuestIndex >= mainQuestLine.Count)
        {
            CloseDialogue();
            return;
        }

        QuestSO currentQuest = mainQuestLine[currentQuestIndex];

        if (currentStepIndex < currentQuest.dialogues.Count)
        {
            currentStepIndex++;
        }
        else
        {
            if (CheckAllRequirements(currentQuest.turnInStep))
            {
                foreach (var req in currentQuest.turnInStep.requirements) req.ConsumeRequirement(_playerObj);
                foreach (var reward in currentQuest.rewards) reward.GiveReward(_playerObj);

                currentQuestIndex++;
                currentStepIndex = 0;
            }
        }

        UpdateDialogueUI();
    }

    private void UpdateDialogueUI(Dictionary<int, InventoryItem> inventoryState = null)
    {
        if (currentQuestIndex >= mainQuestLine.Count)
        {
            questText.text = "ไม่มีเควสแล้ว";
            actionText.text = "ปิด";
            UpdateActionButtonState();
            return;
        }

        QuestSO currentQuest = mainQuestLine[currentQuestIndex];

        if (currentStepIndex < currentQuest.dialogues.Count)
        {
            QuestDialogueStep step = currentQuest.dialogues[currentStepIndex];

            string finalActionText = step.actionText;
            if (finalActionText.Contains("[CL]"))
            {
                finalActionText = finalActionText.Replace("[CL]", $"({currentStepIndex + 1}/{currentQuest.dialogues.Count})");
            }

            actionText.text = finalActionText;
            questText.text = step.questText;
        }
        else
        {
            QuestTurnInStep turnIn = currentQuest.turnInStep;
            actionText.text = turnIn.actionText;

            string finalQuestText = turnIn.questText;
            bool hasUsedTags = false;

            if (turnIn.requirements.Count > 0)
            {
                for (int i = 0; i < turnIn.requirements.Count; i++)
                {
                    string tag = $"[R{i}]";
                    if (finalQuestText.Contains(tag))
                    {
                        string progressText = turnIn.requirements[i].GetProgressText(_playerObj);
                        finalQuestText = finalQuestText.Replace(tag, progressText);
                        hasUsedTags = true;
                    }
                }

                if (!hasUsedTags)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(finalQuestText);
                    sb.AppendLine("\n<color=yellow>เงื่อนไข:</color>");
                    foreach (var req in turnIn.requirements)
                    {
                        sb.AppendLine($"- {req.GetProgressText(_playerObj)}");
                    }
                    finalQuestText = sb.ToString();
                }
            }

            questText.text = finalQuestText;
        }

        UpdateActionButtonState();
    }

    private void UpdateActionButtonState()
    {
        if (!_isInInteractRange)
        {
            actionButton.interactable = false;
            return;
        }

        if (currentQuestIndex >= mainQuestLine.Count)
        {
            actionButton.interactable = true;
            return;
        }

        QuestSO currentQuest = mainQuestLine[currentQuestIndex];
        if (currentStepIndex < currentQuest.dialogues.Count)
        {
            actionButton.interactable = true;
        }
        else
        {
            actionButton.interactable = CheckAllRequirements(currentQuest.turnInStep);
        }
    }

    private bool CheckAllRequirements(QuestTurnInStep turnIn)
    {
        foreach (var req in turnIn.requirements)
        {
            if (!req.IsMet(_playerObj)) return false;
        }
        return true;
    }
}