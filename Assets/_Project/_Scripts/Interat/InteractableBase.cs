using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableBase : MonoBehaviour
{
    [Header("Interact Message Prompt")]
    public string message;
    public Vector3 offSetInteractText;
    
    [Header("On Interact")]
    public UnityEvent OnInteract;

    [Header("Hold Interact")]
    public bool hasDuration;
    public float duration = 0f;
    public bool isResetTimer;
    public UnityEvent OnFinished;

    [Header("System")]
    [HideInInspector] public UnityEvent<float> Progress;
    [HideInInspector] public float timer;
    private bool onProgress;
    private bool onFinishedCall;

    private int _originalLayer;

    private void Awake()
    {
        // จำ Layer ตอนเริ่มเกมเอาไว้ (เช่น Layer 'Interactable')
        _originalLayer = gameObject.layer;
    }

    private void Update()
    {
        if (onProgress)
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                if (onFinishedCall) return;
                onFinishedCall = true;
                OnFinished?.Invoke();
                Progress?.Invoke(0f);
                EndInteract();
            }
            else
            {
                Progress?.Invoke(timer / duration);
            }
        }
    }
    public void Interact()
    {
        //interact
        OnInteract?.Invoke();
        //ani OnInteract

        if (hasDuration)
        {
            onProgress = true;
        }
    }

    public void InvokePlayCanMove()
    {
        //GameManager.instance.PlayeCanAction(true);
    }

    public void EndInteract()
    {
        onProgress = false;
        onFinishedCall = false;
        if (isResetTimer)
        {
            timer = 0f;
        }

        if (hasDuration)
        {
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                if(audioSource.loop) audioSource.Stop();
            }
        }
    }

    public void ReInteract()
    {
        gameObject.layer = 0;
        GetComponent<InteractableBase>().enabled = false;
    }

    // --- Helper สำหรับปิด Interact ชั่วคราว ---
    public void PauseInteract()
    {
        gameObject.layer = 0; // เปลี่ยนเป็น Default (OverlapBox จะมองไม่เห็นทันที)
        this.enabled = false;
    }

    // --- Helper สำหรับเปิด Interact กลับมา ---
    public void ResumeInteract()
    {
        gameObject.layer = _originalLayer; // คืนค่า Layer กลับมาเหมือนเดิม
        this.enabled = true;
    }

}
